using System;
using System.IO;
using System.IO.Compression;
using K4os.Compression.LZ4.Streams;
using LZ4Encoder.Utilities;

namespace LZ4Encoder
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || args.Length != 2)
                Console.WriteLine("Usage: <source folder> <destinaion file>");

            var source = new DirectoryInfo(args[0]);

            if (!source.Exists)
                throw new ApplicationException("Source folder does not exist");


            var destination = new FileInfo(args[1]);
            if (destination.Exists)
                throw new ApplicationException("Destination already exists");

            /*
             using (var source = File.OpenRead(filename))
using (var target = LZ4Stream.Encode(File.Create(filename + ".lz4")))
{
    source.CopyTo(target);
}
             */

            // mem-stream needs to be here for this to work?
            using (var destinationStream = destination.OpenWrite())
            {
                using (var memStream = new MemoryStream())
                {
                    //using (var lz4Stream = new LZ4Stream(destinationStream, CompressionMode.Compress, true, true))
                    using (var lz4Stream = LZ4Stream.Encode(destinationStream))
                    {
                        using (var archiveWriter = new ArchiveWriter(memStream, true))
                            archiveWriter.AddFiles(source);

                        memStream.Position = 0;
                        memStream.CopyTo(lz4Stream);
                    }
                }
            }
        }

        private static void AddFilesToArchive(DirectoryInfo source, string path, ZipArchive zipArchive)
        {
            foreach (var s in source.GetFiles())
            {
                var entry = zipArchive.CreateEntry(Path.Combine(path, s.Name), CompressionLevel.NoCompression);
                using (var zipStream = entry.Open())
                using (var fileStream = s.OpenRead())
                    fileStream.CopyTo(zipStream);
            }

            foreach (var s in source.GetDirectories())
                AddFilesToArchive(s, Path.Combine(path, s.Name), zipArchive);
        }
    }
}
