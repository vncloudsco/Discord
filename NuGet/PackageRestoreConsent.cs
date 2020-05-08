namespace NuGet
{
    using System;
    using System.Globalization;

    internal class PackageRestoreConsent
    {
        private const string EnvironmentVariableName = "EnableNuGetPackageRestore";
        private const string PackageRestoreSection = "packageRestore";
        private const string PackageRestoreConsentKey = "enabled";
        private const string PackageRestoreAutomaticKey = "automatic";
        private readonly ISettings _settings;
        private readonly IEnvironmentVariableReader _environmentReader;
        private readonly ConfigurationDefaults _configurationDefaults;

        public PackageRestoreConsent(ISettings settings) : this(settings, new EnvironmentVariableWrapper())
        {
        }

        public PackageRestoreConsent(ISettings settings, IEnvironmentVariableReader environmentReader) : this(settings, environmentReader, ConfigurationDefaults.Instance)
        {
        }

        public PackageRestoreConsent(ISettings settings, IEnvironmentVariableReader environmentReader, ConfigurationDefaults configurationDefaults)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }
            if (environmentReader == null)
            {
                throw new ArgumentNullException("environmentReader");
            }
            if (configurationDefaults == null)
            {
                throw new ArgumentNullException("configurationDefaults");
            }
            this._settings = settings;
            this._environmentReader = environmentReader;
            this._configurationDefaults = configurationDefaults;
        }

        private static bool IsSet(string value)
        {
            bool flag;
            int num;
            return ((bool.TryParse(value, out flag) & flag) || (int.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out num) && (num == 1)));
        }

        public bool IsGranted
        {
            get
            {
                string str = this._environmentReader.GetEnvironmentVariable("EnableNuGetPackageRestore").SafeTrim();
                return (this.IsGrantedInSettings || IsSet(str));
            }
        }

        public bool IsGrantedInSettings
        {
            get
            {
                string defaultPackageRestoreConsent = this._settings.GetValue("packageRestore", "enabled");
                if (string.IsNullOrWhiteSpace(defaultPackageRestoreConsent))
                {
                    defaultPackageRestoreConsent = this._configurationDefaults.DefaultPackageRestoreConsent;
                }
                defaultPackageRestoreConsent = defaultPackageRestoreConsent.SafeTrim();
                return (!string.IsNullOrEmpty(defaultPackageRestoreConsent) ? IsSet(defaultPackageRestoreConsent) : true);
            }
            set => 
                this._settings.SetValue("packageRestore", "enabled", value.ToString());
        }

        public bool IsAutomatic
        {
            get
            {
                string str = this._settings.GetValue("packageRestore", "automatic");
                return (!string.IsNullOrWhiteSpace(str) ? IsSet(str.SafeTrim()) : this.IsGrantedInSettings);
            }
            set => 
                this._settings.SetValue("packageRestore", "automatic", value.ToString());
        }
    }
}

