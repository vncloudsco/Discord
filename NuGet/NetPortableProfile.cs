namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;

    internal class NetPortableProfile : IEquatable<NetPortableProfile>
    {
        private string _customProfile;

        public NetPortableProfile(string name, IEnumerable<FrameworkName> supportedFrameworks, IEnumerable<FrameworkName> optionalFrameworks = null) : this("v0.0", name, supportedFrameworks, optionalFrameworks)
        {
        }

        public NetPortableProfile(string version, string name, IEnumerable<FrameworkName> supportedFrameworks, IEnumerable<FrameworkName> optionalFrameworks)
        {
            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "version");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "name");
            }
            if (supportedFrameworks == null)
            {
                throw new ArgumentNullException("supportedFrameworks");
            }
            List<FrameworkName> items = supportedFrameworks.ToList<FrameworkName>();
            if (Enumerable.Any<FrameworkName>(items, (Func<FrameworkName, bool>) (f => (f == null))))
            {
                throw new ArgumentException(NuGetResources.SupportedFrameworkIsNull, "supportedFrameworks");
            }
            if (items.Count == 0)
            {
                throw new ArgumentOutOfRangeException("supportedFrameworks");
            }
            this.Name = name;
            this.SupportedFrameworks = new ReadOnlyHashSet<FrameworkName>(items);
            this.OptionalFrameworks = ((optionalFrameworks == null) || optionalFrameworks.IsEmpty<FrameworkName>()) ? new ReadOnlyHashSet<FrameworkName>(Enumerable.Empty<FrameworkName>()) : new ReadOnlyHashSet<FrameworkName>(optionalFrameworks);
            this.FrameworkVersion = version;
        }

        public bool Equals(NetPortableProfile other) => 
            (this.Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase) && (this.SupportedFrameworks.SetEquals(other.SupportedFrameworks) && this.OptionalFrameworks.SetEquals(other.OptionalFrameworks)));

        public override int GetHashCode()
        {
            HashCodeCombiner combiner1 = new HashCodeCombiner();
            combiner1.AddObject(this.Name);
            combiner1.AddObject(this.SupportedFrameworks);
            combiner1.AddObject(this.OptionalFrameworks);
            return combiner1.CombinedHash;
        }

        public bool IsCompatibleWith(NetPortableProfile projectFrameworkProfile) => 
            this.IsCompatibleWith(projectFrameworkProfile, NetPortableProfileTable.Default);

        public bool IsCompatibleWith(FrameworkName projectFramework) => 
            this.IsCompatibleWith(projectFramework, NetPortableProfileTable.Default);

        public bool IsCompatibleWith(NetPortableProfile projectFrameworkProfile, NetPortableProfileTable portableProfileTable)
        {
            if (projectFrameworkProfile == null)
            {
                throw new ArgumentNullException("projectFrameworkProfile");
            }
            return Enumerable.All<FrameworkName>(projectFrameworkProfile.SupportedFrameworks, (Func<FrameworkName, bool>) (projectFramework => Enumerable.Any<FrameworkName>(this.SupportedFrameworks, (Func<FrameworkName, bool>) (packageFramework => VersionUtility.IsCompatible(projectFramework, packageFramework, portableProfileTable)))));
        }

        public bool IsCompatibleWith(FrameworkName projectFramework, NetPortableProfileTable portableProfileTable)
        {
            if (projectFramework == null)
            {
                throw new ArgumentNullException("projectFramework");
            }
            return (Enumerable.Any<FrameworkName>(this.SupportedFrameworks, (Func<FrameworkName, bool>) (packageFramework => VersionUtility.IsCompatible(projectFramework, packageFramework, portableProfileTable))) || portableProfileTable.HasCompatibleProfileWith(this, projectFramework, portableProfileTable));
        }

        public static NetPortableProfile Parse(string profileValue, bool treatOptionalFrameworksAsSupportedFrameworks = false, NetPortableProfileTable portableProfileTable = null)
        {
            portableProfileTable = portableProfileTable ?? NetPortableProfileTable.Default;
            if (string.IsNullOrEmpty(profileValue))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "profileValue");
            }
            NetPortableProfile profile = portableProfileTable.GetProfile(profileValue);
            if (profile != null)
            {
                if (treatOptionalFrameworksAsSupportedFrameworks)
                {
                    profile = new NetPortableProfile(profile.Name, profile.SupportedFrameworks.Concat<FrameworkName>(profile.OptionalFrameworks), null);
                }
                return profile;
            }
            if (profileValue.StartsWith("Profile", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            VersionUtility.ValidatePortableFrameworkProfilePart(profileValue);
            char[] separator = new char[] { '+' };
            return new NetPortableProfile(profileValue, Enumerable.Select<string, FrameworkName>(profileValue.Split(separator, StringSplitOptions.RemoveEmptyEntries), new Func<string, FrameworkName>(VersionUtility.ParseFrameworkName)), null);
        }

        public string Name { get; private set; }

        public string FrameworkVersion { get; private set; }

        public ISet<FrameworkName> SupportedFrameworks { get; private set; }

        public ISet<FrameworkName> OptionalFrameworks { get; private set; }

        public string CustomProfileString
        {
            get
            {
                if (this._customProfile == null)
                {
                    IEnumerable<FrameworkName> enumerable = this.SupportedFrameworks.Concat<FrameworkName>(this.OptionalFrameworks);
                    this._customProfile = string.Join("+", (IEnumerable<string>) (from f in enumerable select VersionUtility.GetShortFrameworkName(f, null)));
                }
                return this._customProfile;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly NetPortableProfile.<>c <>9 = new NetPortableProfile.<>c();
            public static Func<FrameworkName, bool> <>9__2_0;
            public static Func<FrameworkName, string> <>9__22_0;

            internal bool <.ctor>b__2_0(FrameworkName f) => 
                (f == null);

            internal string <get_CustomProfileString>b__22_0(FrameworkName f) => 
                VersionUtility.GetShortFrameworkName(f, null);
        }
    }
}

