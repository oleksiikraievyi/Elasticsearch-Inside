using System;
using System.Collections.Generic;

namespace ElasticsearchInside.Config
{
    public interface ISettings : IDisposable
    {
        IDictionary<string, string> ElasticsearchParameters { get; }

        IList<string> JVMParameters { get; }
        ISettings EnableLogging(bool enable = true);
        ISettings LogTo(Action<string> logger);

        ISettings AddPlugin(Plugin plugin);
    }
}
