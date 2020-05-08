namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;
    using System.Threading;

    internal class PackageSourceProvider : IPackageSourceProvider
    {
        private const string PackageSourcesSectionName = "packageSources";
        private const string DisabledPackageSourcesSectionName = "disabledPackageSources";
        private const string CredentialsSectionName = "packageSourceCredentials";
        private const string UsernameToken = "Username";
        private const string PasswordToken = "Password";
        private const string ClearTextPasswordToken = "ClearTextPassword";
        private readonly ISettings _settingsManager;
        private readonly IEnumerable<PackageSource> _providerDefaultSources;
        private readonly IDictionary<PackageSource, PackageSource> _migratePackageSources;
        private readonly IEnumerable<PackageSource> _configurationDefaultSources;
        private IEnvironmentVariableReader _environment;
        [CompilerGenerated]
        private EventHandler PackageSourcesSaved;

        public event EventHandler PackageSourcesSaved
        {
            [CompilerGenerated] add
            {
                EventHandler packageSourcesSaved = this.PackageSourcesSaved;
                while (true)
                {
                    EventHandler a = packageSourcesSaved;
                    EventHandler handler3 = (EventHandler) Delegate.Combine(a, value);
                    packageSourcesSaved = Interlocked.CompareExchange<EventHandler>(ref this.PackageSourcesSaved, handler3, a);
                    if (ReferenceEquals(packageSourcesSaved, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler packageSourcesSaved = this.PackageSourcesSaved;
                while (true)
                {
                    EventHandler source = packageSourcesSaved;
                    EventHandler handler3 = (EventHandler) Delegate.Remove(source, value);
                    packageSourcesSaved = Interlocked.CompareExchange<EventHandler>(ref this.PackageSourcesSaved, handler3, source);
                    if (ReferenceEquals(packageSourcesSaved, source))
                    {
                        return;
                    }
                }
            }
        }

        public PackageSourceProvider(ISettings settingsManager) : this(settingsManager, null)
        {
        }

        public PackageSourceProvider(ISettings settingsManager, IEnumerable<PackageSource> providerDefaultSources) : this(settingsManager, providerDefaultSources, null)
        {
        }

        public PackageSourceProvider(ISettings settingsManager, IEnumerable<PackageSource> providerDefaultSources, IDictionary<PackageSource, PackageSource> migratePackageSources) : this(settingsManager, providerDefaultSources, migratePackageSources, ConfigurationDefaults.Instance.DefaultPackageSources, new EnvironmentVariableWrapper())
        {
        }

        internal PackageSourceProvider(ISettings settingsManager, IEnumerable<PackageSource> providerDefaultSources, IDictionary<PackageSource, PackageSource> migratePackageSources, IEnumerable<PackageSource> configurationDefaultSources, IEnvironmentVariableReader environment)
        {
            if (settingsManager == null)
            {
                throw new ArgumentNullException("settingsManager");
            }
            this._settingsManager = settingsManager;
            this._providerDefaultSources = providerDefaultSources ?? Enumerable.Empty<PackageSource>();
            this._migratePackageSources = migratePackageSources;
            this._configurationDefaultSources = configurationDefaultSources ?? Enumerable.Empty<PackageSource>();
            this._environment = environment;
        }

        public void DisablePackageSource(PackageSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            this._settingsManager.SetValue("disabledPackageSources", source.Name, "true");
        }

        public bool IsPackageSourceEnabled(PackageSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return string.IsNullOrEmpty(this._settingsManager.GetValue("disabledPackageSources", source.Name));
        }

        public IEnumerable<PackageSource> LoadPackageSources()
        {
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<SettingValue> sequence = new List<SettingValue>();
            IList<SettingValue> settingValues = this._settingsManager.GetSettingValues("packageSources", true);
            int machineWideSourcesCount = 0;
            if (!settingValues.IsEmpty<SettingValue>())
            {
                List<SettingValue> collection = new List<SettingValue>();
                foreach (SettingValue value2 in settingValues.Reverse<SettingValue>())
                {
                    if (!set.Contains(value2.Key))
                    {
                        if (value2.IsMachineWide)
                        {
                            collection.Add(value2);
                        }
                        else
                        {
                            sequence.Add(value2);
                        }
                        set.Add(value2.Key);
                    }
                }
                sequence.Reverse();
                machineWideSourcesCount = collection.Count;
                sequence.AddRange(collection);
            }
            List<PackageSource> loadedPackageSources = new List<PackageSource>();
            if (!sequence.IsEmpty<SettingValue>())
            {
                Dictionary<string, SettingValue> disabledSources = Enumerable.ToDictionary<SettingValue, string>(this._settingsManager.GetSettingValues("disabledPackageSources", false) ?? Enumerable.Empty<SettingValue>(), s => s.Key, StringComparer.CurrentCultureIgnoreCase);
                loadedPackageSources = Enumerable.Select<SettingValue, PackageSource>(sequence, delegate (SettingValue p) {
                    SettingValue value2;
                    string key = p.Key;
                    PackageSourceCredential credential = this.ReadCredential(key);
                    bool isEnabled = true;
                    if (disabledSources.TryGetValue(key, out value2) && (value2.Priority >= p.Priority))
                    {
                        isEnabled = false;
                    }
                    PackageSource source1 = new PackageSource(p.Value, key, isEnabled);
                    PackageSource source2 = new PackageSource(p.Value, key, isEnabled);
                    source2.UserName = credential?.Username;
                    PackageSource local4 = source2;
                    PackageSource local5 = source2;
                    local5.Password = credential?.Password;
                    PackageSource local2 = local5;
                    PackageSource local3 = local5;
                    local3.IsPasswordClearText = (credential != null) && credential.IsPasswordClearText;
                    PackageSource local1 = local3;
                    local1.IsMachineWide = p.IsMachineWide;
                    return local1;
                }).ToList<PackageSource>();
                if (this._migratePackageSources != null)
                {
                    this.MigrateSources(loadedPackageSources);
                }
            }
            this.SetDefaultPackageSources(loadedPackageSources, machineWideSourcesCount);
            return loadedPackageSources;
        }

        private void MigrateSources(List<PackageSource> loadedPackageSources)
        {
            bool flag = false;
            List<PackageSource> list = new List<PackageSource>();
            for (int i = 0; i < loadedPackageSources.Count; i++)
            {
                PackageSource targetPackageSource;
                PackageSource key = loadedPackageSources[i];
                if (this._migratePackageSources.TryGetValue(key, out targetPackageSource))
                {
                    if (Enumerable.Any<PackageSource>(loadedPackageSources, p => p.Equals(targetPackageSource)))
                    {
                        list.Add(loadedPackageSources[i]);
                    }
                    else
                    {
                        loadedPackageSources[i] = targetPackageSource.Clone();
                        loadedPackageSources[i].IsEnabled = key.IsEnabled;
                    }
                    flag = true;
                }
            }
            foreach (PackageSource source2 in list)
            {
                loadedPackageSources.Remove(source2);
            }
            if (flag)
            {
                this.SavePackageSources(loadedPackageSources);
            }
        }

        private PackageSourceCredential ReadCredential(string sourceName)
        {
            PackageSourceCredential credential = this.ReadCredentialFromEnvironment(sourceName);
            if (credential != null)
            {
                return credential;
            }
            IList<KeyValuePair<string, string>> nestedValues = this._settingsManager.GetNestedValues("packageSourceCredentials", sourceName);
            if (!nestedValues.IsEmpty<KeyValuePair<string, string>>())
            {
                string str = Enumerable.FirstOrDefault<KeyValuePair<string, string>>(nestedValues, k => k.Key.Equals("Username", StringComparison.OrdinalIgnoreCase)).Value;
                if (!string.IsNullOrEmpty(str))
                {
                    string str2 = Enumerable.FirstOrDefault<KeyValuePair<string, string>>(nestedValues, k => k.Key.Equals("Password", StringComparison.OrdinalIgnoreCase)).Value;
                    if (!string.IsNullOrEmpty(str2))
                    {
                        return new PackageSourceCredential(str, EncryptionUtility.DecryptString(str2), false);
                    }
                    string str3 = Enumerable.FirstOrDefault<KeyValuePair<string, string>>(nestedValues, k => k.Key.Equals("ClearTextPassword", StringComparison.Ordinal)).Value;
                    if (!string.IsNullOrEmpty(str3))
                    {
                        return new PackageSourceCredential(str, str3, true);
                    }
                }
            }
            return null;
        }

        private PackageSourceCredential ReadCredentialFromEnvironment(string sourceName)
        {
            string environmentVariable = this._environment.GetEnvironmentVariable("NuGetPackageSourceCredentials_" + sourceName);
            if (string.IsNullOrEmpty(environmentVariable))
            {
                return null;
            }
            Match match = Regex.Match(environmentVariable.Trim(), @"^Username=(?<user>.*?);\s*Password=(?<pass>.*?)$", RegexOptions.IgnoreCase);
            return (match.Success ? new PackageSourceCredential(match.Groups["user"].Value, match.Groups["pass"].Value, true) : null);
        }

        private static KeyValuePair<string, string> ReadPasswordValues(PackageSource source) => 
            new KeyValuePair<string, string>(source.IsPasswordClearText ? "ClearTextPassword" : "Password", source.IsPasswordClearText ? source.Password : EncryptionUtility.EncryptString(source.Password));

        public void SavePackageSources(IEnumerable<PackageSource> sources)
        {
            this._settingsManager.DeleteSection("packageSources");
            this._settingsManager.SetValues("packageSources", (from p in sources
                where !p.IsMachineWide && p.IsPersistable
                select new KeyValuePair<string, string>(p.Name, p.Source)).ToList<KeyValuePair<string, string>>());
            this._settingsManager.DeleteSection("disabledPackageSources");
            this._settingsManager.SetValues("disabledPackageSources", (from p in sources
                where !p.IsEnabled
                select new KeyValuePair<string, string>(p.Name, "true")).ToList<KeyValuePair<string, string>>());
            this._settingsManager.DeleteSection("packageSourceCredentials");
            foreach (PackageSource source in from s in sources
                where !string.IsNullOrEmpty(s.UserName) && !string.IsNullOrEmpty(s.Password)
                select s)
            {
                KeyValuePair<string, string>[] values = new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("Username", source.UserName), ReadPasswordValues(source) };
                this._settingsManager.SetNestedValues("packageSourceCredentials", source.Name, values);
            }
            if (this.PackageSourcesSaved != null)
            {
                this.PackageSourcesSaved(this, EventArgs.Empty);
            }
        }

        private void SetDefaultPackageSources(List<PackageSource> loadedPackageSources, int machineWideSourcesCount)
        {
            IEnumerable<PackageSource> sequence = this._configurationDefaultSources;
            if (sequence.IsEmpty<PackageSource>())
            {
                this.UpdateProviderDefaultSources(loadedPackageSources);
                sequence = this._providerDefaultSources;
            }
            List<PackageSource> collection = new List<PackageSource>();
            foreach (PackageSource packageSource in sequence)
            {
                int num = loadedPackageSources.FindIndex(p => p.Source.Equals(packageSource.Source, StringComparison.OrdinalIgnoreCase));
                if (num != -1)
                {
                    if (!loadedPackageSources[num].Name.Equals(packageSource.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        continue;
                    }
                    loadedPackageSources[num].IsOfficial = true;
                    continue;
                }
                int num2 = loadedPackageSources.FindIndex(p => p.Name.Equals(packageSource.Name, StringComparison.CurrentCultureIgnoreCase));
                if (num2 != -1)
                {
                    loadedPackageSources[num2] = packageSource;
                    continue;
                }
                collection.Add(packageSource);
            }
            loadedPackageSources.InsertRange(loadedPackageSources.Count - machineWideSourcesCount, collection);
        }

        private void UpdateProviderDefaultSources(List<PackageSource> loadedSources)
        {
            bool flag = (loadedSources.Count == 0) || ((from p in loadedSources
                where !p.IsMachineWide
                select p).Count<PackageSource>() == 0);
            foreach (PackageSource local2 in this._providerDefaultSources)
            {
                local2.IsEnabled = flag;
                local2.IsOfficial = true;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PackageSourceProvider.<>c <>9 = new PackageSourceProvider.<>c();
            public static Func<SettingValue, string> <>9__15_0;
            public static Func<KeyValuePair<string, string>, bool> <>9__16_0;
            public static Func<KeyValuePair<string, string>, bool> <>9__16_1;
            public static Func<KeyValuePair<string, string>, bool> <>9__16_2;
            public static Func<PackageSource, bool> <>9__20_0;
            public static Func<PackageSource, bool> <>9__21_0;
            public static Func<PackageSource, KeyValuePair<string, string>> <>9__21_1;
            public static Func<PackageSource, bool> <>9__21_2;
            public static Func<PackageSource, KeyValuePair<string, string>> <>9__21_3;
            public static Func<PackageSource, bool> <>9__21_4;

            internal string <LoadPackageSources>b__15_0(SettingValue s) => 
                s.Key;

            internal bool <ReadCredential>b__16_0(KeyValuePair<string, string> k) => 
                k.Key.Equals("Username", StringComparison.OrdinalIgnoreCase);

            internal bool <ReadCredential>b__16_1(KeyValuePair<string, string> k) => 
                k.Key.Equals("Password", StringComparison.OrdinalIgnoreCase);

            internal bool <ReadCredential>b__16_2(KeyValuePair<string, string> k) => 
                k.Key.Equals("ClearTextPassword", StringComparison.Ordinal);

            internal bool <SavePackageSources>b__21_0(PackageSource p) => 
                (!p.IsMachineWide && p.IsPersistable);

            internal KeyValuePair<string, string> <SavePackageSources>b__21_1(PackageSource p) => 
                new KeyValuePair<string, string>(p.Name, p.Source);

            internal bool <SavePackageSources>b__21_2(PackageSource p) => 
                !p.IsEnabled;

            internal KeyValuePair<string, string> <SavePackageSources>b__21_3(PackageSource p) => 
                new KeyValuePair<string, string>(p.Name, "true");

            internal bool <SavePackageSources>b__21_4(PackageSource s) => 
                (!string.IsNullOrEmpty(s.UserName) && !string.IsNullOrEmpty(s.Password));

            internal bool <UpdateProviderDefaultSources>b__20_0(PackageSource p) => 
                !p.IsMachineWide;
        }

        private class PackageSourceCredential
        {
            public PackageSourceCredential(string username, string password, bool isPasswordClearText)
            {
                this.Username = username;
                this.Password = password;
                this.IsPasswordClearText = isPasswordClearText;
            }

            public string Username { get; private set; }

            public string Password { get; private set; }

            public bool IsPasswordClearText { get; private set; }
        }
    }
}

