using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ElasticsearchInside.Utilities.Archive
{
    internal class ArchiveReader : BinaryReader
    {
        public ArchiveReader(Stream input, bool leaveOpen = false) : base(input, Encoding.UTF8, leaveOpen) { }

        internal string ReadFileName()
        {
            var filenameLength = ReadInt32();
            return Encoding.UTF8.GetString(ReadBytes(filenameLength));
        }

        internal int ReadStreamLength()
        {
            return ReadInt32();
        }

        public void ExtractToDirectory(DirectoryInfo target)
        {
            try
            {
                while (true)
                {
                    var filename = ReadFileName();
                    var fullPath = new FileInfo(Path.Combine(target.FullName, filename));

                    EnsurePath(fullPath.Directory);

                    using (var destination = fullPath.OpenWrite())
                        ExtractToStream(destination);
                }
            }
            catch (EndOfStreamException)
            {
                
            }
           
        }

        internal void ExtractToStream(Stream destination)
        {
            var length = ReadInt32();
            var buffer = new byte[81920];
            int count;

            var readLength = Math.Min(buffer.Length, length);

            var total = 0;
            while ((readLength > 0) && (count = Read(buffer, 0, readLength)) != 0)
            {
                total += count;
                if (total + buffer.Length > length)
                    readLength = length - total;

                destination.Write(buffer, 0, count);
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
