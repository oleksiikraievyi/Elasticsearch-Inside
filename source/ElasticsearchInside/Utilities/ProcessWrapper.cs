using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ElasticsearchInside.Utilities
{
    public class ProcessWrapper : IDisposable
    {
        private Process _process;
        private readonly Action<string> _logger;
        private readonly ProcessStartInfo _processStartInfo;

        public ProcessWrapper(DirectoryInfo workingDirectory, string executable, string arguments, Action<string> logger, Action<ProcessStartInfo> modifyProcessStartInfo = null)
        {
            _processStartInfo = new ProcessStartInfo(executable)
            {
                UseShellExecute = false,
                Arguments = arguments,
                WorkingDirectory = workingDirectory.FullName,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.ASCII,
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                _processStartInfo.CreateNoWindow = true;

            modifyProcessStartInfo?.Invoke(_processStartInfo);

            _logger = logger;
        }

        public async Task Start(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.Register(Dispose);
            
            await Task.Run(() =>
            {
                _process = new Process {StartInfo = this._processStartInfo};
                _process.OutputDataReceived += (sender, e) => {_logger.Invoke(e.Data); };
                _process.ErrorDataReceived += (sender, e) => { _logger.Invoke(e.Data); };
                _process.EnableRaisingEvents = true;
                _process.Start();
                _process.BeginErrorReadLine();
                _process.BeginOutputReadLine();
            }, cancellationToken);
        }
        
        public void Stop()
        {
            _process.Kill();
            WaitForExit();
        }

        public void WaitForExit()
        {
            _process.WaitForExit();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_process == null)
                return;

            try
            {
                Stop();
            }
            catch (Exception ex)
            {
                _logger(ex.ToString());
            }
            _process.Dispose();
            _process = null;
        }

        ~ProcessWrapper()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task Restart()
        {
            Stop();
            await Start();
        }
    }
}
