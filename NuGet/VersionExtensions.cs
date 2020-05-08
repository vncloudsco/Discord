namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    internal static class VersionExtensions
    {
        private static void AddVersionToList(string originalVersion, LinkedList<string> paths, string nextVersion)
        {
            if (nextVersion.Equals(originalVersion, StringComparison.OrdinalIgnoreCase))
            {
                paths.AddFirst(nextVersion);
            }
            else
            {
                paths.AddLast(nextVersion);
            }
        }

        public static IEnumerable<string> GetComparableVersionStrings(this SemanticVersion version)
        {
            Version version2 = version.Version;
            string str = string.IsNullOrEmpty(version.SpecialVersion) ? string.Empty : ("-" + version.SpecialVersion);
            string originalVersion = version.ToString();
            string[] originalVersionComponents = version.GetOriginalVersionComponents();
            LinkedList<string> paths = new LinkedList<string>();
            if (version2.Revision == 0)
            {
                if (version2.Build == 0)
                {
                    object[] objArray1 = new object[] { originalVersionComponents[0], originalVersionComponents[1], str };
                    AddVersionToList(originalVersion, paths, string.Format(CultureInfo.InvariantCulture, "{0}.{1}{2}", objArray1));
                }
                object[] objArray2 = new object[] { originalVersionComponents[0], originalVersionComponents[1], originalVersionComponents[2], str };
                AddVersionToList(originalVersion, paths, string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}{3}", objArray2));
            }
            object[] args = new object[] { originalVersionComponents[0], originalVersionComponents[1], originalVersionComponents[2], originalVersionComponents[3], str };
            AddVersionToList(originalVersion, paths, string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}{4}", args));
            return paths;
        }

        public static bool Satisfies(this IVersionSpec versionSpec, SemanticVersion version) => 
            ((versionSpec != null) ? versionSpec.ToDelegate<SemanticVersion>(v => v)(version) : true);

        public static Func<IPackage, bool> ToDelegate(this IVersionSpec versionInfo)
        {
            if (versionInfo == null)
            {
                throw new ArgumentNullException("versionInfo");
            }
            return versionInfo.ToDelegate<IPackage>(p => p.Version);
        }

        public static Func<T, bool> ToDelegate<T>(this IVersionSpec versionInfo, Func<T, SemanticVersion> extractor)
        {
            if (versionInfo == null)
            {
                throw new ArgumentNullException("versionInfo");
            }
            if (extractor == null)
            {
                throw new ArgumentNullException("extractor");
            }
            return delegate (T p) {
                SemanticVersion version = extractor(p);
                bool flag = true;
                if (versionInfo.MinVersion != null)
                {
                    flag = !versionInfo.IsMinInclusive ? (flag && (version > versionInfo.MinVersion)) : (flag && (version >= versionInfo.MinVersion));
                }
                if (versionInfo.MaxVersion != null)
                {
                    flag = !versionInfo.IsMaxInclusive ? (flag && (version < versionInfo.MaxVersion)) : (flag && (version <= versionInfo.MaxVersion));
                }
                return flag;
            };
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly VersionExtensions.<>c <>9 = new VersionExtensions.<>c();
            public static Func<IPackage, SemanticVersion> <>9__0_0;
            public static Func<SemanticVersion, SemanticVersion> <>9__2_0;

            internal SemanticVersion <Satisfies>b__2_0(SemanticVersion v) => 
                v;

            internal SemanticVersion <ToDelegate>b__0_0(IPackage p) => 
                p.Version;
        }
    }
}

