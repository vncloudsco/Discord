namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;

    internal class PackageRepositoryFactory : IPackageRepositoryFactory
    {
        private static readonly PackageRepositoryFactory _default = new PackageRepositoryFactory();
        private static readonly Func<Uri, IHttpClient> _defaultHttpClientFactory = u => new RedirectedHttpClient(u);
        private Func<Uri, IHttpClient> _httpClientFactory;

        public virtual IPackageRepository CreateRepository(string packageSource)
        {
            if (packageSource == null)
            {
                throw new ArgumentNullException("packageSource");
            }
            Uri arg = new Uri(packageSource);
            return (!arg.IsFile ? ((IPackageRepository) new DataServicePackageRepository(this.HttpClientFactory(arg))) : ((IPackageRepository) new LocalPackageRepository(arg.LocalPath)));
        }

        public static PackageRepositoryFactory Default =>
            _default;

        public Func<Uri, IHttpClient> HttpClientFactory
        {
            get => 
                (this._httpClientFactory ?? _defaultHttpClientFactory);
            set => 
                (this._httpClientFactory = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PackageRepositoryFactory.<>c <>9 = new PackageRepositoryFactory.<>c();

            internal IHttpClient <.cctor>b__10_0(Uri u) => 
                new RedirectedHttpClient(u);
        }
    }
}

