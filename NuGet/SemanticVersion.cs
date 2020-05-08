namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    [Serializable, TypeConverter(typeof(SemanticVersionTypeConverter))]
    internal sealed class SemanticVersion : IComparable, IComparable<SemanticVersion>, IEquatable<SemanticVersion>
    {
        private const RegexOptions _flags = (RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
        private static readonly Regex _semanticVersionRegex = new Regex(@"^(?<Version>\d+(\s*\.\s*\d+){0,3})(?<Release>-[a-z][0-9a-z-]*)?$", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
        private static readonly Regex _strictSemanticVersionRegex = new Regex(@"^(?<Version>\d+(\.\d+){2})(?<Release>-[a-z][0-9a-z-]*)?$", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
        private readonly string _originalString;

        internal SemanticVersion(SemanticVersion semVer)
        {
            this._originalString = semVer.ToString();
            this.Version = semVer.Version;
            this.SpecialVersion = semVer.SpecialVersion;
        }

        public SemanticVersion(string version) : this(Parse(version))
        {
            this._originalString = version;
        }

        public SemanticVersion(System.Version version) : this(version, string.Empty)
        {
        }

        public SemanticVersion(System.Version version, string specialVersion) : this(version, specialVersion, null)
        {
        }

        private SemanticVersion(System.Version version, string specialVersion, string originalString)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            this.Version = NormalizeVersionValue(version);
            this.SpecialVersion = specialVersion ?? string.Empty;
            this._originalString = string.IsNullOrEmpty(originalString) ? (version.ToString() + (!string.IsNullOrEmpty(specialVersion) ? ("-" + specialVersion) : null)) : originalString;
        }

        public SemanticVersion(int major, int minor, int build, int revision) : this(new System.Version(major, minor, build, revision))
        {
        }

        public SemanticVersion(int major, int minor, int build, string specialVersion) : this(new System.Version(major, minor, build), specialVersion)
        {
        }

        public int CompareTo(SemanticVersion other)
        {
            if (other == null)
            {
                return 1;
            }
            int num = this.Version.CompareTo(other.Version);
            if (num != 0)
            {
                return num;
            }
            bool flag = string.IsNullOrEmpty(this.SpecialVersion);
            bool flag2 = string.IsNullOrEmpty(other.SpecialVersion);
            return (!(flag & flag2) ? (!flag ? (!flag2 ? StringComparer.OrdinalIgnoreCase.Compare(this.SpecialVersion, other.SpecialVersion) : -1) : 1) : 0);
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            SemanticVersion other = obj as SemanticVersion;
            if (other == null)
            {
                throw new ArgumentException(NuGetResources.TypeMustBeASemanticVersion, "obj");
            }
            return this.CompareTo(other);
        }

        public bool Equals(SemanticVersion other) => 
            ((other != null) && (this.Version.Equals(other.Version) && this.SpecialVersion.Equals(other.SpecialVersion, StringComparison.OrdinalIgnoreCase)));

        public override bool Equals(object obj)
        {
            SemanticVersion other = obj as SemanticVersion;
            return ((other != null) && this.Equals(other));
        }

        public override int GetHashCode()
        {
            int hashCode = this.Version.GetHashCode();
            if (this.SpecialVersion != null)
            {
                hashCode = (hashCode * 0x11d7) + this.SpecialVersion.GetHashCode();
            }
            return hashCode;
        }

        public string[] GetOriginalVersionComponents()
        {
            if (string.IsNullOrEmpty(this._originalString))
            {
                return SplitAndPadVersionString(this.Version.ToString());
            }
            int index = this._originalString.IndexOf('-');
            string version = (index == -1) ? this._originalString : this._originalString.Substring(0, index);
            return SplitAndPadVersionString(version);
        }

        private static System.Version NormalizeVersionValue(System.Version version) => 
            new System.Version(version.Major, version.Minor, Math.Max(version.Build, 0), Math.Max(version.Revision, 0));

        public static bool operator ==(SemanticVersion version1, SemanticVersion version2) => 
            ((version1 != null) ? version1.Equals(version2) : ReferenceEquals(version2, null));

        public static bool operator >(SemanticVersion version1, SemanticVersion version2)
        {
            if (version1 == null)
            {
                throw new ArgumentNullException("version1");
            }
            return (version2 < version1);
        }

        public static bool operator >=(SemanticVersion version1, SemanticVersion version2) => 
            ((version1 == version2) || (version1 > version2));

        public static bool operator !=(SemanticVersion version1, SemanticVersion version2) => 
            !(version1 == version2);

        public static bool operator <(SemanticVersion version1, SemanticVersion version2)
        {
            if (version1 == null)
            {
                throw new ArgumentNullException("version1");
            }
            return (version1.CompareTo(version2) < 0);
        }

        public static bool operator <=(SemanticVersion version1, SemanticVersion version2) => 
            ((version1 == version2) || (version1 < version2));

        public static SemanticVersion Parse(string version)
        {
            SemanticVersion version2;
            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "version");
            }
            if (TryParse(version, out version2))
            {
                return version2;
            }
            object[] args = new object[] { version };
            throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, NuGetResources.InvalidVersionString, args), "version");
        }

        public static SemanticVersion ParseOptionalVersion(string version)
        {
            SemanticVersion version2;
            TryParse(version, out version2);
            return version2;
        }

        private static string[] SplitAndPadVersionString(string version)
        {
            char[] separator = new char[] { '.' };
            string[] sourceArray = version.Split(separator);
            if (sourceArray.Length == 4)
            {
                return sourceArray;
            }
            string[] destinationArray = new string[] { "0", "0", "0", "0" };
            Array.Copy(sourceArray, 0, destinationArray, 0, sourceArray.Length);
            return destinationArray;
        }

        public override string ToString() => 
            this._originalString;

        public static bool TryParse(string version, out SemanticVersion value) => 
            TryParseInternal(version, _semanticVersionRegex, out value);

        private static bool TryParseInternal(string version, Regex regex, out SemanticVersion semVer)
        {
            System.Version version2;
            semVer = null;
            if (string.IsNullOrEmpty(version))
            {
                return false;
            }
            Match match = regex.Match(version.Trim());
            if (!match.Success || !System.Version.TryParse(match.Groups["Version"].Value, out version2))
            {
                return false;
            }
            char[] trimChars = new char[] { '-' };
            semVer = new SemanticVersion(NormalizeVersionValue(version2), match.Groups["Release"].Value.TrimStart(trimChars), version.Replace(" ", ""));
            return true;
        }

        public static bool TryParseStrict(string version, out SemanticVersion value) => 
            TryParseInternal(version, _strictSemanticVersionRegex, out value);

        public System.Version Version { get; private set; }

        public string SpecialVersion { get; private set; }
    }
}

