namespace NuGet
{
    using System;
    using System.Net;

    internal class NullCredentialProvider : ICredentialProvider
    {
        private static readonly NullCredentialProvider _instance = new NullCredentialProvider();

        private NullCredentialProvider()
        {
        }

        public ICredentials GetCredentials(Uri uri, IWebProxy proxy, CredentialType credentialType, bool retrying) => 
            null;

        public static ICredentialProvider Instance =>
            _instance;
    }
}

