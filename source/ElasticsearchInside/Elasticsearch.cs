using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ElasticsearchInside.Config;
using ElasticsearchInside.Executables;
using ElasticsearchInside.Utilities;
using ElasticsearchInside.Utilities.Archive;
using K4os.Compression.LZ4.Streams;


namespace ElasticsearchInside
{
    /// <summary>
    /// Starts a elasticsearch instance in the background, use Ready() to wait for start to complete
    /// </summary>
    public class Elasticsearch : IDisposable
    {
        private bool _disposed;
        private readonly Stopwatch _stopwatch;
        private ProcessWrapper _processWrapper;
        private readonly Task _startupTask;
        private Settings _settings;

        static Elasticsearch()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                if (e.Name != "LZ4PCL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")
                    return null;

                using (var memStream = new MemoryStream())
                {
                    using (var stream = typeof(Elasticsearch).Assembly.GetManifestResourceStream(typeof(RessourceTarget), "LZ4PCL.dll"))
                        stream.CopyTo(memStream);

                    return Assembly.Load(memStream.GetBuffer());
                }
            };
        }

        public Uri Url => _settings.GetUrl();
        public ISettings Settings => _settings;
        public async Task<Elasticsearch> Ready()
        {
            await _startupTask;
            return this;
        }

        public Elasticsearch ReadySync()
        {
            return Ready().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private void Info(string message)
        {
            if (_settings == null || !_settings.LoggingEnabled)
                return;

            _settings.Logger(message);
        }

        public Elasticsearch(Func<ISettings, ISettings> configurationAction = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            _stopwatch = Stopwatch.StartNew();
            _startupTask = SetupAndStart(configurationAction, cancellationToken);
        }

        private async Task SetupAndStart(Func<ISettings, ISettings> configurationAction, CancellationToken cancellationToken = default(CancellationToken))
        {
            _settings = await Config.Settings.LoadDefault(cancellationToken).ConfigureAwait(false);
            configurationAction?.Invoke(_settings);

            Info($"Starting Elasticsearch {_settings.ElasticsearchVersion}");
            
            await SetupEnvironment(cancellationToken).ConfigureAwait(false);
            Info($"Environment ready after {_stopwatch.Elapsed.TotalSeconds} seconds");
            await StartProcess(cancellationToken).ConfigureAwait(false);
            Info("Process started");
            await WaitForOk(_settings.ElasticsearchStartTimeout, cancellationToken).ConfigureAwait(false);
            Info("We got ok");
            await InstallPlugins(cancellationToken).ConfigureAwait(false);
            Info("Installed plugins");
        }

        private async Task InstallPlugins(CancellationToken cancellationToken = default(CancellationToken))
        {
            foreach (var plugin in _settings.Plugins)
            {
                Info($"Installing plugin {plugin.Name}...");
                var pluginInstallCommand = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)? 
                    Path.Combine(_settings.ElasticsearchHomePath.FullName,"bin\\elasticsearch-plugin.bat")
                    : "bash";
                using (var process = new ProcessWrapper(
                    new DirectoryInfo(Path.Combine(_settings.ElasticsearchHomePath.FullName, "bin")),
                    pluginInstallCommand,
                    plugin.GetInstallCommand(),
                    Info,
                    startInfo =>
                    {
                        if (startInfo.EnvironmentVariables.ContainsKey("JAVA_HOME"))
                        {
                            Info("Removing old JAVA_HOME and replacing with bundled JRE.");
                            startInfo.EnvironmentVariables.Remove("JAVA_HOME");
                        }
                        startInfo.EnvironmentVariables.Add("JAVA_HOME", _settings.JvmPath.FullName);
                    }
                ))
                {
                    await process.Start(cancellationToken).ConfigureAwait(false);
                    Info($"Waiting for plugin {plugin.Name} install...");
                    process.WaitForExit();
                }
                Info($"Plugin {plugin.Name} installed.");
                await Restart().ConfigureAwait(false);
            }
        }

        private async Task SetupEnvironment(CancellationToken cancellationToken = default(CancellationToken))
        {
            var jreTask = Task.Run(() => ExtractEmbeddedLz4Stream("jre.lz4", _settings.JvmPath, cancellationToken), cancellationToken);
            var esTask = Task.Run(() => ExtractEmbeddedLz4Stream("elasticsearch.lz4", _settings.ElasticsearchHomePath, cancellationToken), cancellationToken)
                .ContinueWith(_ => _settings.WriteSettings(), cancellationToken);

            await Task.WhenAll(jreTask, esTask).ConfigureAwait(false);
        }


        private async Task WaitForOk(int timeout, CancellationToken cancellationToken = default(CancellationToken))
        {
            var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeout));
            var linked = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, cancellationToken);
            
            var statusUrl = new UriBuilder(_settings.GetUrl())
            {
                Path = "_cluster/health",
                Query = "wait_for_status=yellow"
            }.Uri;

            using (var client = new HttpClient())
            {
                var statusCode = (HttpStatusCode)0;
                do
                {
                    try
                    {
                        var response = await client.GetAsync(statusUrl, linked.Token);
                        statusCode = response.StatusCode;
                    }
                    catch (HttpRequestException) { }
                    catch (TaskCanceledException ex) {
                        throw new TimeoutWaitingForElasticsearchStatusException(ex); 
                    }
                    await Task.Delay(100, linked.Token).ConfigureAwait(false);

                } while (statusCode != HttpStatusCode.OK && !linked.IsCancellationRequested);
            }
            
            _stopwatch.Stop();
            Info($"Started in {_stopwatch.Elapsed.TotalSeconds} seconds");
        }

        private async Task StartProcess(CancellationToken cancellationToken = default(CancellationToken))
        {
            var args = _settings.BuildCommandline();

            var javaExecutable = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? Path.Combine(_settings.JvmPath.FullName, "bin/java.exe")
                : "java";
            _processWrapper = new ProcessWrapper(_settings.ElasticsearchHomePath, javaExecutable, args, Info);
            await _processWrapper.Start(cancellationToken).ConfigureAwait(false);
        }
        
        public async Task Restart()
        {
            _processWrapper.Stop();

            await StartProcess().ConfigureAwait(false);
            await WaitForOk(_settings.ElasticsearchStartTimeout).ConfigureAwait(false);
        }

        private async Task ExtractEmbeddedLz4Stream(string name, DirectoryInfo destination, CancellationToken cancellationToken = default(CancellationToken))
        {
            var started = Stopwatch.StartNew();

            using (var stream = GetType().Assembly.GetManifestResourceStream(typeof(RessourceTarget), name))
            {
                //using (var decompresStream = new LZ4Stream(stream, LZ4StreamMode.Decompress))
                //{
                //    using (var archiveReader = new ArchiveReader(decompresStream))
                //    {
                //        await archiveReader.ExtractToDirectory(destination, cancellationToken).ConfigureAwait(false);
                //    }
                //}
                using (LZ4DecoderStream decompresStream = LZ4Stream.Decode(stream))
                {
                    using (var archiveReader = new ArchiveReader(decompresStream))
                    {
                        await archiveReader.ExtractToDirectory(destination, cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            Info($"Extracted {name.Split('.')[0]} in {started.Elapsed.TotalSeconds:#0.##} seconds");
        }


        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
            try
            {
                _processWrapper.Dispose();
                _settings.Dispose();
            }
            catch (Exception ex)
            {
                Info(ex.ToString());
            }
            _disposed = true;

        }

        ~Elasticsearch()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
