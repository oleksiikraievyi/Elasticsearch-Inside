using System;
using ElasticsearchInside.Utilities;

namespace ElasticsearchInside.Config
{
    /// <summary>
    /// Adds convenince-methods on top of Elasitcsearch configuration dictionaries and lists
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Configures the http-port that elasticsearch should bind to
        /// </summary>
        public static ISettings SetPort(this ISettings config, int port)
        {
            config.ElasticsearchParameters.AddOrUpdate("http.port", port.ToString());
            return config;
        }
        
        /// <summary>
        /// Parses the current port from config
        /// </summary>
        public static int? GetPort(this ISettings config)
        {
            int port;

            if (!int.TryParse(config.ElasticsearchParameters.ValueOrNull("http.port"), out port))
                return null;

            return port;
        }

        /// <summary>
        /// Builds the current expected full url
        /// </summary>
        public static Uri GetUrl(this ISettings config)
        {
            var port = config.GetPort();

            if (!port.HasValue)
                throw new ApplicationException("Port has not been set, unable to create url");

            return new UriBuilder
            {
                Scheme = Uri.UriSchemeHttp,
                Port = port.Value
            }.Uri;
        }

        /// <summary>
        /// Configures the clustername for elasticsearch
        /// </summary>
        public static ISettings SetClustername(this ISettings config, string clusterName)
        {
            config.ElasticsearchParameters.AddOrUpdate("cluster.name", clusterName);
            return config;
        }

        /// <summary>
        /// Configures the nodename for elasticsearch
        /// </summary>
        public static ISettings SetNodename(this ISettings config, string nodeName)
        {
            config.ElasticsearchParameters.AddOrUpdate("node.name", nodeName);
            return config;
        }
    }
}
