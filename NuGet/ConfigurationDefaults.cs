namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal class ConfigurationDefaults
    {
        private ISettings _settingsManager = NullSettings.Instance;
        private const string ConfigurationDefaultsFile = "NuGetDefaults.config";
        private static readonly ConfigurationDefaults _instance = InitializeInstance();
        private bool _defaultPackageSourceInitialized;
        private List<PackageSource> _defaultPackageSources;
        private string _defaultPushSource;

        internal ConfigurationDefaults(IFileSystem fileSystem, string path)
        {
            try
            {
                if (fileSystem.FileExists(path))
                {
                    this._settingsManager = new Settings(fileSystem, path);
                }
            }
            catch (FileNotFoundException)
            {
            }
        }

        private static ConfigurationDefaults InitializeInstance() => 
            new ConfigurationDefaults(new PhysicalFileSystem(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "NuGet")), "NuGetDefaults.config");

        public static ConfigurationDefaults Instance =>
            _instance;

        public IEnumerable<PackageSource> DefaultPackageSources
        {
            get
            {
                if (this._defaultPackageSources == null)
                {
                    this._defaultPackageSources = new List<PackageSource>();
                    IList<SettingValue> settingValues = this._settingsManager.GetSettingValues("disabledPackageSources", false);
                    foreach (SettingValue settingValue in this._settingsManager.GetSettingValues("packageSources", false))
                    {
                        this._defaultPackageSources.Add(new PackageSource(settingValue.Value, settingValue.Key, !Enumerable.Any<SettingValue>(settingValues, p => p.Key.Equals(settingValue.Key, StringComparison.CurrentCultureIgnoreCase)), true, true));
                    }
                }
                return this._defaultPackageSources;
            }
        }

        public string DefaultPushSource
        {
            get
            {
                if ((this._defaultPushSource == null) && !this._defaultPackageSourceInitialized)
                {
                    this._defaultPackageSourceInitialized = true;
                    this._defaultPushSource = this._settingsManager.GetConfigValue("DefaultPushSource", false, false);
                }
                return this._defaultPushSource;
            }
        }

        public string DefaultPackageRestoreConsent =>
            this._settingsManager.GetValue("packageRestore", "enabled");
    }
}

