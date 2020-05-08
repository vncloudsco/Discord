namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Globalization;
    using System.Net;

    internal class RedirectedHttpClient : HttpClient
    {
        private const string RedirectedClientCacheKey = "RedirectedHttpClient|";
        private readonly System.Uri _originalUri;
        private readonly MemoryCache _memoryCache;

        public RedirectedHttpClient(System.Uri uri) : this(uri, MemoryCache.Instance)
        {
        }

        public RedirectedHttpClient(System.Uri uri, MemoryCache memoryCache) : base(uri)
        {
            this._originalUri = uri;
            this._memoryCache = memoryCache;
        }

        protected internal virtual IHttpClient EnsureClient()
        {
            HttpClient client = new HttpClient(this._originalUri);
            return new HttpClient(this.GetResponseUri(client));
        }

        private string GetCacheKey() => 
            ("RedirectedHttpClient|" + this._originalUri.OriginalString);

        public override WebResponse GetResponse() => 
            this.CachedClient.GetResponse();

        private System.Uri GetResponseUri(HttpClient client)
        {
            using (WebResponse response = client.GetResponse())
            {
                if (response == null)
                {
                    object[] args = new object[] { this.Uri };
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.UnableToResolveUri, args));
                }
                return response.ResponseUri;
            }
        }

        public override System.Uri Uri =>
            this.CachedClient.Uri;

        public override System.Uri OriginalUri =>
            this._originalUri;

        internal IHttpClient CachedClient
        {
            get
            {
                IHttpClient client;
                string cacheKey = this.GetCacheKey();
                try
                {
                    client = this._memoryCache.GetOrAdd<IHttpClient>(cacheKey, new Func<IHttpClient>(this.EnsureClient), TimeSpan.FromHours(1.0), false);
                }
                catch (Exception)
                {
                    this._memoryCache.Remove(cacheKey);
                    throw;
                }
                return client;
            }
        }
    }
}

