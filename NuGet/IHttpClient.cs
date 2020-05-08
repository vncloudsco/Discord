namespace NuGet
{
    using System;
    using System.IO;
    using System.Net;

    internal interface IHttpClient : IHttpClientEvents, IProgressProvider
    {
        void DownloadData(Stream targetStream);
        WebResponse GetResponse();
        void InitializeRequest(WebRequest request);

        string UserAgent { get; set; }

        System.Uri Uri { get; }

        System.Uri OriginalUri { get; }

        bool AcceptCompression { get; set; }
    }
}

