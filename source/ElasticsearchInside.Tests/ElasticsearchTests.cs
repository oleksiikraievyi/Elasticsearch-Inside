using System.IO;
using LZ4PCL;
using Nest;
using NUnit.Framework;

namespace ElasticsearchInside.Tests
{
    [TestFixture]
    public class ElasticsearchTests
    {
        [Test]
        public void Can_start()
        { 
            using (var elasticsearch = new Elasticsearch())
            {
                ////Arrange
                var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));

                ////Act
                var result = client.Ping();

                ////Assert
                Assert.That(result.IsValid);
            }
        }

        
        [Test]
        public void Can_insert_data()
        {
            using (var elasticsearch = new Elasticsearch())
            {
                ////Arrange
                var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));

                ////Act
                client.Index(new { id = "tester"}, i => i.Index("test-index"));

                ////Assert
                var result = client.Get<dynamic>("tester", "test-index");
                Assert.That(result, Is.Not.Null);
            }
        }

        [Test]
        public void Can_change_configuration()
        {
            using (var elasticsearch = new Elasticsearch(c => c
                .Port(444)
                .EnableLogging()
                .AddArgument("-Des.script.engine.groovy.file.aggs=on")))
            {
                ////Arrange
                var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));

                ////Act
                var result = client.Ping();

                ////Assert
                Assert.That(result.IsValid);
                Assert.That(elasticsearch.Url.Port, Is.EqualTo(444));
            }
        }

        [Test]
        public void Can_log_output()
        {
            var logged = false;
            using (new Elasticsearch(c => c.EnableLogging().LogTo((f, a) => logged = true)))
            {
                ////Assert
                Assert.That(logged);
            }
        }

        [Test, Ignore("Will be moved to external tool later")]
        public void Compress()
        {
            using (var source = File.OpenRead(@"z:\data\projects\Embedded Elasticsearch\Embedded Elasticsearch\Executables\elasticsearch.zip"))
            using (var dest = File.OpenWrite(@"z:\data\projects\Embedded Elasticsearch\Embedded Elasticsearch\Executables\elasticsearch.lz4"))
            using (var compressed = new LZ4Stream(dest, CompressionMode.Compress, true, true))
                source.CopyTo(compressed);
        }

    }
}
