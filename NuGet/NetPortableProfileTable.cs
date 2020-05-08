namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Xml;
    using System.Xml.Linq;

    internal class NetPortableProfileTable
    {
        private const string PortableReferenceAssemblyPathEnvironmentVariableName = "NuGetPortableReferenceAssemblyPath";
        private static readonly Lazy<NetPortableProfileTable> _defaultTable = new Lazy<NetPortableProfileTable>(new Func<NetPortableProfileTable>(NetPortableProfileTable.LoadDefaultTable));
        private IDictionary<string, NetPortableProfile> _portableProfilesByCustomProfileString;
        private IDictionary<string, List<Tuple<Version, ISet<string>>>> _portableProfilesSetByOptionalFrameworks;

        public NetPortableProfileTable(IEnumerable<NetPortableProfile> profiles)
        {
            this.Profiles = new NetPortableProfileCollection();
            this.Profiles.AddRange<NetPortableProfile>(profiles);
            this._portableProfilesByCustomProfileString = Enumerable.ToDictionary<NetPortableProfile, string>(this.Profiles, p => p.CustomProfileString);
            this.CreateOptionalFrameworksDictionary();
        }

        private void CreateOptionalFrameworksDictionary()
        {
            this._portableProfilesSetByOptionalFrameworks = new Dictionary<string, List<Tuple<Version, ISet<string>>>>();
            foreach (NetPortableProfile profile in this.Profiles)
            {
                foreach (FrameworkName optionalFramework in profile.OptionalFrameworks)
                {
                    if (!this._portableProfilesSetByOptionalFrameworks.ContainsKey(optionalFramework.get_Identifier()))
                    {
                        this._portableProfilesSetByOptionalFrameworks.Add(optionalFramework.get_Identifier(), new List<Tuple<Version, ISet<string>>>());
                    }
                    List<Tuple<Version, ISet<string>>> list = this._portableProfilesSetByOptionalFrameworks[optionalFramework.get_Identifier()];
                    if (list != null)
                    {
                        Tuple<Version, ISet<string>> item = (from tuple in list
                            where tuple.Item1.Equals(optionalFramework.get_Version())
                            select tuple).FirstOrDefault<Tuple<Version, ISet<string>>>();
                        if (item == null)
                        {
                            item = new Tuple<Version, ISet<string>>(optionalFramework.get_Version(), (ISet<string>) new HashSet<string>());
                            list.Add(item);
                        }
                        item.Item2.Add(profile.Name);
                    }
                }
            }
        }

        public static NetPortableProfileTable Deserialize(Stream input) => 
            NetPortableProfileTableSerializer.Deserialize(input);

        public NetPortableProfile GetProfile(string profileName)
        {
            if (string.IsNullOrEmpty(profileName))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "profileName");
            }
            if (this.Profiles.Contains(profileName))
            {
                return this.Profiles[profileName];
            }
            NetPortableProfile profile = null;
            this._portableProfilesByCustomProfileString.TryGetValue(profileName, out profile);
            return profile;
        }

        internal bool HasCompatibleProfileWith(NetPortableProfile packageFramework, FrameworkName projectOptionalFrameworkName, NetPortableProfileTable portableProfileTable)
        {
            List<Tuple<Version, ISet<string>>> list = null;
            if ((this._portableProfilesSetByOptionalFrameworks != null) && this._portableProfilesSetByOptionalFrameworks.TryGetValue(projectOptionalFrameworkName.get_Identifier(), out list))
            {
                using (List<Tuple<Version, ISet<string>>>.Enumerator enumerator = list.GetEnumerator())
                {
                    while (true)
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        Tuple<Version, ISet<string>> current = enumerator.Current;
                        if (projectOptionalFrameworkName.get_Version() >= current.Item1)
                        {
                            using (IEnumerator<string> enumerator2 = current.Item2.GetEnumerator())
                            {
                                while (true)
                                {
                                    if (!enumerator2.MoveNext())
                                    {
                                        break;
                                    }
                                    string profileName = enumerator2.Current;
                                    NetPortableProfile projectFrameworkProfile = this.GetProfile(profileName);
                                    if ((projectFrameworkProfile != null) && packageFramework.IsCompatibleWith(projectFrameworkProfile, portableProfileTable))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static bool IsOptionalFramework(FrameworkName framework) => 
            (framework.get_Identifier().StartsWith("Mono", StringComparison.OrdinalIgnoreCase) || framework.get_Identifier().StartsWith("Xamarin", StringComparison.OrdinalIgnoreCase));

        private static NetPortableProfileTable LoadDefaultTable()
        {
            string environmentVariable = Environment.GetEnvironmentVariable("NuGetPortableReferenceAssemblyPath");
            string portableRootDirectory = string.IsNullOrEmpty(environmentVariable) ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86, Environment.SpecialFolderOption.DoNotVerify), @"Reference Assemblies\Microsoft\Framework\.NETPortable") : environmentVariable;
            return LoadFromProfileDirectory(portableRootDirectory);
        }

        public static NetPortableProfileTable LoadFromProfileDirectory(string portableRootDirectory) => 
            new NetPortableProfileTable(LoadPortableProfiles(portableRootDirectory));

        private static NetPortableProfile LoadPortableProfile(string version, string profileDirectory)
        {
            string fileName = Path.GetFileName(profileDirectory);
            string path = Path.Combine(profileDirectory, "SupportedFrameworks");
            return (Directory.Exists(path) ? LoadPortableProfile(version, fileName, new PhysicalFileSystem(path), Directory.EnumerateFiles(path, "*.xml")) : null);
        }

        internal static NetPortableProfile LoadPortableProfile(string version, string profileName, IFileSystem fileSystem, IEnumerable<string> frameworkFiles)
        {
            IEnumerable<FrameworkName> enumerable = from p in frameworkFiles
                select LoadSupportedFramework(fileSystem, p) into p
                where p != null
                select p;
            List<FrameworkName> optionalFrameworks = (from p in enumerable
                where IsOptionalFramework(p)
                select p).ToList<FrameworkName>();
            return new NetPortableProfile(version, profileName, optionalFrameworks.IsEmpty<FrameworkName>() ? enumerable : (from p in enumerable
                where !optionalFrameworks.Contains(p)
                select p), optionalFrameworks);
        }

        private static IEnumerable<NetPortableProfile> LoadPortableProfiles(string portableRootDirectory) => 
            (!Directory.Exists(portableRootDirectory) ? Enumerable.Empty<NetPortableProfile>() : (from versionDir in Directory.EnumerateDirectories(portableRootDirectory, "v*", SearchOption.TopDirectoryOnly) select LoadProfilesFromFramework(versionDir, versionDir + @"\Profile\")));

        private static IEnumerable<NetPortableProfile> LoadProfilesFromFramework(string version, string profileFilesPath)
        {
            if (Directory.Exists(profileFilesPath))
            {
                try
                {
                    return (from profileDir in Directory.EnumerateDirectories(profileFilesPath, "Profile*")
                        select LoadPortableProfile(version, profileDir) into p
                        where p != null
                        select p);
                }
                catch (IOException)
                {
                }
                catch (SecurityException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
            return Enumerable.Empty<NetPortableProfile>();
        }

        internal static FrameworkName LoadSupportedFramework(Stream stream)
        {
            try
            {
                XElement root = XmlUtility.LoadSafe(stream).Root;
                if (root.Name.LocalName.Equals("Framework", StringComparison.Ordinal))
                {
                    FrameworkName name;
                    string str = root.GetOptionalAttributeValue("Identifier", null);
                    if (str == null)
                    {
                        name = null;
                    }
                    else
                    {
                        string input = root.GetOptionalAttributeValue("MinimumVersion", null);
                        if (input == null)
                        {
                            name = null;
                        }
                        else
                        {
                            Version version;
                            if (!Version.TryParse(input, out version))
                            {
                                name = null;
                            }
                            else
                            {
                                string str3 = root.GetOptionalAttributeValue("Profile", null);
                                if (str3 == null)
                                {
                                    str3 = "";
                                }
                                if (str3.EndsWith("*", StringComparison.Ordinal))
                                {
                                    str3 = str3.Substring(0, str3.Length - 1);
                                    if (str3.Equals("WindowsPhone7", StringComparison.OrdinalIgnoreCase))
                                    {
                                        str3 = "WindowsPhone71";
                                    }
                                    else if (str.Equals("Silverlight", StringComparison.OrdinalIgnoreCase) && (str3.Equals("WindowsPhone", StringComparison.OrdinalIgnoreCase) && (version == new Version(4, 0))))
                                    {
                                        version = new Version(3, 0);
                                    }
                                }
                                name = new FrameworkName(str, version, str3);
                            }
                        }
                    }
                    return name;
                }
            }
            catch (XmlException)
            {
            }
            catch (IOException)
            {
            }
            catch (SecurityException)
            {
            }
            return null;
        }

        private static FrameworkName LoadSupportedFramework(IFileSystem fileSystem, string frameworkFile)
        {
            using (Stream stream = fileSystem.OpenFile(frameworkFile))
            {
                return LoadSupportedFramework(stream);
            }
        }

        public void Serialize(Stream output)
        {
            NetPortableProfileTableSerializer.Serialize(this, output);
        }

        public static NetPortableProfileTable Default =>
            _defaultTable.Value;

        public NetPortableProfileCollection Profiles { get; private set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly NetPortableProfileTable.<>c <>9 = new NetPortableProfileTable.<>c();
            public static Func<NetPortableProfile, string> <>9__6_0;
            public static Func<string, IEnumerable<NetPortableProfile>> <>9__18_0;
            public static Func<NetPortableProfile, bool> <>9__19_1;
            public static Func<FrameworkName, bool> <>9__21_1;
            public static Func<FrameworkName, bool> <>9__21_2;

            internal string <.ctor>b__6_0(NetPortableProfile p) => 
                p.CustomProfileString;

            internal bool <LoadPortableProfile>b__21_1(FrameworkName p) => 
                (p != null);

            internal bool <LoadPortableProfile>b__21_2(FrameworkName p) => 
                NetPortableProfileTable.IsOptionalFramework(p);

            internal IEnumerable<NetPortableProfile> <LoadPortableProfiles>b__18_0(string versionDir) => 
                NetPortableProfileTable.LoadProfilesFromFramework(versionDir, versionDir + @"\Profile\");

            internal bool <LoadProfilesFromFramework>b__19_1(NetPortableProfile p) => 
                (p != null);
        }
    }
}

