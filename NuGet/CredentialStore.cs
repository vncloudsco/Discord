namespace NuGet
{
    using System;
    using System.Collections.Concurrent;
    using System.Net;

    internal class CredentialStore : ICredentialCache
    {
        private readonly ConcurrentDictionary<Uri, ICredentials> _credentialCache = new ConcurrentDictionary<Uri, ICredentials>();
        private static readonly CredentialStore _instance = new CredentialStore();

        public void Add(Uri uri, ICredentials credentials)
        {
            Uri rootUri = GetRootUri(uri);
            this._credentialCache.TryAdd(uri, credentials);
            this._credentialCache.AddOrUpdate(rootUri, credentials, (u, c) => credentials);
        }

        public ICredentials GetCredentials(Uri uri)
        {
            ICredentials credentials;
            Uri rootUri = GetRootUri(uri);
            return ((this._credentialCache.TryGetValue(uri, out credentials) || this._credentialCache.TryGetValue(rootUri, out credentials)) ? credentials : null);
        }

        internal static Uri GetRootUri(Uri uri) => 
            new Uri(uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped));

        public static CredentialStore Instance =>
            _instance;
    }
}

