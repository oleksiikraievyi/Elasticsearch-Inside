using System.IO;
using System.Text;
using ElasticsearchInside.Utilities.Archive;
using NUnit.Framework;

namespace ElasticsearchInside.Tests.Utilities.Archive
{
    [TestFixture]
    public class ArchiveReaderWriterTests
    {
        [Test]
        public void Can_read_write_stream()
        {
            ////Arrange
            using (var stream = new MemoryStream())
            {
                using (var archiveWriter = new ArchiveWriter(stream, true))
                    archiveWriter.AddStream("test.txt", new MemoryStream(Encoding.UTF8.GetBytes("hello world")));

                stream.Position = 0;

                using (var archiveReader = new ArchiveReader(stream))
                using (var destinationStream = new MemoryStream())
                {
                    ////Act
                    var file = archiveReader.ReadFileName();
                    archiveReader.ExtractToStream(destinationStream);

                    ////Assert
                    Assert.That(file, Is.EqualTo("test.txt"));
                    Assert.That(Encoding.UTF8.GetString(destinationStream.ToArray()), Is.EqualTo("hello world"));
                }
            }
        }

        [Test]
        public void Can_read_write_binary_to_stream()
        {
            ////Arrange
            using (var stream = new MemoryStream())
            {
                using (var archiveWriter = new ArchiveWriter(stream, true))
                    archiveWriter.AddStream("test.bin", new MemoryStream(new byte[] { 1, 2, 3 }));

                stream.Position = 0;

                using (var archiveReader = new ArchiveReader(stream))
                using (var destinationStream = new MemoryStream())
                {
                    ////Act
                    var file = archiveReader.ReadFileName();
                    archiveReader.ExtractToStream(destinationStream);

                    ////Assert
                    Assert.That(file, Is.EqualTo("test.bin"));
                    Assert.AreEqual(destinationStream.ToArray(), new byte[] { 1, 2, 3 });
                }
            }
        }

        [Test]
        public void Can_read_write_nested_named_stream()
        {
            ////Arrange
            using (var stream = new MemoryStream())
            {
                using (var archiveWriter = new ArchiveWriter(stream, true))
                    archiveWriter
                        .AddStream("test.txt", new MemoryStream(Encoding.UTF8.GetBytes("hello world")))
                        .AddStream("this/is/nested/test.txt", new MemoryStream(Encoding.UTF8.GetBytes("hello world")));

                stream.Position = 0;

                using (var archiveReader = new ArchiveReader(stream))
                using (var destinationStream = new MemoryStream())
                {
                    // move to second file
                    archiveReader.ReadFileName();
                    archiveReader.ExtractToStream(Stream.Null);

                    ////Act
                    var file = archiveReader.ReadFileName();
                    archiveReader.ExtractToStream(destinationStream);


                    ////Assert
                    Assert.That(file, Is.EqualTo("this/is/nested/test.txt"));
                    Assert.That(Encoding.UTF8.GetString(destinationStream.ToArray()), Is.EqualTo("hello world"));
                }
            }
        }
    }
}
