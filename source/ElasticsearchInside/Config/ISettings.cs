using System;
using System.Collections.Generic;
using System.IO;

namespace ElasticsearchInside.Config
{
    public interface ISettings : IDisposable
    {
        IDictionary<string, string> ElasticsearchParameters { get; }

        IList<string> JVMParameters { get; }
        ISettings EnableLogging(bool enable = true);
        ISettings SetElasticsearchStartTimeout(int timeout);
        ISettings LogTo(Action<string> logger);

        ISettings AddPlugin(Plugin plugin);
        ISettings AddFile(string destinationRelativeToConfigPath, string source);
    }
}
