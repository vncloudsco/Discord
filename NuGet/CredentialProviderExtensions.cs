namespace NuGet
{
    using System;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static class CredentialProviderExtensions
    {
        private static readonly string[] _authenticationSchemes = new string[] { "Basic", "NTLM", "Negotiate" };

        internal static ICredentials AsCredentialCache(this ICredentials credentials, Uri uri)
        {
            if (credentials == null)
            {
                return null;
            }
            if (ReferenceEquals(credentials, CredentialCache.DefaultCredentials) || ReferenceEquals(credentials, CredentialCache.DefaultNetworkCredentials))
            {
                return credentials;
            }
            NetworkCredential cred = credentials as NetworkCredential;
            if (cred == null)
            {
                return credentials;
            }
            CredentialCache cache = new CredentialCache();
            foreach (string str in _authenticationSchemes)
            {
                cache.Add(uri, str, cred);
            }
            return cache;
        }

        internal static ICredentials GetCredentials(this ICredentialProvider provider, WebRequest request, CredentialType credentialType, bool retrying = false) => 
            provider.GetCredentials(request.RequestUri, request.Proxy, credentialType, retrying);
    }
}

