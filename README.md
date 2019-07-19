![](https://raw.githubusercontent.com/poulfoged/elasticsearch-inside/master/logo.png) &nbsp; ![](https://ci.appveyor.com/api/projects/status/prwp3j290469ntpb/branch/master?svg=true) &nbsp; ![](http://img.shields.io/nuget/v/elasticsearch-inside.svg?style=flat)

# Elasticsearch Inside

Many thanks to [DJPorv](https://github.com/DJPorv) who created the first version of this.

This is a fully embedded version of [Elasticsearch][Elasticsearch] for integration tests. When the instance is created both the JVM and Elasticsearch itself is extracted to a temporary location *(2-3 seconds in my tests)* and started *(5-6 seconds in my tests)*. Once disposed everything is removed again.

The instance will be started on a random port - full url available as `Url` property.

## How to
To use Elasticsearch in integration tests first create a new instance of the Elasticsearch class. Right after instantiation the Elasticsearch server is started asynchronously and you can continue to do other work. Once you need the instance to be ready simply `await` the blocking function `Ready()`.

In these tests I'm using the excellent client [Elasticsearch-NEST][nest].

```c#
using (var elasticsearch = new Elasticsearch())
{
    ////Arrange
    await elasticsearch.Ready();
    var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));

    ////Act
    var result = client.Ping();

    ////Assert
    Assert.That(result.IsValid);
}
```

Note that if you are not using async you can use the sync version of ready:

```c#
using (var elasticsearch = new Elasticsearch(i => i.EnableLogging()).ReadySync())
{
    ////Arrange
    var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));

    ////Act
    var result = client.Ping();

    ////Assert
    Assert.That(result.IsValid);
}
```

All settings can be modified via the constructor via these two collections:

* ElasticsearchParameters (writen to elasticsearch.yml)
* JVMParameters (written to jvm.options)

Then there is a few helper funtions for to make it easier to work with these collections:
`GetPort()`, `SetPort()`, `SetClustername()`, etc.

In this example I change the port for the Elasticsearch startup:

Also note that since `Ready()` returns the instance, it can be awaited directly.

```c#
using (var elasticsearch = await new Elasticsearch(c => c.SetPort(444).SetNodename("Homer")).Ready())
{
    ////Arrange
    var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));

    ////Act
    var result = client.Ping();

    ////Assert
    Assert.That(result.IsValid);
}
```

Plugins can be added during initialization. Elasticsearch is restarted after each plugin is installed.

```c#
using (var elasticsearch = await new Elasticsearch(c => c.AddPlugin(new Plugin("plugin"))).Ready())
{
    ////Arrange
    var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));

    ////Act
    var result = client.CatPlugins();

    int pluginCount = 0;
    foreach (CatPluginsRecord plugin in result.Records)
    {
        pluginCount++;
    }

    ////Assert
    Assert.That(result.IsValid);
    Assert.AreEqual(1, pluginCount);
}
```

By default nothing is being logged, logging to trace can be enabled with `EnableLogging()` and can be customized to log to somewhere else with the `LogTo()` statement:

Console output is by default being written to `Trace.Write` but can be customized by providing a custom logging-lambda:

```c#
using (new Elasticsearch(c => c.EnableLogging().LogTo(Console.WriteLine)))
{

}
```

## Install

Simply add the NuGet package:

`PM> Install-Package elasticsearch-inside`

## Requirements

This project is compiled using .net standard 2.0, which means this project will work with .NET Framework 4.6.1 (or later) or .net core 2.0 (or later).

## License

Elasticsearch Inside is under the MIT license.

[Elasticsearch]: https://www.elastic.co/products/elasticsearch  "Elasticsearch"
[nest]: https://github.com/elastic/elasticsearch-net  "Elasticsearch.Net & NEST"
