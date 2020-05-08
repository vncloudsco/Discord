namespace NuGet
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal static class ManifestVersionUtility
    {
        public const int DefaultVersion = 1;
        public const int SemverVersion = 3;
        public const int TargetFrameworkSupportForDependencyContentsAndToolsVersion = 4;
        public const int TargetFrameworkSupportForReferencesVersion = 5;
        public const int XdtTransformationVersion = 6;
        private static readonly Type[] _xmlAttributes = new Type[] { typeof(XmlElementAttribute), typeof(XmlAttributeAttribute), typeof(XmlArrayAttribute) };

        public static int GetManifestVersion(ManifestMetadata metadata) => 
            Math.Max(VisitObject(metadata), GetMaxVersionFromMetadata(metadata));

        private static int GetMaxVersionFromMetadata(ManifestMetadata metadata)
        {
            SemanticVersion version;
            return (!((metadata.ReferenceSets != null) && Enumerable.Any<ManifestReferenceSet>(metadata.ReferenceSets, r => r.TargetFramework != null)) ? (!((metadata.DependencySets != null) && Enumerable.Any<ManifestDependencySet>(metadata.DependencySets, d => d.TargetFramework != null)) ? ((!SemanticVersion.TryParse(metadata.Version, out version) || string.IsNullOrEmpty(version.SpecialVersion)) ? 1 : 3) : 4) : 5);
        }

        private static int GetPropertyVersion(PropertyInfo property)
        {
            ManifestVersionAttribute customAttribute = property.GetCustomAttribute<ManifestVersionAttribute>();
            return ((customAttribute != null) ? customAttribute.Version : 1);
        }

        private static bool IsManifestMetadata(PropertyInfo property) => 
            Enumerable.Any<Type>(_xmlAttributes, attr => property.GetCustomAttribute(attr) != null);

        private static int VisitList(IList list)
        {
            int num = 1;
            foreach (object obj2 in list)
            {
                num = Math.Max(num, VisitObject(obj2));
            }
            return num;
        }

        private static int VisitObject(object obj) => 
            ((obj != null) ? ((IEnumerable<int>) (from property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance) select VisitProperty(obj, property))).Max() : 1);

        private static int VisitProperty(object obj, PropertyInfo property)
        {
            if (!IsManifestMetadata(property))
            {
                return 1;
            }
            object obj2 = property.GetValue(obj, null);
            if (obj2 == null)
            {
                return 1;
            }
            int propertyVersion = GetPropertyVersion(property);
            IList list = obj2 as IList;
            if (list != null)
            {
                return ((list.Count <= 0) ? propertyVersion : Math.Max(propertyVersion, VisitList(list)));
            }
            string str = obj2 as string;
            return ((str == null) ? propertyVersion : (string.IsNullOrEmpty(str) ? 1 : propertyVersion));
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ManifestVersionUtility.<>c <>9 = new ManifestVersionUtility.<>c();
            public static Func<ManifestReferenceSet, bool> <>9__7_0;
            public static Func<ManifestDependencySet, bool> <>9__7_1;

            internal bool <GetMaxVersionFromMetadata>b__7_0(ManifestReferenceSet r) => 
                (r.TargetFramework != null);

            internal bool <GetMaxVersionFromMetadata>b__7_1(ManifestDependencySet d) => 
                (d.TargetFramework != null);
        }
    }
}

