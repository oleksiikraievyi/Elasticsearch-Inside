using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ElasticsearchInside.Config;
using Nest;
using NUnit.Framework;

namespace ElasticsearchInside.Tests
{
    [TestFixture]
    public class ElasticsearchTests
    {
        [Test]
        public async Task Can_start()
        {
            using (var elasticsearch = await new Elasticsearch(i => i.SetPort(4444).EnableLogging().SetElasticsearchStartTimeout(60)).Ready())
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
        public void Can_start_sync()
        {
            using (var elasticsearch = new Elasticsearch(i => i.SetPort(4444).EnableLogging().SetElasticsearchStartTimeout(60)).ReadySync())
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
        public async Task Can_insert_data()
        {
            using (var elasticsearch = await new Elasticsearch(i => i.SetPort(4444).EnableLogging().SetElasticsearchStartTimeout(60)).Ready())
            {
                ////Arrange
                var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));

                ////Act
                client.Index(new { id = "tester" }, i => i.Index("test-index").Type("test-type"));

                ////Assert
                var result = client.Get(DocumentPath<dynamic>.Id("tester"), i => i.Index("test-index").Type("test-type"));
                Assert.That(result, Is.Not.Null);
                Assert.That(result.Found);
            }
        }

        [Test]
        public async Task Can_change_configuration()
        {
            using (var elasticsearch = await new Elasticsearch(c => c.SetPort(4444).EnableLogging().LogTo(Console.WriteLine).SetElasticsearchStartTimeout(60)).Ready())
            {
                ////Arrange
                var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));

                ////Act
                var result = client.Ping();

                ////Assert
                Assert.That(result.IsValid);
                Assert.That(elasticsearch.Url.Port, Is.EqualTo(4444));
            }
        }
        
        [Test]
        public async Task Can_log_output()
        {
            var logged = false;
            using (var elasticsearch = new Elasticsearch(c => c.SetPort(4444).EnableLogging().LogTo(message => logged = true).SetElasticsearchStartTimeout(60)))
            {
                await elasticsearch.Ready();

                ////Assert
                Assert.That(logged);
            }
        }

        [Test]
        public async Task Can_install_plugin()
        {
            using (var elasticsearch = await new Elasticsearch(c => c.SetPort(4444).EnableLogging().AddPlugin(new Plugin("analysis-icu")).SetElasticsearchStartTimeout(60)).Ready())
            {
                ////Arrange
                var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));

                ////Act
                var result = await client.CatPluginsAsync();
                
                ////Assert
                Assert.That(result.Records.Count, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task Folder_is_removed_after_dispose()
        {
            ////Arrange
            Settings settings;

            ////Act
            using (var elasticsearch = await new Elasticsearch(c => c.SetPort(4444).EnableLogging().LogTo(Console.WriteLine).SetElasticsearchStartTimeout(60)).Ready())
                settings = (Settings)elasticsearch.Settings;


            ////Assert
            var folder = settings.RootFolder;
            folder.Refresh();

            Assert.That(!folder.Exists);
        }

        [Test]
        public void Has_specific_version()
        {
            using (var elasticsearch = new Elasticsearch(i => i.SetPort(4444).EnableLogging().SetElasticsearchStartTimeout(60)).ReadySync())
            {
                ////Arrange
                var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));

                ////Act
                var version = client.RootNodeInfo().Version.Number;

                ////Assert
                Assert.That(version, Is.EqualTo("7.1.0"));
            }
        }

        [Test]
        public async Task Can_add_file()
        {
            const string relativeDestination = "test/copied.txt";
            const string relativeSource = "TestFiles/testfile.txt";
            var sourceFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var sourcePath = Path.Combine(sourceFolder, relativeSource);
            using (var elasticsearch = await new Elasticsearch(c => c.SetPort(4444).EnableLogging().AddFile(relativeDestination, sourcePath).SetElasticsearchStartTimeout(60)).Ready())
            {
                var settings = (Settings)elasticsearch.Settings;
                var folder = settings.ElasticsearchConfigPath;
                var expected = new FileInfo(Path.Combine(folder.FullName, relativeDestination));
                Assert.That(expected.Exists);
            }
        }

        [Test]
        public async Task Can_spinup_with_plugin_and_cleanup_twice_without_problems()
        {
            Settings settings;

            var plugin = new Plugin("analysis-icu");
            using (var elasticsearch = await new Elasticsearch(c => c.SetPort(4444).EnableLogging().LogTo(Console.WriteLine).AddPlugin(plugin)).Ready())
            {
                settings = (Settings)elasticsearch.Settings;
                var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));
                var result = await client.CatPluginsAsync();
                Assert.That(result.Records.Count, Is.EqualTo(1));
            }

            var folder = settings.RootFolder;
            folder.Refresh();
            Assert.That(!folder.Exists);

            using (var elasticsearch = await new Elasticsearch(c => c.SetPort(4444).EnableLogging().LogTo(Console.WriteLine).AddPlugin(plugin)).Ready())
            {
                settings = (Settings)elasticsearch.Settings;
                var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));
                var result = await client.CatPluginsAsync();
                Assert.That(result.Records.Count, Is.EqualTo(1));
            }

            folder = settings.RootFolder;
            folder.Refresh();
            Assert.That(!folder.Exists);
        }

    }
}
