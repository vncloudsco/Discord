namespace NuGet
{
    using Microsoft.Data.OData;
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;

    internal class ShimDataRequestMessage : IODataRequestMessage
    {
        private SendingRequest2EventArgs _args;

        public ShimDataRequestMessage(DataServiceClientRequestMessageArgs args)
        {
            this.WebRequest = ShimWebHelpers.AddHeaders(System.Net.WebRequest.CreateHttp(args.get_RequestUri()), args.get_Headers());
            this.WebRequest.Method = args.get_Method();
        }

        public ShimDataRequestMessage(SendingRequest2EventArgs args)
        {
            this._args = args;
            this.WebRequest = ShimWebHelpers.AddHeaders(System.Net.WebRequest.CreateHttp(this._args.get_RequestMessage().get_Url()), this._args.get_RequestMessage().get_Headers());
            this.WebRequest.Method = this._args.get_RequestMessage().get_Method();
        }

        public string GetHeader(string headerName) => 
            this.WebRequest.Headers.Get(headerName);

        public Stream GetStream() => 
            this.WebRequest.GetRequestStream();

        public void SetHeader(string headerName, string headerValue)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(headerName, "Content-Length"))
            {
                this.WebRequest.ContentLength = long.Parse(headerValue, CultureInfo.InvariantCulture.NumberFormat);
            }
            else
            {
                this.WebRequest.Headers.Set(headerName, headerValue);
            }
        }

        public HttpWebRequest WebRequest { get; private set; }

        public IEnumerable<KeyValuePair<string, string>> Headers
        {
            get
            {
                List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
                foreach (string str in this.WebRequest.Headers.AllKeys)
                {
                    list.Add(new KeyValuePair<string, string>(str, this.WebRequest.Headers.Get(str)));
                }
                return list;
            }
        }

        public Uri Url
        {
            get => 
                this.WebRequest.RequestUri;
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Method
        {
            get => 
                this.WebRequest.Method;
            set => 
                (this.WebRequest.Method = value);
        }
    }
}

