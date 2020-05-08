namespace NuGet
{
    using System;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class HttpClient : IHttpClient, IHttpClientEvents, IProgressProvider
    {
        [CompilerGenerated]
        private EventHandler<ProgressEventArgs> ProgressAvailable = delegate (object <sender>, ProgressEventArgs <e>) {
        };
        [CompilerGenerated]
        private EventHandler<WebRequestEventArgs> SendingRequest = delegate (object <sender>, WebRequestEventArgs <e>) {
        };
        private static ICredentialProvider _credentialProvider;
        private System.Uri _uri;

        public event EventHandler<ProgressEventArgs> ProgressAvailable
        {
            [CompilerGenerated] add
            {
                EventHandler<ProgressEventArgs> progressAvailable = this.ProgressAvailable;
                while (true)
                {
                    EventHandler<ProgressEventArgs> a = progressAvailable;
                    EventHandler<ProgressEventArgs> handler3 = (EventHandler<ProgressEventArgs>) Delegate.Combine(a, value);
                    progressAvailable = Interlocked.CompareExchange<EventHandler<ProgressEventArgs>>(ref this.ProgressAvailable, handler3, a);
                    if (ReferenceEquals(progressAvailable, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<ProgressEventArgs> progressAvailable = this.ProgressAvailable;
                while (true)
                {
                    EventHandler<ProgressEventArgs> source = progressAvailable;
                    EventHandler<ProgressEventArgs> handler3 = (EventHandler<ProgressEventArgs>) Delegate.Remove(source, value);
                    progressAvailable = Interlocked.CompareExchange<EventHandler<ProgressEventArgs>>(ref this.ProgressAvailable, handler3, source);
                    if (ReferenceEquals(progressAvailable, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EventHandler<WebRequestEventArgs> SendingRequest
        {
            [CompilerGenerated] add
            {
                EventHandler<WebRequestEventArgs> sendingRequest = this.SendingRequest;
                while (true)
                {
                    EventHandler<WebRequestEventArgs> a = sendingRequest;
                    EventHandler<WebRequestEventArgs> handler3 = (EventHandler<WebRequestEventArgs>) Delegate.Combine(a, value);
                    sendingRequest = Interlocked.CompareExchange<EventHandler<WebRequestEventArgs>>(ref this.SendingRequest, handler3, a);
                    if (ReferenceEquals(sendingRequest, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<WebRequestEventArgs> sendingRequest = this.SendingRequest;
                while (true)
                {
                    EventHandler<WebRequestEventArgs> source = sendingRequest;
                    EventHandler<WebRequestEventArgs> handler3 = (EventHandler<WebRequestEventArgs>) Delegate.Remove(source, value);
                    sendingRequest = Interlocked.CompareExchange<EventHandler<WebRequestEventArgs>>(ref this.SendingRequest, handler3, source);
                    if (ReferenceEquals(sendingRequest, source))
                    {
                        return;
                    }
                }
            }
        }

        public HttpClient(System.Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            this._uri = uri;
        }

        public void DownloadData(Stream targetStream)
        {
            using (WebResponse response = this.GetResponse())
            {
                int contentLength = (int) response.ContentLength;
                using (Stream stream = response.GetResponseStream())
                {
                    if (contentLength < 0)
                    {
                        stream.CopyTo(targetStream);
                        this.OnProgressAvailable(100);
                    }
                    else
                    {
                        int num2 = 0;
                        byte[] buffer = new byte[0x1000];
                        while (num2 < contentLength)
                        {
                            int count = stream.Read(buffer, 0, Math.Min(contentLength - num2, 0x1000));
                            if (count == 0)
                            {
                                break;
                            }
                            targetStream.Write(buffer, 0, count);
                            num2 += count;
                            this.OnProgressAvailable((num2 * 100) / contentLength);
                        }
                    }
                }
            }
        }

        public virtual WebResponse GetResponse() => 
            new RequestHelper(delegate {
                WebRequest request = WebRequest.Create(this.Uri);
                this.InitializeRequestProperties(request);
                return request;
            }, new Action<WebRequest>(this.RaiseSendingRequest), ProxyCache.Instance, CredentialStore.Instance, DefaultCredentialProvider, this.DisableBuffering).GetResponse();

        public void InitializeRequest(WebRequest request)
        {
            this.InitializeRequestProperties(request);
            this.TrySetCredentialsAndProxy(request);
            this.RaiseSendingRequest(request);
        }

        private void InitializeRequestProperties(WebRequest request)
        {
            HttpWebRequest request2 = request as HttpWebRequest;
            if (request2 != null)
            {
                request2.UserAgent = this.UserAgent ?? HttpUtility.CreateUserAgentString("NuGet");
                request2.CookieContainer = new CookieContainer();
                if (this.AcceptCompression)
                {
                    request2.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                }
            }
            if (!string.IsNullOrEmpty(this.ContentType))
            {
                request.ContentType = this.ContentType;
            }
            if (!string.IsNullOrEmpty(this.Method))
            {
                request.Method = this.Method;
            }
        }

        private void OnProgressAvailable(int percentage)
        {
            this.ProgressAvailable(this, new ProgressEventArgs(percentage));
        }

        private void RaiseSendingRequest(WebRequest webRequest)
        {
            this.SendingRequest(this, new WebRequestEventArgs(webRequest));
        }

        private void TrySetCredentialsAndProxy(WebRequest request)
        {
            request.Credentials = CredentialStore.Instance.GetCredentials(this.Uri);
            request.Proxy = ProxyCache.Instance.GetProxy(this.Uri);
            STSAuthHelper.PrepareSTSRequest(request);
        }

        public string UserAgent { get; set; }

        public virtual System.Uri Uri
        {
            get => 
                this._uri;
            set => 
                (this._uri = value);
        }

        public virtual System.Uri OriginalUri =>
            this._uri;

        public string Method { get; set; }

        public string ContentType { get; set; }

        public bool AcceptCompression { get; set; }

        public bool DisableBuffering { get; set; }

        public static ICredentialProvider DefaultCredentialProvider
        {
            get => 
                (_credentialProvider ?? NullCredentialProvider.Instance);
            set => 
                (_credentialProvider = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly HttpClient.<>c <>9 = new HttpClient.<>c();
            public static EventHandler<ProgressEventArgs> <>9__8_0;
            public static EventHandler<WebRequestEventArgs> <>9__8_1;

            internal void <.ctor>b__8_0(object <sender>, ProgressEventArgs <e>)
            {
            }

            internal void <.ctor>b__8_1(object <sender>, WebRequestEventArgs <e>)
            {
            }
        }
    }
}

