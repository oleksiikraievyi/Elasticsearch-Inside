﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LZ4PCL;
using CompressionMode = LZ4PCL.CompressionMode;

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

            using (var destinationStream = destination.OpenWrite())
            using (var memStream = new MemoryStream())
            using (var lz4Stream = new LZ4Stream(destinationStream, CompressionMode.Compress, true, true))
            {
                using (var zipArchive = new ZipArchive(memStream, ZipArchiveMode.Create, true))
                {
                    AddFilesToArchive(source, "", zipArchive);
                    
                }
                memStream.Position = 0;
                memStream.CopyTo(lz4Stream);

            }
            
            
            //zipArchive.CreateEntry()




            //using (var source = File.OpenRead(@"z:\data\projects\Embedded Elasticsearch\Embedded Elasticsearch\Executables\elasticsearch.zip"))
            //using (var dest = File.OpenWrite(@"z:\data\projects\Embedded Elasticsearch\Embedded Elasticsearch\Executables\elasticsearch.lz4"))
            //
            //    source.CopyTo(compressed);
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
