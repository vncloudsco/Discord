namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;

    internal static class ShimWebHelpers
    {
        public static HttpWebRequest AddHeaders(HttpWebRequest response, IDictionary<string, string> headers) => 
            AddHeaders(response, headers.AsEnumerable<KeyValuePair<string, string>>());

        public static HttpWebRequest AddHeaders(HttpWebRequest response, IEnumerable<KeyValuePair<string, string>> headers)
        {
            foreach (KeyValuePair<string, string> pair in headers)
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(pair.Key, "accept"))
                {
                    response.Accept = pair.Value;
                    continue;
                }
                if (StringComparer.OrdinalIgnoreCase.Equals(pair.Key, "user-agent"))
                {
                    response.UserAgent = pair.Value;
                    continue;
                }
                if (StringComparer.OrdinalIgnoreCase.Equals(pair.Key, "content-type"))
                {
                    response.ContentType = pair.Value;
                    continue;
                }
                response.Headers.Set(pair.Key, pair.Value);
            }
            return response;
        }
    }
}

