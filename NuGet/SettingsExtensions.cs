namespace NuGet
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static class SettingsExtensions
    {
        private const string ConfigSection = "config";

        public static bool DeleteConfigValue(this ISettings settings, string key) => 
            settings.DeleteValue("config", key);

        public static string GetConfigValue(this ISettings settings, string key, bool decrypt = false, bool isPath = false) => 
            (decrypt ? settings.GetDecryptedValue("config", key, isPath) : settings.GetValue("config", key, isPath));

        public static string GetDecryptedValue(this ISettings settings, string section, string key, bool isPath = false)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "section");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "key");
            }
            string str = settings.GetValue(section, key, isPath);
            return ((str != null) ? (!string.IsNullOrEmpty(str) ? EncryptionUtility.DecryptString(str) : string.Empty) : null);
        }

        public static string GetRepositoryPath(this ISettings settings)
        {
            string str = settings.GetValue("config", "repositoryPath", true);
            if (!string.IsNullOrEmpty(str))
            {
                str = str.Replace('/', Path.DirectorySeparatorChar);
            }
            return str;
        }

        public static void SetConfigValue(this ISettings settings, string key, string value, bool encrypt = false)
        {
            if (encrypt)
            {
                settings.SetEncryptedValue("config", key, value);
            }
            else
            {
                settings.SetValue("config", key, value);
            }
        }

        public static void SetEncryptedValue(this ISettings settings, string section, string key, string value)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "section");
            }
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "key");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (string.IsNullOrEmpty(value))
            {
                settings.SetValue(section, key, string.Empty);
            }
            else
            {
                string str = EncryptionUtility.EncryptString(value);
                settings.SetValue(section, key, str);
            }
        }
    }
}

