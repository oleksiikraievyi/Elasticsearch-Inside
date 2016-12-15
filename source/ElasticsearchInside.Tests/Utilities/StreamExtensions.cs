using System.IO;
using System.Threading.Tasks;

namespace ElasticsearchInside.Tests.Utilities
{
    public static class StreamExtensions
    {
        public static async Task<string> Dump(this Stream stream)
        {
            if (stream.CanSeek)
                stream.Position = 0;

            using (var reader = new StreamReader(stream))
                return await reader.ReadToEndAsync();
        }
    }
}
