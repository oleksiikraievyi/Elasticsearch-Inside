using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ElasticsearchInside.Utilities.Archive
{
    internal class ArchiveReader : BinaryReader
    {
        public ArchiveReader(Stream input, bool leaveOpen = false) : base(input, Encoding.UTF8, leaveOpen) { }

        internal string ReadFileName()
        {
            var filenameLength = ReadInt32();
            var filestring = Encoding.UTF8.GetString(ReadBytes(filenameLength));
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? filestring : filestring.Replace("\\", "/");
        }

        internal int ReadStreamLength()
        {
            return ReadInt32();
        }

        public async Task ExtractToDirectory(DirectoryInfo target, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var filename = ReadFileName();
                    var fullPath = new FileInfo(Path.Combine(target.FullName, filename));

                    EnsurePath(fullPath.Directory);

                    using (var destination = fullPath.OpenWrite())
                        await ExtractToStream(destination, cancellationToken);
                }
            }
            catch (EndOfStreamException)
            {
                
            }
           
        }

        internal async Task ExtractToStream(Stream destination, CancellationToken cancellationToken = default(CancellationToken))
        {
            var length = ReadInt32();
            var buffer = new byte[81920];
            int count;

            var readLength = Math.Min(buffer.Length, length);

            var total = 0;
            while ((readLength > 0) && (count = Read(buffer, 0, readLength)) != 0 && !cancellationToken.IsCancellationRequested)
            {
                total += count;
                if (total + buffer.Length > length)
                    readLength = length - total;

                await destination.WriteAsync(buffer, 0, count, cancellationToken);
            }
        }

        private static void EnsurePath(DirectoryInfo directory)
        {
            if (directory.Exists)
                return;

            EnsurePath(directory.Parent);
            directory.Create();
        }
    }
}
