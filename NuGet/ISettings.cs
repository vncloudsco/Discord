namespace NuGet
{
    using System;
    using System.Collections.Generic;

    internal interface ISettings
    {
        bool DeleteSection(string section);
        bool DeleteValue(string section, string key);
        IList<KeyValuePair<string, string>> GetNestedValues(string section, string key);
        IList<SettingValue> GetSettingValues(string section, bool isPath);
        string GetValue(string section, string key);
        string GetValue(string section, string key, bool isPath);
        IList<KeyValuePair<string, string>> GetValues(string section);
        void SetNestedValues(string section, string key, IList<KeyValuePair<string, string>> values);
        void SetValue(string section, string key, string value);
        void SetValues(string section, IList<KeyValuePair<string, string>> values);
    }
}

