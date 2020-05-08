namespace NuGet
{
    using System;
    using System.Net;
    using System.Runtime.CompilerServices;

    internal class WebRequestEventArgs : EventArgs
    {
        public WebRequestEventArgs(WebRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            this.Request = request;
        }

        public WebRequest Request { get; private set; }
    }
}

