![](https://raw.githubusercontent.com/poulfoged/elasticsearch-inside/master/logo.png) &nbsp; ![](https://ci.appveyor.com/api/projects/status/prwp3j290469ntpb/branch/master?svg=true) &nbsp; ![](http://img.shields.io/nuget/v/elasticsearch-inside.svg?style=flat)
#Elasticsearch Inside  

Many thanks to [DJPorv](https://github.com/DJPorv) who created the first version of this.

This is a fully embedded version of [Elasticsearch][Elasticsearch] for integration tests. When the instance is created both the jvm and elasticsearch itself is extracted to a temporary location (2-3 seconds in my tests) and started (5-6 seconds in my tests). Once disposed everything is removed again.

The instance will be started on a random port - full url available as Url property.

## How to
To use elasticsearch in integration tests simply instantiate it - in these tests I'm using the excellent client [Elasticsearch-NEST][nest]:

```c#
using (var elasticsearch = new Elasticsearch())
{
    ////Arrange
    var client = new ElasticClient(new ConnectionSettings(elasticsearch.Url));

    ////Act
    var result = client.Ping();

    ////Assert
    Assert.That(result.IsValid);
}

```

A few settings can be modified via the constructor. In this example I change the port and add a custom commandline argument for the elasticsearch startup:

```c#
using (var elasticsearch = new Elasticsearch(c => c
    .Port(444)
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

```

Plugins can be added during initialization. Elasticsearch is restarted after each plugin is installed. In this example I add the head plugin.
```c#
string pluginName = "mobz/elasticsearch-head";
using (var elasticsearch = new Elasticsearch(c => c.AddPlugin(new Plugin(pluginName))))
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

By default nothing is being logged, logging to trace can be enabled with EnableLogging() and can be customized to log to somewhere else with the LogTo() statement:

Console output is by default being written to Trace.Write but can be customized by providing a custom logging-lambda:

```c#
using (new Elasticsearch(c => c.EnableLogging().LogTo(Console.WriteLine)))
{
                
}
```


## Install

Simply add the Nuget package:

`PM> Install-Package elasticsearch-inside`

## Requirements

You'll need .NET Framework 4.5.1 or later to use the precompiled binaries.

## License

Elasticsearch Inside is under the MIT license. 

[Elasticsearch]: https://www.elastic.co/products/elasticsearch  "Elasticsearch"
[nest]: https://github.com/elastic/elasticsearch-net  "Elasticsearch.Net & NEST"



