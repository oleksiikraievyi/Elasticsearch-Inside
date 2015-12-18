using System.IO;
using ElasticsearchInside.Utilities.Archive;
using NUnit.Framework;

namespace ElasticsearchInside.Tests
{
    [TestFixture]
    public class FileTests
    {
        [Test]
        public void Can_read_write()
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new ArchiveWriter(stream, true))
                    writer.AddFiles(new DirectoryInfo(@"c:\source"));

                stream.Position = 0;

                using (var reader = new ArchiveReader(stream))
                    reader.ExtractToDirectory(new DirectoryInfo(@"c:\test"));
            }
        }
    }
}
