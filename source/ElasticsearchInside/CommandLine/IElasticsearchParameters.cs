using ElasticsearchInside.Configuration;
using System;

namespace ElasticsearchInside.CommandLine
{
    public interface IElasticsearchParameters
    {
        IElasticsearchParameters HeapSize(int initialHeapsizeMB = 128, int maximumHeapsizeMB = 128);

        IElasticsearchParameters Port(int port);

        IElasticsearchParameters EnableLogging(bool enable = true);

        IElasticsearchParameters LogTo(Action<string, object[]> logger);

        IElasticsearchParameters AddArgument(string argument);

        IElasticsearchParameters AddPlugin(Plugin plugin);

    }
}