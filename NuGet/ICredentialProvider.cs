namespace NuGet
{
    using System;
    using System.Net;

    internal interface ICredentialProvider
    {
        ICredentials GetCredentials(Uri uri, IWebProxy proxy, CredentialType credentialType, bool retrying);
    }
}

