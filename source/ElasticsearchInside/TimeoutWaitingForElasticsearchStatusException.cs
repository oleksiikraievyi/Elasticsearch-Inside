using System;

namespace ElasticsearchInside
{
    public class TimeoutWaitingForElasticsearchStatusException : Exception
    {
        public TimeoutWaitingForElasticsearchStatusException(Exception ex) : base("Timeout waiting for Elasticsearch status", ex)
        {
        }
    }
}