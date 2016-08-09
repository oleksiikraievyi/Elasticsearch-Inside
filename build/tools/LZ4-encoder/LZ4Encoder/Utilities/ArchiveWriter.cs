using System.IO;
using System.Text;

namespace LZ4Encoder.Utilities
{
    internal class ArchiveWriter : BinaryWriter
    {
        public ArchiveWriter(Stream destinationStream, bool leaveOpen = false) : base(destinationStream, Encoding.UTF8, leaveOpen) { }

        public void AddFiles(DirectoryInfo sourceDirectory)
        {
            RecursiveAddFiles(sourceDirectory, sourceDirectory.FullName);
        }

        private void RecursiveAddFiles(DirectoryInfo sourceDirectory, string root)
        {
            foreach (var entry in sourceDirectory.GetFileSystemInfos())
            {
                var relativePath = entry.FullName.Replace(root, "").TrimStart('\\');

                var file = entry as FileInfo;
                if (file != null)
                {
                    using (var stream = file.OpenRead())
                        AddStream(relativePath, stream);

                    continue;
                }

                var directory = entry as DirectoryInfo;
                if (directory != null)
                    RecursiveAddFiles(directory, root);
            }
        }

        public void AddStream(string relativePath, Stream stream)
        {
            Write(relativePath.Length);
            Write(Encoding.UTF8.GetBytes(relativePath));
            Write((int)stream.Length);
            Flush();
            stream.CopyTo(BaseStream);
        }
    }
}
