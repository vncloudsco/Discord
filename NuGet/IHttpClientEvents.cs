namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;

    internal interface IHttpClientEvents : IProgressProvider
    {
        event EventHandler<WebRequestEventArgs> SendingRequest;
    }
}

