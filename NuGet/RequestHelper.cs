namespace NuGet
{
    using System;
    using System.Collections.Specialized;
    using System.Net;
    using System.Text;

    internal class RequestHelper
    {
        private Func<WebRequest> _createRequest;
        private Action<WebRequest> _prepareRequest;
        private IProxyCache _proxyCache;
        private ICredentialCache _credentialCache;
        private ICredentialProvider _credentialProvider;
        private HttpWebRequest _previousRequest;
        private IHttpWebResponse _previousResponse;
        private HttpStatusCode? _previousStatusCode;
        private int _credentialsRetryCount;
        private bool _usingSTSAuth;
        private bool _continueIfFailed;
        private int _proxyCredentialsRetryCount;
        private bool _basicAuthIsUsedInPreviousRequest;
        private bool _disableBuffering;

        public RequestHelper(Func<WebRequest> createRequest, Action<WebRequest> prepareRequest, IProxyCache proxyCache, ICredentialCache credentialCache, ICredentialProvider credentialProvider, bool disableBuffering)
        {
            this._createRequest = createRequest;
            this._prepareRequest = prepareRequest;
            this._proxyCache = proxyCache;
            this._credentialCache = credentialCache;
            this._credentialProvider = credentialProvider;
            this._disableBuffering = disableBuffering;
        }

        private void ConfigureRequest(HttpWebRequest request)
        {
            request.Proxy = this._proxyCache.GetProxy(request.RequestUri);
            if ((request.Proxy != null) && (request.Proxy.Credentials == null))
            {
                request.Proxy.Credentials = CredentialCache.DefaultCredentials;
            }
            if ((this._previousResponse == null) || ShouldKeepAliveBeUsedInRequest(this._previousRequest, this._previousResponse))
            {
                request.Credentials = this._credentialCache.GetCredentials(request.RequestUri);
                if (request.Credentials == null)
                {
                    request.UseDefaultCredentials = true;
                }
            }
            else
            {
                HttpStatusCode? nullable = this._previousStatusCode;
                HttpStatusCode proxyAuthenticationRequired = HttpStatusCode.ProxyAuthenticationRequired;
                if ((((HttpStatusCode) nullable.GetValueOrDefault()) == proxyAuthenticationRequired) ? (nullable != null) : false)
                {
                    request.Proxy.Credentials = this._credentialProvider.GetCredentials(request, CredentialType.ProxyCredentials, this._proxyCredentialsRetryCount > 0);
                    this._continueIfFailed = request.Proxy.Credentials != null;
                    this._proxyCredentialsRetryCount++;
                }
                else
                {
                    nullable = this._previousStatusCode;
                    proxyAuthenticationRequired = HttpStatusCode.Unauthorized;
                    if ((((HttpStatusCode) nullable.GetValueOrDefault()) == proxyAuthenticationRequired) ? (nullable != null) : false)
                    {
                        this.SetCredentialsOnAuthorizationError(request);
                    }
                }
            }
            SetKeepAliveHeaders(request, this._previousResponse);
            if (this._usingSTSAuth)
            {
                STSAuthHelper.PrepareSTSRequest(request);
            }
            request.Credentials = request.Credentials.AsCredentialCache(request.RequestUri);
        }

        public WebResponse GetResponse()
        {
            WebResponse response2;
            this._previousRequest = null;
            this._previousResponse = null;
            this._previousStatusCode = null;
            this._usingSTSAuth = false;
            this._continueIfFailed = true;
            this._proxyCredentialsRetryCount = 0;
            this._credentialsRetryCount = 0;
            int num = 0;
            while (true)
            {
                HttpWebRequest request = (HttpWebRequest) this._createRequest();
                this.ConfigureRequest(request);
                try
                {
                    if (this._disableBuffering)
                    {
                        request.AllowWriteStreamBuffering = false;
                        bool flag = ((this._previousResponse != null) && (this._previousResponse.AuthType != null)) && (this._previousResponse.AuthType.IndexOf("Basic", StringComparison.OrdinalIgnoreCase) != -1);
                        NetworkCredential credential = request.Credentials.GetCredential(request.RequestUri, "Basic");
                        if ((credential != null) & flag)
                        {
                            string s = credential.UserName + ":" + credential.Password;
                            s = Convert.ToBase64String(Encoding.Default.GetBytes(s));
                            request.Headers["Authorization"] = "Basic " + s;
                            this._basicAuthIsUsedInPreviousRequest = true;
                        }
                    }
                    this._prepareRequest(request);
                    WebResponse response = HttpShim.Instance.ShimWebRequest(request);
                    this._proxyCache.Add(request.Proxy);
                    ICredentials credentials = request.Credentials;
                    this._credentialCache.Add(request.RequestUri, credentials);
                    this._credentialCache.Add(response.ResponseUri, credentials);
                    response2 = response;
                    break;
                }
                catch (WebException exception)
                {
                    if ((num + 1) >= 10)
                    {
                        throw;
                    }
                    using (IHttpWebResponse response3 = GetResponse(exception.Response))
                    {
                        if ((response3 == null) && (exception.Status != WebExceptionStatus.SecureChannelFailure))
                        {
                            throw;
                        }
                        if (exception.Status == WebExceptionStatus.SecureChannelFailure)
                        {
                            if (!this._continueIfFailed)
                            {
                                throw;
                            }
                            this._previousStatusCode = 0x191;
                        }
                        else
                        {
                            HttpStatusCode? nullable = this._previousStatusCode;
                            HttpStatusCode proxyAuthenticationRequired = HttpStatusCode.ProxyAuthenticationRequired;
                            if (((((HttpStatusCode) nullable.GetValueOrDefault()) == proxyAuthenticationRequired) ? (nullable != null) : false) && (response3.StatusCode != HttpStatusCode.ProxyAuthenticationRequired))
                            {
                                this._proxyCache.Add(request.Proxy);
                            }
                            else
                            {
                                nullable = this._previousStatusCode;
                                proxyAuthenticationRequired = HttpStatusCode.Unauthorized;
                                if (((((HttpStatusCode) nullable.GetValueOrDefault()) == proxyAuthenticationRequired) ? (nullable != null) : false) && (response3.StatusCode != HttpStatusCode.Unauthorized))
                                {
                                    this._credentialCache.Add(request.RequestUri, request.Credentials);
                                    this._credentialCache.Add(response3.ResponseUri, request.Credentials);
                                }
                            }
                            this._usingSTSAuth = STSAuthHelper.TryRetrieveSTSToken(request.RequestUri, response3);
                            if (!IsAuthenticationResponse(response3) || !this._continueIfFailed)
                            {
                                throw;
                            }
                            if (!EnvironmentUtility.IsNet45Installed && (!request.AllowWriteStreamBuffering && ((response3.AuthType != null) && IsNtlmOrKerberos(response3.AuthType))))
                            {
                                throw;
                            }
                            this._previousRequest = request;
                            this._previousResponse = response3;
                            this._previousStatusCode = new HttpStatusCode?(this._previousResponse.StatusCode);
                        }
                    }
                }
            }
            return response2;
        }

        private static IHttpWebResponse GetResponse(WebResponse response)
        {
            IHttpWebResponse response2 = response as IHttpWebResponse;
            if (response2 != null)
            {
                return response2;
            }
            HttpWebResponse response3 = response as HttpWebResponse;
            return ((response3 != null) ? new HttpWebResponseWrapper(response3) : null);
        }

        private static bool IsAuthenticationResponse(IHttpWebResponse response) => 
            ((response.StatusCode == HttpStatusCode.Unauthorized) || (response.StatusCode == HttpStatusCode.ProxyAuthenticationRequired));

        private static bool IsNtlmOrKerberos(string authType) => 
            (!string.IsNullOrEmpty(authType) ? ((authType.IndexOf("NTLM", StringComparison.OrdinalIgnoreCase) != -1) || (authType.IndexOf("Kerberos", StringComparison.OrdinalIgnoreCase) != -1)) : false);

        private void SetCredentialsOnAuthorizationError(HttpWebRequest request)
        {
            if (!this._usingSTSAuth)
            {
                bool flag = (this._previousResponse.AuthType != null) && (this._previousResponse.AuthType.IndexOf("Basic", StringComparison.OrdinalIgnoreCase) != -1);
                if ((this._disableBuffering & flag) && !this._basicAuthIsUsedInPreviousRequest)
                {
                    request.Credentials = this._credentialCache.GetCredentials(request.RequestUri);
                }
                if (request.Credentials == null)
                {
                    request.Credentials = this._credentialProvider.GetCredentials(request, CredentialType.RequestCredentials, this._credentialsRetryCount > 0);
                }
                this._continueIfFailed = request.Credentials != null;
                this._credentialsRetryCount++;
            }
        }

        private static void SetKeepAliveHeaders(HttpWebRequest request, IHttpWebResponse previousResponse)
        {
            if ((previousResponse == null) || !IsNtlmOrKerberos(previousResponse.AuthType))
            {
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
            }
        }

        private static bool ShouldKeepAliveBeUsedInRequest(HttpWebRequest request, IHttpWebResponse response)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            if (response == null)
            {
                throw new ArgumentNullException("response");
            }
            return (!request.KeepAlive && IsNtlmOrKerberos(response.AuthType));
        }

        private class HttpWebResponseWrapper : IHttpWebResponse, IDisposable
        {
            private readonly HttpWebResponse _response;

            public HttpWebResponseWrapper(HttpWebResponse response)
            {
                this._response = response;
            }

            public void Dispose()
            {
                if (this._response != null)
                {
                    this._response.Close();
                }
            }

            public string AuthType =>
                this._response.Headers[HttpResponseHeader.WwwAuthenticate];

            public HttpStatusCode StatusCode =>
                this._response.StatusCode;

            public Uri ResponseUri =>
                this._response.ResponseUri;

            public NameValueCollection Headers =>
                this._response.Headers;
        }
    }
}

