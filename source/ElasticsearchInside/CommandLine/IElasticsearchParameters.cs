using System;

namespace ElasticsearchInside.CommandLine
{
    public interface IElasticsearchParameters
    {
        IElasticsearchParameters HeapSize(int heapsizeInBytes);

        IElasticsearchParameters Port(int port);

        IElasticsearchParameters EnableLogging(bool enable = true);

        IElasticsearchParameters LogTo(Action<string, object[]> logger);

        IElasticsearchParameters AddArgument(string argument);

    }
}