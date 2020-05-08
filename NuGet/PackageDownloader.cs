namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class PackageDownloader : IHttpClientEvents, IProgressProvider
    {
        private const string DefaultUserAgentClient = "NuGet Core";
        [CompilerGenerated]
        private EventHandler<ProgressEventArgs> ProgressAvailable = delegate (object <sender>, ProgressEventArgs <e>) {
        };
        [CompilerGenerated]
        private EventHandler<WebRequestEventArgs> SendingRequest = delegate (object <sender>, WebRequestEventArgs <e>) {
        };

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

        public void DownloadPackage(IHttpClient downloadClient, IPackageName package, Stream targetStream)
        {
            if (downloadClient == null)
            {
                throw new ArgumentNullException("downloadClient");
            }
            if (targetStream == null)
            {
                throw new ArgumentNullException("targetStream");
            }
            object[] args = new object[] { package.Id, package.Version };
            string operation = string.Format(CultureInfo.CurrentCulture, NuGetResources.DownloadProgressStatus, args);
            this.CurrentDownloadPackageId = package.Id;
            this.CurrentDownloadPackageVersion = package.Version.ToString();
            EventHandler<ProgressEventArgs> handler = (sender, e) => this.OnPackageDownloadProgress(new ProgressEventArgs(operation, e.PercentComplete));
            try
            {
                downloadClient.ProgressAvailable += handler;
                downloadClient.SendingRequest += new EventHandler<WebRequestEventArgs>(this.OnSendingRequest);
                downloadClient.DownloadData(targetStream);
            }
            finally
            {
                downloadClient.ProgressAvailable -= handler;
                downloadClient.SendingRequest -= new EventHandler<WebRequestEventArgs>(this.OnSendingRequest);
                this.CurrentDownloadPackageId = null;
            }
        }

        public virtual void DownloadPackage(Uri uri, IPackageMetadata package, Stream targetStream)
        {
            if (uri == null)
            {
                throw new ArgumentNullException("uri");
            }
            HttpClient client1 = new HttpClient(uri);
            client1.UserAgent = HttpUtility.CreateUserAgentString("NuGet Core");
            HttpClient downloadClient = client1;
            this.DownloadPackage(downloadClient, package, targetStream);
        }

        private void OnPackageDownloadProgress(ProgressEventArgs e)
        {
            this.ProgressAvailable(this, e);
        }

        private void OnSendingRequest(object sender, WebRequestEventArgs webRequestArgs)
        {
            this.SendingRequest(this, webRequestArgs);
        }

        public string CurrentDownloadPackageId { get; private set; }

        public string CurrentDownloadPackageVersion { get; private set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PackageDownloader.<>c <>9 = new PackageDownloader.<>c();
            public static EventHandler<ProgressEventArgs> <>9__19_0;
            public static EventHandler<WebRequestEventArgs> <>9__19_1;

            internal void <.ctor>b__19_0(object <sender>, ProgressEventArgs <e>)
            {
            }

            internal void <.ctor>b__19_1(object <sender>, WebRequestEventArgs <e>)
            {
            }
        }
    }
}

