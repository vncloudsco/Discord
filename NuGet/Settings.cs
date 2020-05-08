namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Xml;
    using System.Xml.Linq;

    internal class Settings : ISettings
    {
        private readonly XDocument _config;
        private readonly IFileSystem _fileSystem;
        private readonly string _fileName;
        private Settings _next;
        private readonly bool _isMachineWideSettings;
        private int _priority;

        public Settings(IFileSystem fileSystem) : this(fileSystem, Constants.SettingsFileName, false)
        {
        }

        public Settings(IFileSystem fileSystem, string fileName) : this(fileSystem, fileName, false)
        {
        }

        public Settings(IFileSystem fileSystem, string fileName, bool isMachineWideSettings)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException("fileSystem");
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "fileName");
            }
            this._fileSystem = fileSystem;
            this._fileName = fileName;
            XDocument conf = null;
            this.ExecuteSynchronized(() => conf = XmlUtility.GetOrCreateDocument("configuration", this._fileSystem, this._fileName));
            this._config = conf;
            this._isMachineWideSettings = isMachineWideSettings;
        }

        public bool DeleteSection(string section)
        {
            if (this.IsMachineWideSettings)
            {
                if (this._next == null)
                {
                    throw new InvalidOperationException(NuGetResources.Error_NoWritableConfig);
                }
                return this._next.DeleteSection(section);
            }
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "section");
            }
            XElement element = GetSection(this._config.Root, section);
            if (element == null)
            {
                return false;
            }
            element.RemoveIndented();
            this.Save();
            return true;
        }

        public bool DeleteValue(string section, string key)
        {
            if (this.IsMachineWideSettings)
            {
                if (this._next == null)
                {
                    throw new InvalidOperationException(NuGetResources.Error_NoWritableConfig);
                }
                return this._next.DeleteValue(section, key);
            }
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "section");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "key");
            }
            XElement sectionElement = GetSection(this._config.Root, section);
            if (sectionElement == null)
            {
                return false;
            }
            XElement element = FindElementByKey(sectionElement, key, null);
            if (element == null)
            {
                return false;
            }
            element.RemoveIndented();
            this.Save();
            return true;
        }

        private string ElementToValue(XElement element, bool isPath)
        {
            if (element == null)
            {
                return null;
            }
            string str = element.GetOptionalAttributeValue("value", null);
            return ((!isPath || string.IsNullOrEmpty(str)) ? str : this._fileSystem.GetFullPath(ResolvePath(Path.GetDirectoryName(this.ConfigFilePath), str)));
        }

        private void ExecuteSynchronized(Action ioOperation)
        {
            string fullPath = this._fileSystem.GetFullPath(this._fileName);
            using (Mutex mutex = new Mutex(false, @"Global\" + EncryptionUtility.GenerateUniqueToken(fullPath)))
            {
                bool flag = false;
                try
                {
                    flag = mutex.WaitOne(TimeSpan.FromMinutes(1.0));
                    ioOperation();
                }
                finally
                {
                    if (flag)
                    {
                        mutex.ReleaseMutex();
                    }
                }
            }
        }

        private static XElement FindElementByKey(XElement sectionElement, string key, XElement curr)
        {
            XElement element = curr;
            foreach (XElement element2 in sectionElement.Elements())
            {
                string localName = element2.Name.LocalName;
                if (localName.Equals("clear", StringComparison.OrdinalIgnoreCase))
                {
                    element = null;
                    continue;
                }
                if (localName.Equals("add", StringComparison.OrdinalIgnoreCase) && element2.GetOptionalAttributeValue("key", null).Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    element = element2;
                }
            }
            return element;
        }

        public IList<KeyValuePair<string, string>> GetNestedValues(string section, string key)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "section");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "key");
            }
            List<SettingValue> current = new List<SettingValue>();
            for (Settings settings = this; settings != null; settings = settings._next)
            {
                settings.PopulateNestedValues(section, key, current);
            }
            return (from v in current select new KeyValuePair<string, string>(v.Key, v.Value)).ToList<KeyValuePair<string, string>>().AsReadOnly();
        }

        private static XElement GetOrCreateSection(XElement parentElement, string sectionName)
        {
            sectionName = XmlConvert.EncodeLocalName(sectionName);
            XElement content = parentElement.Element(sectionName);
            if (content == null)
            {
                content = new XElement(sectionName);
                parentElement.AddIndented(content);
            }
            return content;
        }

        private static XElement GetSection(XElement parentElement, string section)
        {
            section = XmlConvert.EncodeLocalName(section);
            return parentElement.Element(section);
        }

        [IteratorStateMachine(typeof(<GetSettingsFileNames>d__39))]
        private static IEnumerable<string> GetSettingsFileNames(IFileSystem fileSystem)
        {
            <GetSettingsFileNames>d__39 d__1 = new <GetSettingsFileNames>d__39(-2);
            d__1.<>3__fileSystem = fileSystem;
            return d__1;
        }

        [IteratorStateMachine(typeof(<GetSettingsFilePaths>d__40))]
        private static IEnumerable<string> GetSettingsFilePaths(IFileSystem fileSystem)
        {
            <GetSettingsFilePaths>d__40 d__1 = new <GetSettingsFilePaths>d__40(-2);
            d__1.<>3__fileSystem = fileSystem;
            return d__1;
        }

        public IList<SettingValue> GetSettingValues(string section, bool isPath)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "section");
            }
            List<SettingValue> current = new List<SettingValue>();
            for (Settings settings = this; settings != null; settings = settings._next)
            {
                settings.PopulateValues(section, current, isPath);
            }
            return current.AsReadOnly();
        }

        public string GetValue(string section, string key) => 
            this.GetValue(section, key, false);

        public string GetValue(string section, string key, bool isPath)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "section");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "key");
            }
            XElement curr = null;
            string str = null;
            for (Settings settings = this; settings != null; settings = settings._next)
            {
                XElement objB = settings.GetValueInternal(section, key, curr);
                if (!ReferenceEquals(curr, objB))
                {
                    curr = objB;
                    str = settings.ElementToValue(curr, isPath);
                }
            }
            return str;
        }

        private XElement GetValueInternal(string section, string key, XElement curr)
        {
            XElement sectionElement = GetSection(this._config.Root, section);
            return ((sectionElement != null) ? FindElementByKey(sectionElement, key, curr) : curr);
        }

        public IList<KeyValuePair<string, string>> GetValues(string section) => 
            this.GetValues(section, false);

        private IList<KeyValuePair<string, string>> GetValues(string section, bool isPath) => 
            (from v in this.GetSettingValues(section, isPath) select new KeyValuePair<string, string>(v.Key, v.Value)).ToList<KeyValuePair<string, string>>().AsReadOnly();

        public static ISettings LoadDefaultSettings(IFileSystem fileSystem, string configFileName, IMachineWideSettings machineWideSettings)
        {
            List<Settings> validSettingFiles = new List<Settings>();
            if (fileSystem != null)
            {
                validSettingFiles.AddRange(from f in GetSettingsFileNames(fileSystem)
                    select ReadSettings(fileSystem, f) into f
                    where f != null
                    select f);
            }
            LoadUserSpecificSettings(validSettingFiles, fileSystem, configFileName);
            if (machineWideSettings != null)
            {
                validSettingFiles.AddRange(from s in machineWideSettings.Settings select new Settings(s._fileSystem, s._fileName, s._isMachineWideSettings));
            }
            if (validSettingFiles.IsEmpty<Settings>())
            {
                return NullSettings.Instance;
            }
            validSettingFiles[0]._priority = validSettingFiles.Count;
            for (int i = 1; i < validSettingFiles.Count; i++)
            {
                validSettingFiles[i]._next = validSettingFiles[i - 1];
                validSettingFiles[i]._priority = validSettingFiles[i - 1]._priority - 1;
            }
            return validSettingFiles.Last<Settings>();
        }

        public static IEnumerable<Settings> LoadMachineWideSettings(IFileSystem fileSystem, params string[] paths)
        {
            List<Settings> list = new List<Settings>();
            string str = @"NuGet\Config";
            string str2 = Path.Combine(paths);
            while (true)
            {
                string path = Path.Combine(str, str2);
                foreach (string str4 in fileSystem.GetFiles(path, "*.config"))
                {
                    Settings item = ReadSettings(fileSystem, str4, true);
                    if (item != null)
                    {
                        list.Add(item);
                    }
                }
                if (str2.Length == 0)
                {
                    return list;
                }
                int length = str2.LastIndexOf(Path.DirectorySeparatorChar);
                if (length < 0)
                {
                    length = 0;
                }
                str2 = str2.Substring(0, length);
            }
        }

        private static void LoadUserSpecificSettings(List<Settings> validSettingFiles, IFileSystem fileSystem, string configFileName)
        {
            Settings item = null;
            if (configFileName == null)
            {
                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (!string.IsNullOrEmpty(folderPath))
                {
                    string settingsPath = Path.Combine(folderPath, "NuGet", Constants.SettingsFileName);
                    item = ReadSettings(fileSystem ?? new PhysicalFileSystem(@"c:\"), settingsPath);
                }
            }
            else
            {
                if (!fileSystem.FileExists(configFileName))
                {
                    object[] args = new object[] { fileSystem.GetFullPath(configFileName) };
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.FileDoesNotExit, args));
                }
                item = ReadSettings(fileSystem, configFileName);
            }
            if (item != null)
            {
                validSettingFiles.Add(item);
            }
        }

        private void PopulateNestedValues(string section, string key, List<SettingValue> current)
        {
            XElement parentElement = GetSection(this._config.Root, section);
            if (parentElement != null)
            {
                XElement sectionElement = GetSection(parentElement, key);
                if (sectionElement != null)
                {
                    this.ReadSection(sectionElement, current, false);
                }
            }
        }

        private void PopulateValues(string section, List<SettingValue> current, bool isPath)
        {
            XElement sectionElement = GetSection(this._config.Root, section);
            if (sectionElement != null)
            {
                this.ReadSection(sectionElement, current, isPath);
            }
        }

        private void ReadSection(XContainer sectionElement, ICollection<SettingValue> values, bool isPath)
        {
            foreach (XElement element in sectionElement.Elements())
            {
                string localName = element.Name.LocalName;
                if (localName.Equals("add", StringComparison.OrdinalIgnoreCase))
                {
                    KeyValuePair<string, string> pair = this.ReadValue(element, isPath);
                    values.Add(new SettingValue(pair.Key, pair.Value, this._isMachineWideSettings, this._priority));
                    continue;
                }
                if (localName.Equals("clear", StringComparison.OrdinalIgnoreCase))
                {
                    values.Clear();
                }
            }
        }

        private static Settings ReadSettings(IFileSystem fileSystem, string settingsPath) => 
            ReadSettings(fileSystem, settingsPath, false);

        private static Settings ReadSettings(IFileSystem fileSystem, string settingsPath, bool isMachineWideSettings)
        {
            try
            {
                return new Settings(fileSystem, settingsPath, isMachineWideSettings);
            }
            catch (XmlException)
            {
                return null;
            }
        }

        private KeyValuePair<string, string> ReadValue(XElement element, bool isPath)
        {
            Uri uri;
            XAttribute attribute = element.Attribute("key");
            XAttribute attribute2 = element.Attribute("value");
            if ((attribute == null) || (string.IsNullOrEmpty(attribute.Value) || (attribute2 == null)))
            {
                object[] args = new object[] { this.ConfigFilePath };
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, NuGetResources.UserSettings_UnableToParseConfigFile, args));
            }
            string uriString = attribute2.Value;
            if (isPath && Uri.TryCreate(uriString, UriKind.Relative, out uri))
            {
                string directoryName = Path.GetDirectoryName(this.ConfigFilePath);
                uriString = this._fileSystem.GetFullPath(Path.Combine(directoryName, uriString));
            }
            return new KeyValuePair<string, string>(attribute.Value, uriString);
        }

        private static string ResolvePath(string configDirectory, string value)
        {
            string pathRoot = Path.GetPathRoot(value);
            return (((pathRoot == null) || ((pathRoot.Length != 1) || ((pathRoot[0] != Path.DirectorySeparatorChar) && (value[0] != Path.AltDirectorySeparatorChar)))) ? Path.Combine(configDirectory, value) : Path.Combine(Path.GetPathRoot(configDirectory), value.Substring(1)));
        }

        private void Save()
        {
            this.ExecuteSynchronized(() => this._fileSystem.AddFile(this._fileName, new Action<Stream>(this._config.Save)));
        }

        public void SetNestedValues(string section, string key, IList<KeyValuePair<string, string>> values)
        {
            if (this.IsMachineWideSettings)
            {
                if (this._next == null)
                {
                    throw new InvalidOperationException(NuGetResources.Error_NoWritableConfig);
                }
                this._next.SetNestedValues(section, key, values);
            }
            else
            {
                if (string.IsNullOrEmpty(section))
                {
                    throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "section");
                }
                if (values == null)
                {
                    throw new ArgumentNullException("values");
                }
                XElement orCreateSection = GetOrCreateSection(GetOrCreateSection(this._config.Root, section), key);
                foreach (KeyValuePair<string, string> pair in values)
                {
                    this.SetValueInternal(orCreateSection, pair.Key, pair.Value);
                }
                this.Save();
            }
        }

        public void SetValue(string section, string key, string value)
        {
            if (this.IsMachineWideSettings)
            {
                if (this._next == null)
                {
                    throw new InvalidOperationException(NuGetResources.Error_NoWritableConfig);
                }
                this._next.SetValue(section, key, value);
            }
            else
            {
                if (string.IsNullOrEmpty(section))
                {
                    throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "section");
                }
                XElement orCreateSection = GetOrCreateSection(this._config.Root, section);
                this.SetValueInternal(orCreateSection, key, value);
                this.Save();
            }
        }

        private void SetValueInternal(XElement sectionElement, string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "key");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            XElement element = FindElementByKey(sectionElement, key, null);
            if (element != null)
            {
                element.SetAttributeValue("value", value);
                this.Save();
            }
            else
            {
                object[] content = new object[] { new XAttribute("key", key), new XAttribute("value", value) };
                sectionElement.AddIndented(new XElement("add", content));
            }
        }

        public void SetValues(string section, IList<KeyValuePair<string, string>> values)
        {
            if (this.IsMachineWideSettings)
            {
                if (this._next == null)
                {
                    throw new InvalidOperationException(NuGetResources.Error_NoWritableConfig);
                }
                this._next.SetValues(section, values);
            }
            else
            {
                if (string.IsNullOrEmpty(section))
                {
                    throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "section");
                }
                if (values == null)
                {
                    throw new ArgumentNullException("values");
                }
                XElement orCreateSection = GetOrCreateSection(this._config.Root, section);
                foreach (KeyValuePair<string, string> pair in values)
                {
                    this.SetValueInternal(orCreateSection, pair.Key, pair.Value);
                }
                this.Save();
            }
        }

        public bool IsMachineWideSettings =>
            this._isMachineWideSettings;

        public string ConfigFilePath =>
            (Path.IsPathRooted(this._fileName) ? this._fileName : Path.GetFullPath(Path.Combine(this._fileSystem.Root, this._fileName)));

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly Settings.<>c <>9 = new Settings.<>c();
            public static Func<Settings, bool> <>9__13_1;
            public static Func<Settings, Settings> <>9__13_2;
            public static Func<SettingValue, KeyValuePair<string, string>> <>9__22_0;
            public static Func<SettingValue, KeyValuePair<string, string>> <>9__25_0;

            internal KeyValuePair<string, string> <GetNestedValues>b__25_0(SettingValue v) => 
                new KeyValuePair<string, string>(v.Key, v.Value);

            internal KeyValuePair<string, string> <GetValues>b__22_0(SettingValue v) => 
                new KeyValuePair<string, string>(v.Key, v.Value);

            internal bool <LoadDefaultSettings>b__13_1(Settings f) => 
                (f != null);

            internal Settings <LoadDefaultSettings>b__13_2(Settings s) => 
                new Settings(s._fileSystem, s._fileName, s._isMachineWideSettings);
        }

        [CompilerGenerated]
        private sealed class <GetSettingsFileNames>d__39 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private string <>2__current;
            private int <>l__initialThreadId;
            private IFileSystem fileSystem;
            public IFileSystem <>3__fileSystem;
            private IEnumerator<string> <>7__wrap1;

            [DebuggerHidden]
            public <GetSettingsFileNames>d__39(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap1 != null)
                {
                    this.<>7__wrap1.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    int num = this.<>1__state;
                    if (num == 0)
                    {
                        this.<>1__state = -1;
                        this.<>7__wrap1 = Settings.GetSettingsFilePaths(this.fileSystem).GetEnumerator();
                        this.<>1__state = -3;
                    }
                    else if (num == 1)
                    {
                        this.<>1__state = -3;
                    }
                    else
                    {
                        return false;
                    }
                    while (true)
                    {
                        if (!this.<>7__wrap1.MoveNext())
                        {
                            this.<>m__Finally1();
                            this.<>7__wrap1 = null;
                            flag = false;
                        }
                        else
                        {
                            string path = Path.Combine(this.<>7__wrap1.Current, Constants.SettingsFileName);
                            if (!this.fileSystem.FileExists(path))
                            {
                                continue;
                            }
                            this.<>2__current = path;
                            this.<>1__state = 1;
                            flag = true;
                        }
                        break;
                    }
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<string> IEnumerable<string>.GetEnumerator()
            {
                Settings.<GetSettingsFileNames>d__39 d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new Settings.<GetSettingsFileNames>d__39(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                d__.fileSystem = this.<>3__fileSystem;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<System.String>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                int num = this.<>1__state;
                if ((num == -3) || (num == 1))
                {
                    try
                    {
                    }
                    finally
                    {
                        this.<>m__Finally1();
                    }
                }
            }

            string IEnumerator<string>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }

        [CompilerGenerated]
        private sealed class <GetSettingsFilePaths>d__40 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private string <>2__current;
            private int <>l__initialThreadId;
            private IFileSystem fileSystem;
            public IFileSystem <>3__fileSystem;
            private string <root>5__1;

            [DebuggerHidden]
            public <GetSettingsFilePaths>d__40(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private bool MoveNext()
            {
                int num = this.<>1__state;
                if (num == 0)
                {
                    this.<>1__state = -1;
                    this.<root>5__1 = this.fileSystem.Root;
                }
                else
                {
                    if (num != 1)
                    {
                        return false;
                    }
                    this.<>1__state = -1;
                    this.<root>5__1 = Path.GetDirectoryName(this.<root>5__1);
                }
                if (this.<root>5__1 == null)
                {
                    return false;
                }
                this.<>2__current = this.<root>5__1;
                this.<>1__state = 1;
                return true;
            }

            [DebuggerHidden]
            IEnumerator<string> IEnumerable<string>.GetEnumerator()
            {
                Settings.<GetSettingsFilePaths>d__40 d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new Settings.<GetSettingsFilePaths>d__40(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                d__.fileSystem = this.<>3__fileSystem;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<System.String>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
            }

            string IEnumerator<string>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

