namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;
    using System.Xml.Serialization;

    [XmlType("metadata")]
    internal class ManifestMetadata : IPackageMetadata, IPackageName, IValidatableObject
    {
        private string _owners;
        private string _minClientVersionString;

        private static PackageDependencySet CreatePackageDependencySet(ManifestDependencySet manifestDependencySet) => 
            new PackageDependencySet((manifestDependencySet.TargetFramework == null) ? null : VersionUtility.ParseFrameworkName(manifestDependencySet.TargetFramework), from d in manifestDependencySet.Dependencies select new PackageDependency(d.Id, string.IsNullOrEmpty(d.Version) ? null : VersionUtility.ParseVersionSpec(d.Version)));

        private static IEnumerable<FrameworkName> ParseFrameworkNames(string frameworkNames)
        {
            if (string.IsNullOrEmpty(frameworkNames))
            {
                return Enumerable.Empty<FrameworkName>();
            }
            char[] separator = new char[] { ',' };
            return Enumerable.Select<string, FrameworkName>(frameworkNames.Split(separator, StringSplitOptions.RemoveEmptyEntries), new Func<string, FrameworkName>(VersionUtility.ParseFrameworkName));
        }

        [IteratorStateMachine(typeof(<Validate>d__108))]
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(this.Id))
            {
                if (this.Id.Length > 100)
                {
                    yield return new ValidationResult(string.Format(CultureInfo.CurrentCulture, NuGetResources.Manifest_IdMaxLengthExceeded, new object[0]));
                }
                else if (!PackageIdValidator.IsValidPackageId(this.Id))
                {
                    object[] args = new object[] { this.Id };
                    yield return new ValidationResult(string.Format(CultureInfo.CurrentCulture, NuGetResources.InvalidPackageId, args));
                }
            }
            if (this.LicenseUrl == string.Empty)
            {
                object[] args = new object[] { "LicenseUrl" };
                yield return new ValidationResult(string.Format(CultureInfo.CurrentCulture, NuGetResources.Manifest_UriCannotBeEmpty, args));
            }
            if (this.IconUrl == string.Empty)
            {
                object[] args = new object[] { "IconUrl" };
                yield return new ValidationResult(string.Format(CultureInfo.CurrentCulture, NuGetResources.Manifest_UriCannotBeEmpty, args));
            }
            if (this.ProjectUrl == string.Empty)
            {
                object[] args = new object[] { "ProjectUrl" };
                yield return new ValidationResult(string.Format(CultureInfo.CurrentCulture, NuGetResources.Manifest_UriCannotBeEmpty, args));
            }
            while (true)
            {
                if (this.RequireLicenseAcceptance && string.IsNullOrWhiteSpace(this.LicenseUrl))
                {
                    yield return new ValidationResult(NuGetResources.Manifest_RequireLicenseAcceptanceRequiresLicenseUrl);
                }
            }
        }

        [ManifestVersion(5), XmlAttribute("minClientVersion")]
        public string MinClientVersionString
        {
            get => 
                this._minClientVersionString;
            set
            {
                System.Version result = null;
                if (!string.IsNullOrEmpty(value) && !System.Version.TryParse(value, out result))
                {
                    throw new InvalidDataException(NuGetResources.Manifest_InvalidMinClientVersion);
                }
                this._minClientVersionString = value;
                this.MinClientVersion = result;
            }
        }

        [XmlIgnore]
        public System.Version MinClientVersion { get; private set; }

        [Required(ErrorMessageResourceType=typeof(NuGetResources), ErrorMessageResourceName="Manifest_RequiredMetadataMissing"), XmlElement("id")]
        public string Id { get; set; }

        [XmlElement("version"), Required(ErrorMessageResourceType=typeof(NuGetResources), ErrorMessageResourceName="Manifest_RequiredMetadataMissing")]
        public string Version { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [Required(ErrorMessageResourceType=typeof(NuGetResources), ErrorMessageResourceName="Manifest_RequiredMetadataMissing"), XmlElement("authors")]
        public string Authors { get; set; }

        [XmlElement("owners")]
        public string Owners
        {
            get => 
                (this._owners ?? this.Authors);
            set => 
                (this._owners = value);
        }

        [XmlElement("licenseUrl")]
        public string LicenseUrl { get; set; }

        [XmlElement("projectUrl")]
        public string ProjectUrl { get; set; }

        [XmlElement("iconUrl")]
        public string IconUrl { get; set; }

        [XmlElement("requireLicenseAcceptance")]
        public bool RequireLicenseAcceptance { get; set; }

        [XmlElement("developmentDependency"), DefaultValue(false)]
        public bool DevelopmentDependency { get; set; }

        [XmlElement("description"), Required(ErrorMessageResourceType=typeof(NuGetResources), ErrorMessageResourceName="Manifest_RequiredMetadataMissing")]
        public string Description { get; set; }

        [XmlElement("summary")]
        public string Summary { get; set; }

        [ManifestVersion(2), XmlElement("releaseNotes")]
        public string ReleaseNotes { get; set; }

        [XmlElement("copyright"), ManifestVersion(2)]
        public string Copyright { get; set; }

        [XmlElement("language")]
        public string Language { get; set; }

        [XmlElement("tags")]
        public string Tags { get; set; }

        [XmlArray("dependencies", IsNullable=false), XmlArrayItem("group", typeof(ManifestDependencySet)), XmlArrayItem("dependency", typeof(ManifestDependency))]
        public List<object> DependencySetsSerialize
        {
            get => 
                (((this.DependencySets == null) || (this.DependencySets.Count == 0)) ? null : (!Enumerable.Any<ManifestDependencySet>(this.DependencySets, set => set.TargetFramework != null) ? (from set in this.DependencySets select set.Dependencies).Cast<object>().ToList<object>() : this.DependencySets.Cast<object>().ToList<object>()));
            set
            {
                throw new InvalidOperationException();
            }
        }

        [XmlIgnore]
        public List<ManifestDependencySet> DependencySets { get; set; }

        [XmlArray("frameworkAssemblies"), XmlArrayItem("frameworkAssembly")]
        public List<ManifestFrameworkAssembly> FrameworkAssemblies { get; set; }

        [ManifestVersion(2), XmlArray("references", IsNullable=false), XmlArrayItem("group", typeof(ManifestReferenceSet)), XmlArrayItem("reference", typeof(ManifestReference))]
        public List<object> ReferenceSetsSerialize
        {
            get => 
                (((this.ReferenceSets == null) || (this.ReferenceSets.Count == 0)) ? null : (!Enumerable.Any<ManifestReferenceSet>(this.ReferenceSets, set => set.TargetFramework != null) ? (from set in this.ReferenceSets select set.References).Cast<object>().ToList<object>() : this.ReferenceSets.Cast<object>().ToList<object>()));
            set
            {
                throw new InvalidOperationException();
            }
        }

        [XmlIgnore]
        public List<ManifestReferenceSet> ReferenceSets { get; set; }

        SemanticVersion IPackageName.Version =>
            ((this.Version != null) ? new SemanticVersion(this.Version) : null);

        Uri IPackageMetadata.IconUrl =>
            ((this.IconUrl != null) ? new Uri(this.IconUrl) : null);

        Uri IPackageMetadata.LicenseUrl =>
            ((this.LicenseUrl != null) ? new Uri(this.LicenseUrl) : null);

        Uri IPackageMetadata.ProjectUrl =>
            ((this.ProjectUrl != null) ? new Uri(this.ProjectUrl) : null);

        IEnumerable<string> IPackageMetadata.Authors
        {
            get
            {
                if (string.IsNullOrEmpty(this.Authors))
                {
                    return Enumerable.Empty<string>();
                }
                char[] separator = new char[] { ',' };
                return this.Authors.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        IEnumerable<string> IPackageMetadata.Owners
        {
            get
            {
                if (string.IsNullOrEmpty(this.Owners))
                {
                    return Enumerable.Empty<string>();
                }
                char[] separator = new char[] { ',' };
                return this.Owners.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        IEnumerable<PackageDependencySet> IPackageMetadata.DependencySets
        {
            get
            {
                if (this.DependencySets == null)
                {
                    return Enumerable.Empty<PackageDependencySet>();
                }
                List<PackageDependencySet> list = (from set in Enumerable.Select<ManifestDependencySet, PackageDependencySet>(this.DependencySets, new Func<ManifestDependencySet, PackageDependencySet>(ManifestMetadata.CreatePackageDependencySet))
                    group set by set.TargetFramework into group
                    select new PackageDependencySet(group.Key, from g in group select g.Dependencies)).ToList<PackageDependencySet>();
                int index = list.FindIndex(set => set.TargetFramework == null);
                if (index > -1)
                {
                    list.RemoveAt(index);
                    list.Insert(0, list[index]);
                }
                return list;
            }
        }

        ICollection<PackageReferenceSet> IPackageMetadata.PackageAssemblyReferences
        {
            get
            {
                if (this.ReferenceSets == null)
                {
                    return new PackageReferenceSet[0];
                }
                List<PackageReferenceSet> list = (from set in from r in this.ReferenceSets select new PackageReferenceSet(r)
                    group set by set.TargetFramework into group
                    select new PackageReferenceSet(group.Key, from g in group select g.References)).ToList<PackageReferenceSet>();
                int index = list.FindIndex(set => set.TargetFramework == null);
                if (index > -1)
                {
                    list.RemoveAt(index);
                    list.Insert(0, list[index]);
                }
                return list;
            }
        }

        IEnumerable<FrameworkAssemblyReference> IPackageMetadata.FrameworkAssemblies =>
            ((this.FrameworkAssemblies != null) ? (from frameworkReference in this.FrameworkAssemblies select new FrameworkAssemblyReference(frameworkReference.AssemblyName, ParseFrameworkNames(frameworkReference.TargetFramework))) : Enumerable.Empty<FrameworkAssemblyReference>());

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ManifestMetadata.<>c <>9 = new ManifestMetadata.<>c();
            public static Func<ManifestDependencySet, bool> <>9__73_0;
            public static Func<ManifestDependencySet, IEnumerable<ManifestDependency>> <>9__73_1;
            public static Func<ManifestReferenceSet, bool> <>9__84_0;
            public static Func<ManifestReferenceSet, IEnumerable<ManifestReference>> <>9__84_1;
            public static Func<PackageDependencySet, FrameworkName> <>9__103_0;
            public static Func<PackageDependencySet, IEnumerable<PackageDependency>> <>9__103_2;
            public static Func<IGrouping<FrameworkName, PackageDependencySet>, PackageDependencySet> <>9__103_1;
            public static Predicate<PackageDependencySet> <>9__103_3;
            public static Func<ManifestReferenceSet, PackageReferenceSet> <>9__105_0;
            public static Func<PackageReferenceSet, FrameworkName> <>9__105_1;
            public static Func<PackageReferenceSet, IEnumerable<string>> <>9__105_3;
            public static Func<IGrouping<FrameworkName, PackageReferenceSet>, PackageReferenceSet> <>9__105_2;
            public static Predicate<PackageReferenceSet> <>9__105_4;
            public static Func<ManifestFrameworkAssembly, FrameworkAssemblyReference> <>9__107_0;
            public static Func<ManifestDependency, PackageDependency> <>9__110_0;

            internal PackageDependency <CreatePackageDependencySet>b__110_0(ManifestDependency d) => 
                new PackageDependency(d.Id, string.IsNullOrEmpty(d.Version) ? null : VersionUtility.ParseVersionSpec(d.Version));

            internal bool <get_DependencySetsSerialize>b__73_0(ManifestDependencySet set) => 
                (set.TargetFramework != null);

            internal IEnumerable<ManifestDependency> <get_DependencySetsSerialize>b__73_1(ManifestDependencySet set) => 
                set.Dependencies;

            internal bool <get_ReferenceSetsSerialize>b__84_0(ManifestReferenceSet set) => 
                (set.TargetFramework != null);

            internal IEnumerable<ManifestReference> <get_ReferenceSetsSerialize>b__84_1(ManifestReferenceSet set) => 
                set.References;

            internal FrameworkName <NuGet.IPackageMetadata.get_DependencySets>b__103_0(PackageDependencySet set) => 
                set.TargetFramework;

            internal PackageDependencySet <NuGet.IPackageMetadata.get_DependencySets>b__103_1(IGrouping<FrameworkName, PackageDependencySet> group) => 
                new PackageDependencySet(group.Key, from g in group select g.Dependencies);

            internal IEnumerable<PackageDependency> <NuGet.IPackageMetadata.get_DependencySets>b__103_2(PackageDependencySet g) => 
                g.Dependencies;

            internal bool <NuGet.IPackageMetadata.get_DependencySets>b__103_3(PackageDependencySet set) => 
                (set.TargetFramework == null);

            internal FrameworkAssemblyReference <NuGet.IPackageMetadata.get_FrameworkAssemblies>b__107_0(ManifestFrameworkAssembly frameworkReference) => 
                new FrameworkAssemblyReference(frameworkReference.AssemblyName, ManifestMetadata.ParseFrameworkNames(frameworkReference.TargetFramework));

            internal PackageReferenceSet <NuGet.IPackageMetadata.get_PackageAssemblyReferences>b__105_0(ManifestReferenceSet r) => 
                new PackageReferenceSet(r);

            internal FrameworkName <NuGet.IPackageMetadata.get_PackageAssemblyReferences>b__105_1(PackageReferenceSet set) => 
                set.TargetFramework;

            internal PackageReferenceSet <NuGet.IPackageMetadata.get_PackageAssemblyReferences>b__105_2(IGrouping<FrameworkName, PackageReferenceSet> group) => 
                new PackageReferenceSet(group.Key, from g in group select g.References);

            internal IEnumerable<string> <NuGet.IPackageMetadata.get_PackageAssemblyReferences>b__105_3(PackageReferenceSet g) => 
                g.References;

            internal bool <NuGet.IPackageMetadata.get_PackageAssemblyReferences>b__105_4(PackageReferenceSet set) => 
                (set.TargetFramework == null);
        }

    }
}

