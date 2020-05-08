namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Linq;
    using System.Net;
    using System.Runtime.InteropServices;

    internal class SettingsCredentialProvider : ICredentialProvider
    {
        private readonly ICredentialProvider _credentialProvider;
        private readonly IPackageSourceProvider _packageSourceProvider;
        private readonly ILogger _logger;

        public SettingsCredentialProvider(ICredentialProvider credentialProvider, IPackageSourceProvider packageSourceProvider) : this(credentialProvider, packageSourceProvider, NullLogger.Instance)
        {
        }

        public SettingsCredentialProvider(ICredentialProvider credentialProvider, IPackageSourceProvider packageSourceProvider, ILogger logger)
        {
            if (credentialProvider == null)
            {
                throw new ArgumentNullException("credentialProvider");
            }
            if (packageSourceProvider == null)
            {
                throw new ArgumentNullException("packageSourceProvider");
            }
            this._credentialProvider = credentialProvider;
            this._packageSourceProvider = packageSourceProvider;
            this._logger = logger;
        }

        public ICredentials GetCredentials(Uri uri, IWebProxy proxy, CredentialType credentialType, bool retrying)
        {
            NetworkCredential credential;
            if (retrying || ((credentialType != CredentialType.RequestCredentials) || !this.TryGetCredentials(uri, out credential)))
            {
                return this._credentialProvider.GetCredentials(uri, proxy, credentialType, retrying);
            }
            object[] args = new object[] { credential.UserName };
            this._logger.Log(MessageLevel.Info, NuGetResources.SettingsCredentials_UsingSavedCredentials, args);
            return credential;
        }

        private bool TryGetCredentials(Uri uri, out NetworkCredential configurationCredentials)
        {
            PackageSource source = Enumerable.FirstOrDefault<PackageSource>(this._packageSourceProvider.LoadPackageSources(), delegate (PackageSource p) {
                Uri uri;
                return !string.IsNullOrEmpty(p.UserName) && (!string.IsNullOrEmpty(p.Password) && (Uri.TryCreate(p.Source, UriKind.Absolute, out uri) && UriUtility.UriStartsWith(uri, uri)));
            });
            if (source == null)
            {
                configurationCredentials = null;
                return false;
            }
            configurationCredentials = new NetworkCredential(source.UserName, source.Password);
            return true;
        }
    }
}

