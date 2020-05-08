namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class PackageServer
    {
        private const string ServiceEndpoint = "/api/v2/package";
        private const string ApiKeyHeader = "X-NuGet-ApiKey";
        private const int MaxRediretionCount = 20;
        private Lazy<Uri> _baseUri;
        private readonly string _source;
        private readonly string _userAgent;
        [CompilerGenerated]
        private EventHandler<WebRequestEventArgs> SendingRequest = delegate (object <sender>, WebRequestEventArgs <e>) {
        };

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

        public PackageServer(string source, string userAgent)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "source");
            }
            this._source = source;
            this._userAgent = userAgent;
            this._baseUri = new Lazy<Uri>(new Func<Uri>(this.ResolveBaseUrl));
        }

        public void DeletePackage(string apiKey, string packageId, string packageVersion)
        {
            Uri uri = new Uri(this.Source);
            if (uri.IsFile)
            {
                DeletePackageFromFileSystem(new PhysicalFileSystem(uri.LocalPath), packageId, packageVersion);
            }
            else
            {
                this.DeletePackageFromServer(apiKey, packageId, packageVersion);
            }
        }

        private static void DeletePackageFromFileSystem(IFileSystem fileSystem, string packageId, string packageVersion)
        {
            string packageFileName = new DefaultPackagePathResolver(fileSystem).GetPackageFileName(packageId, new SemanticVersion(packageVersion));
            fileSystem.DeleteFile(packageFileName);
        }

        private void DeletePackageFromServer(string apiKey, string packageId, string packageVersion)
        {
            string[] textArray1 = new string[] { packageId, packageVersion };
            string path = string.Join("/", textArray1);
            HttpClient client = this.GetClient(path, "DELETE", "text/html");
            client.SendingRequest += delegate (object sender, WebRequestEventArgs e) {
                this.SendingRequest(this, e);
                ((HttpWebRequest) e.Request).Headers.Add("X-NuGet-ApiKey", apiKey);
            };
            HttpStatusCode? expectedStatusCode = null;
            this.EnsureSuccessfulResponse(client, expectedStatusCode);
        }

        private bool EnsureSuccessfulResponse(HttpClient client, HttpStatusCode? expectedStatusCode = new HttpStatusCode?())
        {
            HttpWebResponse response = null;
            bool flag;
            try
            {
                response = (HttpWebResponse) client.GetResponse();
                if ((response == null) || (((expectedStatusCode == null) || (((HttpStatusCode) expectedStatusCode.Value) == response.StatusCode)) && ((expectedStatusCode != null) || (response.StatusCode < HttpStatusCode.BadRequest))))
                {
                    return true;
                }
                object[] args = new object[] { response.StatusDescription, string.Empty };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.PackageServerError, args));
            }
            catch (WebException exception)
            {
                if (exception.Response == null)
                {
                    throw;
                }
                response = (HttpWebResponse) exception.Response;
                if ((response.StatusCode == HttpStatusCode.MultipleChoices) || ((response.StatusCode == HttpStatusCode.MovedPermanently) || ((response.StatusCode == HttpStatusCode.Found) || ((response.StatusCode == HttpStatusCode.SeeOther) || (response.StatusCode == HttpStatusCode.TemporaryRedirect)))))
                {
                    Uri newUri;
                    string relativeUri = response.Headers["Location"];
                    if (!Uri.TryCreate(client.Uri, relativeUri, out newUri))
                    {
                        throw;
                    }
                    this._baseUri = new Lazy<Uri>(() => newUri);
                    flag = false;
                }
                else
                {
                    HttpStatusCode? nullable = expectedStatusCode;
                    HttpStatusCode statusCode = response.StatusCode;
                    if ((((HttpStatusCode) nullable.GetValueOrDefault()) == statusCode) ? (nullable == null) : true)
                    {
                        object[] args = new object[] { response.StatusDescription, exception.Message };
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.PackageServerError, args), exception);
                    }
                    flag = true;
                }
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
            }
            return flag;
        }

        private static Uri EnsureTrailingSlash(Uri uri)
        {
            string originalString = uri.OriginalString;
            if (!originalString.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                originalString = originalString + "/";
            }
            return new Uri(originalString);
        }

        private HttpClient GetClient(string path, string method, string contentType)
        {
            HttpClient client1 = new HttpClient(GetServiceEndpointUrl(this._baseUri.Value, path));
            client1.ContentType = contentType;
            client1.Method = method;
            HttpClient client = client1;
            if (!string.IsNullOrEmpty(this._userAgent))
            {
                client.UserAgent = HttpUtility.CreateUserAgentString(this._userAgent);
            }
            return client;
        }

        internal static Uri GetServiceEndpointUrl(Uri baseUrl, string path)
        {
            char[] trimChars = new char[] { '/' };
            return (!string.IsNullOrEmpty(baseUrl.AbsolutePath.TrimStart(trimChars)) ? new Uri(baseUrl, path) : new Uri(baseUrl, "/api/v2/package/" + path));
        }

        public void PushPackage(string apiKey, IPackage package, long packageSize, int timeout, bool disableBuffering)
        {
            Uri uri = new Uri(this.Source);
            if (uri.IsFile)
            {
                PushPackageToFileSystem(new PhysicalFileSystem(uri.LocalPath), package);
            }
            else
            {
                this.PushPackageToServer(apiKey, new Func<Stream>(package.GetStream), packageSize, timeout, disableBuffering);
            }
        }

        private static void PushPackageToFileSystem(IFileSystem fileSystem, IPackage package)
        {
            string packageFileName = new DefaultPackagePathResolver(fileSystem).GetPackageFileName(package);
            using (Stream stream = package.GetStream())
            {
                fileSystem.AddFile(packageFileName, stream);
            }
        }

        private void PushPackageToServer(string apiKey, Func<Stream> packageStreamFactory, long packageSize, int timeout, bool disableBuffering)
        {
            int num = 0;
            while (true)
            {
                EventHandler<WebRequestEventArgs> <>9__0;
                HttpClient client = this.GetClient("", "PUT", "application/octet-stream");
                client.DisableBuffering = disableBuffering;
                EventHandler<WebRequestEventArgs> handler2 = <>9__0;
                if (<>9__0 == null)
                {
                    EventHandler<WebRequestEventArgs> local1 = <>9__0;
                    handler2 = <>9__0 = delegate (object sender, WebRequestEventArgs e) {
                        this.SendingRequest(this, e);
                        HttpWebRequest request = (HttpWebRequest) e.Request;
                        if (timeout <= 0)
                        {
                            timeout = request.ReadWriteTimeout;
                        }
                        request.Timeout = timeout;
                        request.ReadWriteTimeout = timeout;
                        if (!string.IsNullOrEmpty(apiKey))
                        {
                            request.Headers.Add("X-NuGet-ApiKey", apiKey);
                        }
                        MultipartWebRequest request1 = new MultipartWebRequest();
                        request1.AddFile(packageStreamFactory, "package", packageSize, "application/octet-stream");
                        request1.CreateMultipartRequest(request);
                    };
                }
                client.SendingRequest += handler2;
                HttpStatusCode? expectedStatusCode = null;
                if (this.EnsureSuccessfulResponse(client, expectedStatusCode))
                {
                    return;
                }
                num++;
                if (num > 20)
                {
                    throw new WebException(NuGetResources.Error_TooManyRedirections);
                }
            }
        }

        private Uri ResolveBaseUrl()
        {
            Uri responseUri;
            try
            {
                responseUri = new RedirectedHttpClient(new Uri(this.Source)).Uri;
            }
            catch (WebException exception1)
            {
                HttpWebResponse response = (HttpWebResponse) exception1.Response;
                if (response == null)
                {
                    throw;
                }
                responseUri = response.ResponseUri;
            }
            return EnsureTrailingSlash(responseUri);
        }

        public string Source =>
            this._source;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PackageServer.<>c <>9 = new PackageServer.<>c();
            public static EventHandler<WebRequestEventArgs> <>9__9_0;

            internal void <.ctor>b__9_0(object <sender>, WebRequestEventArgs <e>)
            {
            }
        }
    }
}

