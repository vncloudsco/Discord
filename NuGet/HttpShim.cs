namespace NuGet
{
    using System;
    using System.Data.Services.Client;
    using System.Net;

    internal sealed class HttpShim
    {
        private static HttpShim _instance;
        private Func<DataServiceClientRequestMessageArgs, DataServiceClientRequestMessage> _dataServiceHandler;
        private Func<WebRequest, WebResponse> _webHandler;

        internal HttpShim()
        {
        }

        public void ClearHandlers()
        {
            this._dataServiceHandler = null;
            this._webHandler = null;
        }

        private static void InitializeMessage(DataServiceClientRequestMessage message)
        {
            IShimWebRequest request = message as IShimWebRequest;
            HttpWebRequestMessage message2 = message as HttpWebRequestMessage;
            if (message2 != null)
            {
                InitializeRequest(message2.get_HttpWebRequest());
            }
            else if (request != null)
            {
                InitializeRequest(request.Request);
            }
        }

        private static void InitializeRequest(WebRequest request)
        {
            try
            {
                SetCredentialsAndProxy(request);
                InitializeRequestProperties(request);
            }
            catch (InvalidOperationException)
            {
            }
        }

        private static void InitializeRequestProperties(WebRequest request)
        {
            HttpWebRequest request2 = request as HttpWebRequest;
            if (request2 != null)
            {
                request2.UserAgent = HttpUtility.CreateUserAgentString("NuGet Shim");
                request2.CookieContainer = new CookieContainer();
                request2.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            }
        }

        private static void SetCredentialsAndProxy(WebRequest request)
        {
            if (request.Credentials == null)
            {
                request.Credentials = CredentialStore.Instance.GetCredentials(request.RequestUri);
            }
            if (request.Proxy == null)
            {
                request.Proxy = ProxyCache.Instance.GetProxy(request.RequestUri);
            }
            STSAuthHelper.PrepareSTSRequest(request);
        }

        public void SetDataServiceRequestHandler(Func<DataServiceClientRequestMessageArgs, DataServiceClientRequestMessage> handler)
        {
            this._dataServiceHandler = handler;
        }

        public void SetWebRequestHandler(Func<WebRequest, WebResponse> handler)
        {
            this._webHandler = handler;
        }

        internal DataServiceClientRequestMessage ShimDataServiceRequest(DataServiceClientRequestMessageArgs args)
        {
            DataServiceClientRequestMessage message = null;
            message = (this._dataServiceHandler == null) ? new HttpWebRequestMessage(args) : this._dataServiceHandler(args);
            InitializeMessage(message);
            return message;
        }

        internal WebResponse ShimWebRequest(WebRequest request)
        {
            InitializeRequest(request);
            return ((this._webHandler == null) ? request.GetResponse() : this._webHandler(request));
        }

        public static HttpShim Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HttpShim();
                }
                return _instance;
            }
        }
    }
}

