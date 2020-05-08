namespace NuGet
{
    using System;
    using System.Collections.Specialized;
    using System.Net;

    internal interface IHttpWebResponse : IDisposable
    {
        HttpStatusCode StatusCode { get; }

        Uri ResponseUri { get; }

        string AuthType { get; }

        NameValueCollection Headers { get; }
    }
}

