namespace NuGet
{
    using System;
    using System.Collections.Concurrent;
    using System.Net;
    using System.Runtime.CompilerServices;

    internal class ProxyCache : IProxyCache
    {
        private const string HostKey = "http_proxy";
        private const string UserKey = "http_proxy.user";
        private const string PasswordKey = "http_proxy.password";
        private static readonly IWebProxy _originalSystemProxy = WebRequest.GetSystemWebProxy();
        private readonly ConcurrentDictionary<Uri, WebProxy> _cache = new ConcurrentDictionary<Uri, WebProxy>();
        private static readonly Lazy<ProxyCache> _instance = new Lazy<ProxyCache>(() => new ProxyCache(Settings.LoadDefaultSettings(null, null, null), new EnvironmentVariableWrapper()));
        private readonly ISettings _settings;
        private readonly IEnvironmentVariableReader _environment;

        public ProxyCache(ISettings settings, IEnvironmentVariableReader environment)
        {
            this._settings = settings;
            this._environment = environment;
        }

        public void Add(IWebProxy proxy)
        {
            WebProxy proxy2 = proxy as WebProxy;
            if (proxy2 != null)
            {
                this._cache.TryAdd(proxy2.Address, proxy2);
            }
        }

        public IWebProxy GetProxy(Uri uri)
        {
            WebProxy proxy3;
            WebProxy userConfiguredProxy = this.GetUserConfiguredProxy();
            if (userConfiguredProxy != null)
            {
                WebProxy proxy4;
                return (!this._cache.TryGetValue(userConfiguredProxy.Address, out proxy4) ? userConfiguredProxy : proxy4);
            }
            if (!IsSystemProxySet(uri))
            {
                return null;
            }
            WebProxy systemProxy = GetSystemProxy(uri);
            return (!this._cache.TryGetValue(systemProxy.Address, out proxy3) ? systemProxy : proxy3);
        }

        private static WebProxy GetSystemProxy(Uri uri) => 
            new WebProxy(_originalSystemProxy.GetProxy(uri));

        internal WebProxy GetUserConfiguredProxy()
        {
            Uri uri;
            string environmentVariable = this._settings.GetConfigValue("http_proxy", false, false);
            if (!string.IsNullOrEmpty(environmentVariable))
            {
                WebProxy proxy = new WebProxy(environmentVariable);
                string str2 = this._settings.GetConfigValue("http_proxy.user", false, false);
                string str3 = this._settings.GetConfigValue("http_proxy.password", true, false);
                if (!string.IsNullOrEmpty(str2) && !string.IsNullOrEmpty(str3))
                {
                    proxy.Credentials = new NetworkCredential(str2, str3);
                }
                return proxy;
            }
            environmentVariable = this._environment.GetEnvironmentVariable("http_proxy");
            if (string.IsNullOrEmpty(environmentVariable) || !Uri.TryCreate(environmentVariable, UriKind.Absolute, out uri))
            {
                return null;
            }
            WebProxy proxy2 = new WebProxy(uri.GetComponents(UriComponents.HttpRequestUrl, UriFormat.SafeUnescaped));
            if (!string.IsNullOrEmpty(uri.UserInfo))
            {
                char[] separator = new char[] { ':' };
                string[] strArray = uri.UserInfo.Split(separator);
                if (strArray.Length > 1)
                {
                    proxy2.Credentials = new NetworkCredential(strArray[0], strArray[1]);
                }
            }
            return proxy2;
        }

        private static bool IsSystemProxySet(Uri uri)
        {
            IWebProxy defaultWebProxy = WebRequest.DefaultWebProxy;
            if (defaultWebProxy != null)
            {
                Uri proxy = defaultWebProxy.GetProxy(uri);
                if (proxy != null)
                {
                    Uri address = new Uri(proxy.AbsoluteUri);
                    if (string.Equals(address.AbsoluteUri, uri.AbsoluteUri))
                    {
                        return false;
                    }
                    if (defaultWebProxy.IsBypassed(uri))
                    {
                        return false;
                    }
                    defaultWebProxy = new WebProxy(address);
                }
            }
            return (defaultWebProxy != null);
        }

        internal static ProxyCache Instance =>
            _instance.Value;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ProxyCache.<>c <>9 = new ProxyCache.<>c();

            internal ProxyCache <.cctor>b__16_0() => 
                new ProxyCache(Settings.LoadDefaultSettings(null, null, null), new EnvironmentVariableWrapper());
        }
    }
}

