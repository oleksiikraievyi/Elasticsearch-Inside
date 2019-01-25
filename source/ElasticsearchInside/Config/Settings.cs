using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticsearchInside.Config
{
    internal class Settings : ISettings
    {
        private static readonly Random Random = new Random();
        internal readonly DirectoryInfo RootFolder = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        public DirectoryInfo ElasticsearchHomePath => new DirectoryInfo(Path.Combine(RootFolder.FullName, "es"));
        public DirectoryInfo JvmPath => new DirectoryInfo(Path.Combine(RootFolder.FullName, "jre"));
        public DirectoryInfo ElasticsearchConfigPath => new DirectoryInfo(Path.Combine(ElasticsearchHomePath.FullName, "config"));
        public IDictionary<string, string> ElasticsearchParameters { get; } = new Dictionary<string, string>();
        public IList<string> JVMParameters { get; private set; } = new List<string>();
        public IList<string> LoggingConfig { get; } = new List<string>();
        public string ElasticsearchVersion { get; } = ReadVersion();
        
        private static string ReadVersion()
        {
            var parts = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion.Split('.', '-');
            return $"{parts[0]}.{parts[1]}.{parts[2]}";
        }

        public string BuildCommandline()
        {             
            return $"-Des.path.conf=\"config\" {string.Join(" ", JVMParameters)} -Des.path.home=\"{ElasticsearchHomePath.FullName}\" -cp \"lib/*\" \"org.elasticsearch.bootstrap.Elasticsearch\"";
        }

        public static async Task<Settings> LoadDefault(CancellationToken cancellationToken = default(CancellationToken))
        {
            var port = Random.Next(49152, 65535 + 1);

            var settings = new Settings
            {
                JVMParameters = await ReadJVMDefaults(cancellationToken)
            };

            settings.SetPort(port);
            settings.SetClustername($"cluster-es-{port}");
            settings.SetNodename($"node-es-{port}");

            settings.LoggingConfig.Add("logger.zen.name = org.elasticsearch.discovery.zen.UnicastZenPing");
            settings.LoggingConfig.Add("logger.zen.level = error");
            settings.LoggingConfig.Add("logger.zen2.name = org.elasticsearch.discovery.zen.ping.unicast.UnicastZenPing");
            settings.LoggingConfig.Add("logger.zen2.level = error");

            return settings;

        }

        private static async Task<IList<string>> ReadJVMDefaults(CancellationToken cancellationToken = default(CancellationToken))
        {
            IList<string> result = new List<string>();

            using (var stream = typeof(ISettings).Assembly.GetManifestResourceStream(typeof(ISettings), "jvm.options"))
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                {
                    var line = await reader.ReadLineAsync();

                    if (line == null)
                        continue;

                    if (line.StartsWith("#"))
                        continue;

                    result.Add(line);
                }
            }

            return result;
        }

        internal async Task WriteSettings()
        {
            var pluginDir = new DirectoryInfo(Path.Combine(ElasticsearchHomePath.FullName, "plugins"));
            if (!pluginDir.Exists)
                pluginDir.Create();

            WriteFiles();
            await WriteLoggingConfig();
            await WriteYaml();
        }

        internal async Task WriteYaml()
        {
            if (!ElasticsearchConfigPath.Exists)
                ElasticsearchConfigPath.Create();

            var file = new FileInfo(Path.Combine(ElasticsearchConfigPath.FullName, @"elasticsearch.yml"));
            if (file.Exists)
                file.Delete();

            using (var fileStream = file.OpenWrite())
            using (var writer = new StreamWriter(fileStream))
                foreach (var elasticsearchParameter in ElasticsearchParameters)
                    await writer.WriteLineAsync($"{elasticsearchParameter.Key}: {elasticsearchParameter.Value}");
        }

        internal async Task WriteLoggingConfig()
        {
            if (!ElasticsearchConfigPath.Exists)
                ElasticsearchConfigPath.Create();

            var file = new FileInfo(Path.Combine(ElasticsearchConfigPath.FullName, @"log4j2.properties"));

            using (var fileStream = file.Open(FileMode.Append, FileAccess.Write))
            using (var writer = new StreamWriter(fileStream))
                foreach (var logsetting in LoggingConfig)
                    await writer.WriteLineAsync(logsetting);
        }

        internal void WriteFiles()
        {
            foreach (var kvp in Files)
            {
                var destination = kvp.Key;
                var source = kvp.Value;

                if (!File.Exists(source))
                    throw new FileNotFoundException("Could not copy unexisting file", source);

                var destinationInfo = new FileInfo(Path.Combine(ElasticsearchConfigPath.FullName, destination));
                if (!destinationInfo.Directory.Exists)
                {
                    destinationInfo.Directory.Create();
                }

                File.Copy(source, destinationInfo.FullName);
            }
        }

        //internal async Task WriteFiles()
        //{
        //    foreach (var kvp in Files)
        //    {
        //        var destination = kvp.Key;
        //        var source = kvp.Value;

        //        if (!File.Exists(source))
        //            await Task.FromException(new FileNotFoundException("Could not copy unexisting file", source));

        //        var destinationInfo = new FileInfo(Path.Combine(ElasticsearchConfigPath.FullName, destination));
        //        if (!destinationInfo.Directory.Exists)
        //        {
        //            destinationInfo.Directory.Create();
        //        }

        //        await new Task(() => File.Copy(source, destinationInfo.FullName));
        //    }
        //}

        public ISettings EnableLogging(bool enable = true)
        {
            this.LoggingEnabled = enable;
            return this;
        }

        public bool LoggingEnabled { get; set; }

        public Action<string> Logger { get; private set; } = message => Trace.WriteLine(message);

        public IList<Plugin> Plugins { get; set; } = new List<Plugin>();
        public IDictionary<string, string> Files { get; set; } = new Dictionary<string, string>();

        public ISettings LogTo(Action<string> logger)
        {
            this.Logger = logger;
            return this;
        }

        public ISettings AddPlugin(Plugin plugin)
        {
            Plugins.Add(plugin);
            return this;
        }

        public ISettings AddFile(string destinationRelativeToConfigPath, string source)
        {
            Files.Add(destinationRelativeToConfigPath, source);
            return this;
        }

        public void Dispose()
        {
            RootFolder.Delete(true);
        }
    }
}
