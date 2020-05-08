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
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Text;
    using System.Text.RegularExpressions;

    internal static class VersionUtility
    {
        private const string NetFrameworkIdentifier = ".NETFramework";
        private const string NetCoreFrameworkIdentifier = ".NETCore";
        private const string PortableFrameworkIdentifier = ".NETPortable";
        private const string AspNetFrameworkIdentifier = "ASP.NET";
        private const string AspNetCoreFrameworkIdentifier = "ASP.NETCore";
        private const string LessThanOrEqualTo = "≤";
        private const string GreaterThanOrEqualTo = "≥";
        public static readonly FrameworkName EmptyFramework = new FrameworkName("NoFramework", new Version());
        public static readonly FrameworkName NativeProjectFramework = new FrameworkName("Native", new Version());
        public static readonly FrameworkName UnsupportedFrameworkName = new FrameworkName("Unsupported", new Version());
        private static readonly Version _emptyVersion = new Version();
        private static readonly Dictionary<string, string> _knownIdentifiers;
        private static readonly Dictionary<string, string> _knownProfiles;
        private static readonly Dictionary<string, string> _identifierToFrameworkFolder;
        private static readonly Dictionary<string, string> _identifierToProfileFolder;
        private static readonly Dictionary<string, Dictionary<string, string[]>> _compatibiltyMapping;
        private static readonly Dictionary<FrameworkName, FrameworkName> _frameworkNameAlias;
        private static readonly Version MaxVersion;
        private static readonly Dictionary<string, FrameworkName> _equivalentProjectFrameworks;

        static VersionUtility()
        {
            Dictionary<string, string> dictionary1 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dictionary1.Add("NET", ".NETFramework");
            dictionary1.Add(".NET", ".NETFramework");
            dictionary1.Add("NETFramework", ".NETFramework");
            dictionary1.Add(".NETFramework", ".NETFramework");
            dictionary1.Add("NETCore", ".NETCore");
            dictionary1.Add(".NETCore", ".NETCore");
            dictionary1.Add("WinRT", ".NETCore");
            dictionary1.Add(".NETMicroFramework", ".NETMicroFramework");
            dictionary1.Add("netmf", ".NETMicroFramework");
            dictionary1.Add("SL", "Silverlight");
            dictionary1.Add("Silverlight", "Silverlight");
            dictionary1.Add(".NETPortable", ".NETPortable");
            dictionary1.Add("NETPortable", ".NETPortable");
            dictionary1.Add("portable", ".NETPortable");
            dictionary1.Add("wp", "WindowsPhone");
            dictionary1.Add("WindowsPhone", "WindowsPhone");
            dictionary1.Add("WindowsPhoneApp", "WindowsPhoneApp");
            dictionary1.Add("wpa", "WindowsPhoneApp");
            dictionary1.Add("Windows", "Windows");
            dictionary1.Add("win", "Windows");
            dictionary1.Add("aspnet", "ASP.NET");
            dictionary1.Add("aspnetcore", "ASP.NETCore");
            dictionary1.Add("asp.net", "ASP.NET");
            dictionary1.Add("asp.netcore", "ASP.NETCore");
            dictionary1.Add("native", "native");
            dictionary1.Add("MonoAndroid", "MonoAndroid");
            dictionary1.Add("MonoTouch", "MonoTouch");
            dictionary1.Add("MonoMac", "MonoMac");
            dictionary1.Add("Xamarin.iOS", "Xamarin.iOS");
            dictionary1.Add("XamariniOS", "Xamarin.iOS");
            dictionary1.Add("Xamarin.Mac", "Xamarin.Mac");
            dictionary1.Add("XamarinMac", "Xamarin.Mac");
            dictionary1.Add("Xamarin.PlayStationThree", "Xamarin.PlayStation3");
            dictionary1.Add("XamarinPlayStationThree", "Xamarin.PlayStation3");
            dictionary1.Add("XamarinPSThree", "Xamarin.PlayStation3");
            dictionary1.Add("Xamarin.PlayStationFour", "Xamarin.PlayStation4");
            dictionary1.Add("XamarinPlayStationFour", "Xamarin.PlayStation4");
            dictionary1.Add("XamarinPSFour", "Xamarin.PlayStation4");
            dictionary1.Add("Xamarin.PlayStationVita", "Xamarin.PlayStationVita");
            dictionary1.Add("XamarinPlayStationVita", "Xamarin.PlayStationVita");
            dictionary1.Add("XamarinPSVita", "Xamarin.PlayStationVita");
            dictionary1.Add("Xamarin.XboxThreeSixty", "Xamarin.Xbox360");
            dictionary1.Add("XamarinXboxThreeSixty", "Xamarin.Xbox360");
            dictionary1.Add("Xamarin.XboxOne", "Xamarin.XboxOne");
            dictionary1.Add("XamarinXboxOne", "Xamarin.XboxOne");
            _knownIdentifiers = dictionary1;
            Dictionary<string, string> dictionary3 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dictionary3.Add("Client", "Client");
            dictionary3.Add("WP", "WindowsPhone");
            dictionary3.Add("WP71", "WindowsPhone71");
            dictionary3.Add("CF", "CompactFramework");
            dictionary3.Add("Full", string.Empty);
            _knownProfiles = dictionary3;
            Dictionary<string, string> dictionary4 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dictionary4.Add(".NETFramework", "net");
            dictionary4.Add(".NETMicroFramework", "netmf");
            dictionary4.Add("ASP.NET", "aspnet");
            dictionary4.Add("ASP.NETCore", "aspnetcore");
            dictionary4.Add("Silverlight", "sl");
            dictionary4.Add(".NETCore", "win");
            dictionary4.Add("Windows", "win");
            dictionary4.Add(".NETPortable", "portable");
            dictionary4.Add("WindowsPhone", "wp");
            dictionary4.Add("WindowsPhoneApp", "wpa");
            dictionary4.Add("Xamarin.iOS", "xamarinios");
            dictionary4.Add("Xamarin.Mac", "xamarinmac");
            dictionary4.Add("Xamarin.PlayStation3", "xamarinpsthree");
            dictionary4.Add("Xamarin.PlayStation4", "xamarinpsfour");
            dictionary4.Add("Xamarin.PlayStationVita", "xamarinpsvita");
            dictionary4.Add("Xamarin.Xbox360", "xamarinxboxthreesixty");
            dictionary4.Add("Xamarin.XboxOne", "xamarinxboxone");
            _identifierToFrameworkFolder = dictionary4;
            Dictionary<string, string> dictionary5 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dictionary5.Add("WindowsPhone", "wp");
            dictionary5.Add("WindowsPhone71", "wp71");
            dictionary5.Add("CompactFramework", "cf");
            _identifierToProfileFolder = dictionary5;
            Dictionary<string, Dictionary<string, string[]>> dictionary = new Dictionary<string, Dictionary<string, string[]>>(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, string[]> dictionary2 = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            string[] textArray1 = new string[] { "Client" };
            dictionary2.Add("", textArray1);
            string[] textArray2 = new string[] { "" };
            dictionary2.Add("Client", textArray2);
            dictionary.Add(".NETFramework", dictionary2);
            dictionary2 = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            string[] textArray3 = new string[] { "WindowsPhone71" };
            dictionary2.Add("WindowsPhone", textArray3);
            string[] textArray4 = new string[] { "WindowsPhone" };
            dictionary2.Add("WindowsPhone71", textArray4);
            dictionary.Add("Silverlight", dictionary2);
            _compatibiltyMapping = dictionary;
            Dictionary<FrameworkName, FrameworkName> dictionary6 = new Dictionary<FrameworkName, FrameworkName>(FrameworkNameEqualityComparer.Default);
            dictionary6.Add(new FrameworkName("WindowsPhone, Version=v0.0"), new FrameworkName("Silverlight, Version=v3.0, Profile=WindowsPhone"));
            dictionary6.Add(new FrameworkName("WindowsPhone, Version=v7.0"), new FrameworkName("Silverlight, Version=v3.0, Profile=WindowsPhone"));
            dictionary6.Add(new FrameworkName("WindowsPhone, Version=v7.1"), new FrameworkName("Silverlight, Version=v4.0, Profile=WindowsPhone71"));
            dictionary6.Add(new FrameworkName("WindowsPhone, Version=v8.0"), new FrameworkName("Silverlight, Version=v8.0, Profile=WindowsPhone"));
            dictionary6.Add(new FrameworkName("WindowsPhone, Version=v8.1"), new FrameworkName("Silverlight, Version=v8.1, Profile=WindowsPhone"));
            dictionary6.Add(new FrameworkName("Windows, Version=v0.0"), new FrameworkName(".NETCore, Version=v4.5"));
            dictionary6.Add(new FrameworkName("Windows, Version=v8.0"), new FrameworkName(".NETCore, Version=v4.5"));
            dictionary6.Add(new FrameworkName("Windows, Version=v8.1"), new FrameworkName(".NETCore, Version=v4.5.1"));
            _frameworkNameAlias = dictionary6;
            MaxVersion = new Version(0x7fffffff, 0x7fffffff, 0x7fffffff, 0x7fffffff);
            Dictionary<string, FrameworkName> dictionary7 = new Dictionary<string, FrameworkName>();
            dictionary7.Add("ASP.NET", new FrameworkName(".NETFramework", MaxVersion));
            _equivalentProjectFrameworks = dictionary7;
        }

        private static long CalculateVersionDistance(Version projectVersion, Version targetFrameworkVersion) => 
            (0x2000000000L - (((((((projectVersion.Major - targetFrameworkVersion.Major) * 0xffL) * 0xffL) * 0xffL) + (((projectVersion.Minor - targetFrameworkVersion.Minor) * 0xffL) * 0xffL)) + ((projectVersion.Build - targetFrameworkVersion.Build) * 0xffL)) + (projectVersion.Revision - targetFrameworkVersion.Revision)));

        internal static long GetCompatibilityBetweenPortableLibraryAndNonPortableLibrary(FrameworkName projectFrameworkName, FrameworkName packagePortableFramework) => 
            GetCompatibilityBetweenPortableLibraryAndNonPortableLibrary(projectFrameworkName, packagePortableFramework, NetPortableProfileTable.Default);

        internal static long GetCompatibilityBetweenPortableLibraryAndNonPortableLibrary(FrameworkName projectFrameworkName, FrameworkName packagePortableFramework, NetPortableProfileTable portableProfileTable)
        {
            NetPortableProfile packageFramework = NetPortableProfile.Parse(packagePortableFramework.get_Profile(), true, portableProfileTable);
            if (packageFramework == null)
            {
                return -9223372036854775808L;
            }
            FrameworkName packageTargetFrameworkName = Enumerable.FirstOrDefault<FrameworkName>(packageFramework.SupportedFrameworks, (Func<FrameworkName, bool>) (f => IsCompatible(projectFrameworkName, f, portableProfileTable)));
            return ((packageTargetFrameworkName == null) ? (!portableProfileTable.HasCompatibleProfileWith(packageFramework, projectFrameworkName, portableProfileTable) ? -9223372036854775808L : ((long) (0 - (packageFramework.SupportedFrameworks.Count * 2)))) : (GetProfileCompatibility(projectFrameworkName, packageTargetFrameworkName, portableProfileTable) - (packageFramework.SupportedFrameworks.Count * 2)));
        }

        internal static int GetCompatibilityBetweenPortableLibraryAndPortableLibrary(FrameworkName projectFrameworkName, FrameworkName packageTargetFrameworkName) => 
            GetCompatibilityBetweenPortableLibraryAndPortableLibrary(projectFrameworkName, packageTargetFrameworkName, NetPortableProfileTable.Default);

        internal static int GetCompatibilityBetweenPortableLibraryAndPortableLibrary(FrameworkName projectFrameworkName, FrameworkName packageTargetFrameworkName, NetPortableProfileTable portableProfileTable)
        {
            NetPortableProfile profile = NetPortableProfile.Parse(projectFrameworkName.get_Profile(), false, portableProfileTable);
            NetPortableProfile profile2 = NetPortableProfile.Parse(packageTargetFrameworkName.get_Profile(), true, portableProfileTable);
            int num = 0;
            int num2 = 0;
            foreach (FrameworkName supportedPackageTargetFramework in profile2.SupportedFrameworks)
            {
                FrameworkName name = Enumerable.FirstOrDefault<FrameworkName>(profile.SupportedFrameworks, (Func<FrameworkName, bool>) (f => IsCompatible(f, supportedPackageTargetFramework, portableProfileTable)));
                if ((name != null) && (name.get_Version() > supportedPackageTargetFramework.get_Version()))
                {
                    num++;
                }
            }
            foreach (FrameworkName optionalProjectFramework in profile.OptionalFrameworks)
            {
                FrameworkName name2 = Enumerable.FirstOrDefault<FrameworkName>(profile2.SupportedFrameworks, (Func<FrameworkName, bool>) (f => IsCompatible(f, optionalProjectFramework, portableProfileTable)));
                if ((name2 == null) || (name2.get_Version() > optionalProjectFramework.get_Version()))
                {
                    num2++;
                    continue;
                }
                if ((name2 != null) && (name2.get_Version() < optionalProjectFramework.get_Version()))
                {
                    num++;
                }
            }
            return -((((((1 + profile.SupportedFrameworks.Count) + profile.OptionalFrameworks.Count) * num2) + num) * 50) + profile2.SupportedFrameworks.Count);
        }

        private static Version GetEffectiveFrameworkVersion(FrameworkName projectFramework, FrameworkName targetFrameworkVersion, NetPortableProfileTable portableProfileTable)
        {
            if (targetFrameworkVersion.IsPortableFramework())
            {
                NetPortableProfile profile = NetPortableProfile.Parse(targetFrameworkVersion.get_Profile(), false, portableProfileTable);
                if (profile != null)
                {
                    FrameworkName name = Enumerable.FirstOrDefault<FrameworkName>(profile.SupportedFrameworks, (Func<FrameworkName, bool>) (f => IsCompatible(projectFramework, f, portableProfileTable)));
                    if (name != null)
                    {
                        return name.get_Version();
                    }
                }
            }
            return targetFrameworkVersion.get_Version();
        }

        public static string GetFrameworkString(FrameworkName frameworkName)
        {
            string str = frameworkName.get_Identifier() + frameworkName.get_Version();
            return (!string.IsNullOrEmpty(frameworkName.get_Profile()) ? (str + "-" + frameworkName.get_Profile()) : str);
        }

        [IteratorStateMachine(typeof(<GetPossibleVersions>d__41))]
        public static IEnumerable<SemanticVersion> GetPossibleVersions(SemanticVersion semver)
        {
            Version version = TrimVersion(semver.Version);
            yield return new SemanticVersion(version, semver.SpecialVersion);
            if ((version.Build == -1) && (version.Revision == -1))
            {
                yield return new SemanticVersion(new Version(version.Major, version.Minor, 0), semver.SpecialVersion);
                yield return new SemanticVersion(new Version(version.Major, version.Minor, 0, 0), semver.SpecialVersion);
            }
            else if (version.Revision == -1)
            {
                yield return new SemanticVersion(new Version(version.Major, version.Minor, version.Build, 0), semver.SpecialVersion);
            }
        }

        private static long GetProfileCompatibility(FrameworkName projectFrameworkName, FrameworkName packageTargetFrameworkName, NetPortableProfileTable portableProfileTable)
        {
            projectFrameworkName = NormalizeFrameworkName(projectFrameworkName);
            packageTargetFrameworkName = NormalizeFrameworkName(packageTargetFrameworkName);
            if (packageTargetFrameworkName.IsPortableFramework())
            {
                return (!projectFrameworkName.IsPortableFramework() ? (GetCompatibilityBetweenPortableLibraryAndNonPortableLibrary(projectFrameworkName, packageTargetFrameworkName, portableProfileTable) / 2L) : ((long) GetCompatibilityBetweenPortableLibraryAndPortableLibrary(projectFrameworkName, packageTargetFrameworkName, portableProfileTable)));
            }
            long num = 0L + CalculateVersionDistance(projectFrameworkName.get_Version(), GetEffectiveFrameworkVersion(projectFrameworkName, packageTargetFrameworkName, portableProfileTable));
            if (packageTargetFrameworkName.get_Profile().Equals(projectFrameworkName.get_Profile(), StringComparison.OrdinalIgnoreCase))
            {
                num += 1L;
            }
            if (packageTargetFrameworkName.get_Identifier().Equals(projectFrameworkName.get_Identifier(), StringComparison.OrdinalIgnoreCase))
            {
                num += 0xa00000000L;
            }
            return num;
        }

        public static IVersionSpec GetSafeRange(SemanticVersion version)
        {
            VersionSpec spec1 = new VersionSpec();
            spec1.IsMinInclusive = true;
            spec1.MinVersion = version;
            spec1.MaxVersion = new SemanticVersion(new Version(version.Version.Major, version.Version.Minor + 1));
            return spec1;
        }

        public static string GetShortFrameworkName(FrameworkName frameworkName) => 
            GetShortFrameworkName(frameworkName, NetPortableProfileTable.Default);

        public static string GetShortFrameworkName(FrameworkName frameworkName, NetPortableProfileTable portableProfileTable)
        {
            string str;
            string str2;
            if (frameworkName == null)
            {
                throw new ArgumentNullException("frameworkName");
            }
            foreach (KeyValuePair<FrameworkName, FrameworkName> pair in _frameworkNameAlias)
            {
                if (FrameworkNameEqualityComparer.Default.Equals(pair.Value, frameworkName))
                {
                    frameworkName = pair.Key;
                    break;
                }
            }
            if (!_identifierToFrameworkFolder.TryGetValue(frameworkName.get_Identifier(), out str))
            {
                str = frameworkName.get_Identifier();
            }
            if (str.Equals("portable", StringComparison.OrdinalIgnoreCase))
            {
                if (portableProfileTable == null)
                {
                    throw new ArgumentException(NuGetResources.PortableProfileTableMustBeSpecified, "portableProfileTable");
                }
                NetPortableProfile profile = NetPortableProfile.Parse(frameworkName.get_Profile(), false, portableProfileTable);
                str2 = (profile == null) ? frameworkName.get_Profile() : profile.CustomProfileString;
            }
            else
            {
                if (frameworkName.get_Version() > new Version())
                {
                    str = str + frameworkName.get_Version().ToString().Replace(".", string.Empty);
                }
                if (string.IsNullOrEmpty(frameworkName.get_Profile()))
                {
                    return str;
                }
                if (!_identifierToProfileFolder.TryGetValue(frameworkName.get_Profile(), out str2))
                {
                    str2 = frameworkName.get_Profile();
                }
            }
            return (str + "-" + str2);
        }

        public static string GetTargetFrameworkLogString(FrameworkName targetFramework) => 
            (((targetFramework == null) || (targetFramework == EmptyFramework)) ? NuGetResources.Debug_TargetFrameworkInfo_NotFrameworkSpecific : string.Empty);

        public static bool IsCompatible(FrameworkName projectFrameworkName, IEnumerable<FrameworkName> packageSupportedFrameworks) => 
            IsCompatible(projectFrameworkName, packageSupportedFrameworks, NetPortableProfileTable.Default);

        internal static bool IsCompatible(FrameworkName projectFrameworkName, FrameworkName packageTargetFrameworkName) => 
            IsCompatible(projectFrameworkName, packageTargetFrameworkName, NetPortableProfileTable.Default);

        public static bool IsCompatible(FrameworkName projectFrameworkName, IEnumerable<FrameworkName> packageSupportedFrameworks, NetPortableProfileTable portableProfileTable) => 
            (!packageSupportedFrameworks.Any<FrameworkName>() || Enumerable.Any<FrameworkName>(packageSupportedFrameworks, (Func<FrameworkName, bool>) (packageSupportedFramework => IsCompatible(projectFrameworkName, packageSupportedFramework, portableProfileTable))));

        internal static bool IsCompatible(FrameworkName projectFrameworkName, FrameworkName packageTargetFrameworkName, NetPortableProfileTable portableProfileTable)
        {
            Dictionary<string, string[]> dictionary;
            string[] strArray;
            if (projectFrameworkName == null)
            {
                return true;
            }
            if (packageTargetFrameworkName.IsPortableFramework())
            {
                return IsPortableLibraryCompatible(projectFrameworkName, packageTargetFrameworkName, portableProfileTable);
            }
            packageTargetFrameworkName = NormalizeFrameworkName(packageTargetFrameworkName);
            projectFrameworkName = NormalizeFrameworkName(projectFrameworkName);
            if (!projectFrameworkName.get_Identifier().Equals(packageTargetFrameworkName.get_Identifier(), StringComparison.OrdinalIgnoreCase))
            {
                FrameworkName name;
                if (!_equivalentProjectFrameworks.TryGetValue(projectFrameworkName.get_Identifier(), out name) || !name.get_Identifier().Equals(packageTargetFrameworkName.get_Identifier(), StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                projectFrameworkName = name;
            }
            return ((NormalizeVersion(projectFrameworkName.get_Version()) >= NormalizeVersion(packageTargetFrameworkName.get_Version())) ? (!string.Equals(projectFrameworkName.get_Profile(), packageTargetFrameworkName.get_Profile(), StringComparison.OrdinalIgnoreCase) ? (_compatibiltyMapping.TryGetValue(projectFrameworkName.get_Identifier(), out dictionary) && (dictionary.TryGetValue(packageTargetFrameworkName.get_Profile(), out strArray) && strArray.Contains<string>(projectFrameworkName.get_Profile(), StringComparer.OrdinalIgnoreCase))) : true) : false);
        }

        public static bool IsPortableFramework(this FrameworkName framework) => 
            ((framework != null) && ".NETPortable".Equals(framework.get_Identifier(), StringComparison.OrdinalIgnoreCase));

        private static bool IsPortableLibraryCompatible(FrameworkName projectFrameworkName, FrameworkName packageTargetFrameworkName, NetPortableProfileTable portableProfileTable)
        {
            if (string.IsNullOrEmpty(packageTargetFrameworkName.get_Profile()))
            {
                return false;
            }
            NetPortableProfile profile = NetPortableProfile.Parse(packageTargetFrameworkName.get_Profile(), false, portableProfileTable);
            if (profile == null)
            {
                return false;
            }
            if (!projectFrameworkName.IsPortableFramework())
            {
                return profile.IsCompatibleWith(projectFrameworkName);
            }
            if (string.Equals(projectFrameworkName.get_Profile(), packageTargetFrameworkName.get_Profile(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            NetPortableProfile projectFrameworkProfile = NetPortableProfile.Parse(projectFrameworkName.get_Profile(), false, portableProfileTable);
            return ((projectFrameworkProfile != null) ? profile.IsCompatibleWith(projectFrameworkProfile, portableProfileTable) : false);
        }

        public static FrameworkName NormalizeFrameworkName(FrameworkName framework)
        {
            FrameworkName name;
            return (!_frameworkNameAlias.TryGetValue(framework, out name) ? framework : name);
        }

        internal static Version NormalizeVersion(Version version) => 
            new Version(version.Major, version.Minor, Math.Max(version.Build, 0), Math.Max(version.Revision, 0));

        public static FrameworkName ParseFrameworkFolderName(string path)
        {
            string str;
            return ParseFrameworkFolderName(path, true, out str);
        }

        public static FrameworkName ParseFrameworkFolderName(string path, bool strictParsing, out string effectivePath)
        {
            char[] separator = new char[] { Path.DirectorySeparatorChar };
            string str = Path.GetDirectoryName(path).Split(separator).First<string>();
            effectivePath = path;
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            FrameworkName name = ParseFrameworkName(str);
            if (!strictParsing && (name == UnsupportedFrameworkName))
            {
                return null;
            }
            effectivePath = path.Substring(str.Length + 1);
            return name;
        }

        public static FrameworkName ParseFrameworkName(string frameworkName)
        {
            int num;
            string str5;
            if (frameworkName == null)
            {
                throw new ArgumentNullException("frameworkName");
            }
            string str = null;
            string s = null;
            char[] separator = new char[] { '-' };
            string[] strArray = frameworkName.Split(separator);
            if (strArray.Length > 2)
            {
                throw new ArgumentException(NuGetResources.InvalidFrameworkNameFormat, "frameworkName");
            }
            string str3 = (strArray.Length != 0) ? strArray[0].Trim() : null;
            string str4 = (strArray.Length > 1) ? strArray[1].Trim() : null;
            if (string.IsNullOrEmpty(str3))
            {
                throw new ArgumentException(NuGetResources.MissingFrameworkName, "frameworkName");
            }
            Match match = Regex.Match(str3, @"\d+");
            if (!match.Success)
            {
                str = str3.Trim();
            }
            else
            {
                str = str3.Substring(0, match.Index).Trim();
                s = str3.Substring(match.Index).Trim();
            }
            if (!string.IsNullOrEmpty(str) && !_knownIdentifiers.TryGetValue(str, out str))
            {
                return UnsupportedFrameworkName;
            }
            if (!string.IsNullOrEmpty(str4) && _knownProfiles.TryGetValue(str4, out str5))
            {
                str4 = str5;
            }
            Version result = null;
            if (int.TryParse(s, out num))
            {
                if (s.Length > 4)
                {
                    s = s.Substring(0, 4);
                }
                s = s.PadRight(2, '0');
                s = string.Join<char>(".", s.ToCharArray());
            }
            if (!Version.TryParse(s, out result))
            {
                if (string.IsNullOrEmpty(str) || !string.IsNullOrEmpty(s))
                {
                    return UnsupportedFrameworkName;
                }
                result = _emptyVersion;
            }
            if (string.IsNullOrEmpty(str))
            {
                str = ".NETFramework";
            }
            if (str.Equals(".NETPortable", StringComparison.OrdinalIgnoreCase))
            {
                ValidatePortableFrameworkProfilePart(str4);
            }
            return new FrameworkName(str, result, str4);
        }

        public static FrameworkName ParseFrameworkNameFromFilePath(string filePath, out string effectivePath)
        {
            string[] strArray = new string[] { Constants.ContentDirectory, Constants.LibDirectory, Constants.ToolsDirectory, Constants.BuildDirectory };
            for (int i = 0; i < strArray.Length; i++)
            {
                string str = strArray[i] + Path.DirectorySeparatorChar.ToString();
                if ((filePath.Length > str.Length) && filePath.StartsWith(str, StringComparison.OrdinalIgnoreCase))
                {
                    string path = filePath.Substring(str.Length);
                    try
                    {
                        return ParseFrameworkFolderName(path, strArray[i] == Constants.LibDirectory, out effectivePath);
                    }
                    catch (ArgumentException)
                    {
                        effectivePath = path;
                        return null;
                    }
                }
            }
            effectivePath = filePath;
            return null;
        }

        public static IVersionSpec ParseVersionSpec(string value)
        {
            IVersionSpec spec;
            if (TryParseVersionSpec(value, out spec))
            {
                return spec;
            }
            object[] args = new object[] { value };
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, NuGetResources.InvalidVersionString, args));
        }

        public static string PrettyPrint(IVersionSpec versionSpec)
        {
            if ((versionSpec.MinVersion != null) && (versionSpec.IsMinInclusive && ((versionSpec.MaxVersion == null) && !versionSpec.IsMaxInclusive)))
            {
                object[] args = new object[] { "≥", versionSpec.MinVersion };
                return string.Format(CultureInfo.InvariantCulture, "({0} {1})", args);
            }
            if ((versionSpec.MinVersion != null) && ((versionSpec.MaxVersion != null) && ((versionSpec.MinVersion == versionSpec.MaxVersion) && (versionSpec.IsMinInclusive && versionSpec.IsMaxInclusive))))
            {
                object[] args = new object[] { versionSpec.MinVersion };
                return string.Format(CultureInfo.InvariantCulture, "(= {0})", args);
            }
            StringBuilder builder = new StringBuilder();
            if (versionSpec.MinVersion != null)
            {
                if (!versionSpec.IsMinInclusive)
                {
                    builder.Append("(> ");
                }
                else
                {
                    object[] args = new object[] { "≥" };
                    builder.AppendFormat(CultureInfo.InvariantCulture, "({0} ", args);
                }
                builder.Append(versionSpec.MinVersion);
            }
            if (versionSpec.MaxVersion != null)
            {
                if (builder.Length == 0)
                {
                    builder.Append("(");
                }
                else
                {
                    builder.Append(" && ");
                }
                if (!versionSpec.IsMaxInclusive)
                {
                    builder.Append("< ");
                }
                else
                {
                    object[] args = new object[] { "≤" };
                    builder.AppendFormat(CultureInfo.InvariantCulture, "{0} ", args);
                }
                builder.Append(versionSpec.MaxVersion);
            }
            if (builder.Length > 0)
            {
                builder.Append(")");
            }
            return builder.ToString();
        }

        public static Version TrimVersion(Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            if ((version.Build == 0) && (version.Revision == 0))
            {
                version = new Version(version.Major, version.Minor);
            }
            else if (version.Revision == 0)
            {
                version = new Version(version.Major, version.Minor, version.Build);
            }
            return version;
        }

        public static bool TryGetCompatibleItems<T>(FrameworkName projectFramework, IEnumerable<T> items, out IEnumerable<T> compatibleItems) where T: IFrameworkTargetable => 
            TryGetCompatibleItems<T>(projectFramework, items, NetPortableProfileTable.Default, out compatibleItems);

        public static bool TryGetCompatibleItems<T>(FrameworkName projectFramework, IEnumerable<T> items, NetPortableProfileTable portableProfileTable, out IEnumerable<T> compatibleItems) where T: IFrameworkTargetable
        {
            if (!items.Any<T>())
            {
                compatibleItems = Enumerable.Empty<T>();
                return true;
            }
            FrameworkName internalProjectFramework = projectFramework ?? EmptyFramework;
            List<IGrouping<FrameworkName, T>> list = (from <>h__TransparentIdentifier0 in from item in items select new { 
                item = item,
                frameworks = ((item.SupportedFrameworks == null) || !item.SupportedFrameworks.Any<FrameworkName>()) ? new FrameworkName[1] : item.SupportedFrameworks
            }
                from framework in <>h__TransparentIdentifier0.frameworks
                select new { 
                    Item = <>h__TransparentIdentifier0.item,
                    TargetFramework = framework
                } into g
                group g.Item by g.TargetFramework).ToList<IGrouping<FrameworkName, T>>();
            compatibleItems = (from g in list
                where (g.Key != null) && IsCompatible(internalProjectFramework, g.Key, portableProfileTable)
                orderby GetProfileCompatibility(internalProjectFramework, g.Key, portableProfileTable) descending
                select g).FirstOrDefault<IGrouping<FrameworkName, T>>();
            bool flag = (compatibleItems != null) && compatibleItems.Any<T>();
            if (!flag)
            {
                compatibleItems = from g in list
                    where g.Key == null
                    select g;
                flag = (compatibleItems != null) && compatibleItems.Any<T>();
            }
            if (!flag)
            {
                compatibleItems = null;
            }
            return flag;
        }

        private static bool TryParseVersion(string versionString, out SemanticVersion version)
        {
            int num;
            version = null;
            if (!SemanticVersion.TryParse(versionString, out version) && (int.TryParse(versionString, out num) && (num > 0)))
            {
                version = new SemanticVersion(new Version(num, 0));
            }
            return (version != null);
        }

        public static bool TryParseVersionSpec(string value, out IVersionSpec result)
        {
            SemanticVersion version;
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            VersionSpec spec = new VersionSpec();
            value = value.Trim();
            if (SemanticVersion.TryParse(value, out version))
            {
                VersionSpec spec1 = new VersionSpec();
                spec1.MinVersion = version;
                spec1.IsMinInclusive = true;
                result = spec1;
                return true;
            }
            result = null;
            if (value.Length < 3)
            {
                return false;
            }
            char ch = value.First<char>();
            if (ch == '(')
            {
                spec.IsMinInclusive = false;
            }
            else
            {
                if (ch != '[')
                {
                    return false;
                }
                spec.IsMinInclusive = true;
            }
            ch = value.Last<char>();
            if (ch == ')')
            {
                spec.IsMaxInclusive = false;
            }
            else
            {
                if (ch != ']')
                {
                    return false;
                }
                spec.IsMaxInclusive = true;
            }
            value = value.Substring(1, value.Length - 2);
            char[] separator = new char[] { ',' };
            string[] strArray = value.Split(separator);
            if (strArray.Length > 2)
            {
                return false;
            }
            if (Enumerable.All<string>(strArray, new Func<string, bool>(string.IsNullOrEmpty)))
            {
                return false;
            }
            string str = strArray[0];
            string str2 = (strArray.Length == 2) ? strArray[1] : strArray[0];
            if (!string.IsNullOrWhiteSpace(str))
            {
                if (!TryParseVersion(str, out version))
                {
                    return false;
                }
                spec.MinVersion = version;
            }
            if (!string.IsNullOrWhiteSpace(str2))
            {
                if (!TryParseVersion(str2, out version))
                {
                    return false;
                }
                spec.MaxVersion = version;
            }
            result = spec;
            return true;
        }

        internal static void ValidatePortableFrameworkProfilePart(string profilePart)
        {
            if (string.IsNullOrEmpty(profilePart))
            {
                throw new ArgumentException(NuGetResources.PortableFrameworkProfileEmpty, "profilePart");
            }
            if (profilePart.Contains<char>('-'))
            {
                throw new ArgumentException(NuGetResources.PortableFrameworkProfileHasDash, "profilePart");
            }
            if (profilePart.Contains<char>(' '))
            {
                throw new ArgumentException(NuGetResources.PortableFrameworkProfileHasSpace, "profilePart");
            }
            char[] separator = new char[] { '+' };
            string[] strArray = profilePart.Split(separator);
            if (Enumerable.Any<string>(strArray, p => string.IsNullOrEmpty(p)))
            {
                throw new ArgumentException(NuGetResources.PortableFrameworkProfileComponentIsEmpty, "profilePart");
            }
            if (Enumerable.Any<string>(strArray, p => p.StartsWith("portable", StringComparison.OrdinalIgnoreCase)) || (Enumerable.Any<string>(strArray, p => p.StartsWith("NETPortable", StringComparison.OrdinalIgnoreCase)) || Enumerable.Any<string>(strArray, p => p.StartsWith(".NETPortable", StringComparison.OrdinalIgnoreCase))))
            {
                throw new ArgumentException(NuGetResources.PortableFrameworkProfileComponentIsPortable, "profilePart");
            }
        }

        public static Version DefaultTargetFrameworkVersion =>
            typeof(string).Assembly.GetName().Version;

        public static FrameworkName DefaultTargetFramework =>
            new FrameworkName(".NETFramework", DefaultTargetFrameworkVersion);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly VersionUtility.<>c <>9 = new VersionUtility.<>c();
            public static Func<string, bool> <>9__24_0;
            public static Func<string, bool> <>9__24_1;
            public static Func<string, bool> <>9__24_2;
            public static Func<string, bool> <>9__24_3;

            internal bool <ValidatePortableFrameworkProfilePart>b__24_0(string p) => 
                string.IsNullOrEmpty(p);

            internal bool <ValidatePortableFrameworkProfilePart>b__24_1(string p) => 
                p.StartsWith("portable", StringComparison.OrdinalIgnoreCase);

            internal bool <ValidatePortableFrameworkProfilePart>b__24_2(string p) => 
                p.StartsWith("NETPortable", StringComparison.OrdinalIgnoreCase);

            internal bool <ValidatePortableFrameworkProfilePart>b__24_3(string p) => 
                p.StartsWith(".NETPortable", StringComparison.OrdinalIgnoreCase);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c__38<T> where T: IFrameworkTargetable
        {
            public static readonly VersionUtility.<>c__38<T> <>9;
            public static Func<T, <>f__AnonymousType31<T, IEnumerable<FrameworkName>>> <>9__38_0;
            public static Func<<>f__AnonymousType31<T, IEnumerable<FrameworkName>>, IEnumerable<FrameworkName>> <>9__38_1;
            public static Func<<>f__AnonymousType31<T, IEnumerable<FrameworkName>>, FrameworkName, <>f__AnonymousType32<T, FrameworkName>> <>9__38_2;
            public static Func<<>f__AnonymousType32<T, FrameworkName>, FrameworkName> <>9__38_3;
            public static Func<<>f__AnonymousType32<T, FrameworkName>, T> <>9__38_4;
            public static Func<IGrouping<FrameworkName, T>, bool> <>9__38_7;
            public static Func<IGrouping<FrameworkName, T>, IEnumerable<T>> <>9__38_8;

            static <>c__38()
            {
                VersionUtility.<>c__38<T>.<>9 = new VersionUtility.<>c__38<T>();
            }

            internal <>f__AnonymousType31<T, IEnumerable<FrameworkName>> <TryGetCompatibleItems>b__38_0(T item) => 
                new { 
                    item = item,
                    frameworks = ((item.SupportedFrameworks == null) || !item.SupportedFrameworks.Any<FrameworkName>()) ? new FrameworkName[1] : item.SupportedFrameworks
                };

            internal IEnumerable<FrameworkName> <TryGetCompatibleItems>b__38_1(<>f__AnonymousType31<T, IEnumerable<FrameworkName>> <>h__TransparentIdentifier0) => 
                <>h__TransparentIdentifier0.frameworks;

            internal <>f__AnonymousType32<T, FrameworkName> <TryGetCompatibleItems>b__38_2(<>f__AnonymousType31<T, IEnumerable<FrameworkName>> <>h__TransparentIdentifier0, FrameworkName framework) => 
                new { 
                    Item = <>h__TransparentIdentifier0.item,
                    TargetFramework = framework
                };

            internal FrameworkName <TryGetCompatibleItems>b__38_3(<>f__AnonymousType32<T, FrameworkName> g) => 
                g.TargetFramework;

            internal T <TryGetCompatibleItems>b__38_4(<>f__AnonymousType32<T, FrameworkName> g) => 
                g.Item;

            internal bool <TryGetCompatibleItems>b__38_7(IGrouping<FrameworkName, T> g) => 
                (g.Key == null);

            internal IEnumerable<T> <TryGetCompatibleItems>b__38_8(IGrouping<FrameworkName, T> g) => 
                g;
        }

    }
}

