namespace NuGet
{
    using System;
    using System.Net;

    internal interface ICredentialCache
    {
        void Add(Uri uri, ICredentials credentials);
        ICredentials GetCredentials(Uri uri);
    }
}

