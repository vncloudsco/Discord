namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    internal class NullSettings : ISettings
    {
        private static readonly NullSettings _settings = new NullSettings();

        public bool DeleteSection(string section)
        {
            object[] args = new object[] { "DeleteSection" };
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.InvalidNullSettingsOperation, args));
        }

        public bool DeleteValue(string section, string key)
        {
            object[] args = new object[] { "DeleteValue" };
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.InvalidNullSettingsOperation, args));
        }

        public IList<KeyValuePair<string, string>> GetNestedValues(string section, string key) => 
            new List<KeyValuePair<string, string>>().AsReadOnly();

        public IList<SettingValue> GetSettingValues(string section, bool isPath) => 
            new List<SettingValue>().AsReadOnly();

        public string GetValue(string section, string key) => 
            string.Empty;

        public string GetValue(string section, string key, bool isPath) => 
            string.Empty;

        public IList<KeyValuePair<string, string>> GetValues(string section) => 
            new List<KeyValuePair<string, string>>().AsReadOnly();

        public void SetNestedValues(string section, string key, IList<KeyValuePair<string, string>> values)
        {
            object[] args = new object[] { "SetNestedValues" };
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.InvalidNullSettingsOperation, args));
        }

        public void SetValue(string section, string key, string value)
        {
            object[] args = new object[] { "SetValue" };
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.InvalidNullSettingsOperation, args));
        }

        public void SetValues(string section, IList<KeyValuePair<string, string>> values)
        {
            object[] args = new object[] { "SetValues" };
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.InvalidNullSettingsOperation, args));
        }

        public static NullSettings Instance =>
            _settings;
    }
}

