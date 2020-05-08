namespace NuGet
{
    using System;
    using System.Net;

    internal interface IProxyCache
    {
        void Add(IWebProxy proxy);
        IWebProxy GetProxy(Uri uri);
    }
}

