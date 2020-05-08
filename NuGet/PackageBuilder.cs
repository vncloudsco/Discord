namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.IO.Packaging;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;

    internal class PackageBuilder : IPackageBuilder, IPackageMetadata, IPackageName
    {
        private const string DefaultContentType = "application/octet";
        internal const string ManifestRelationType = "manifest";
        private readonly bool _includeEmptyDirectories;

        public PackageBuilder() : this(false)
        {
        }

        private PackageBuilder(bool includeEmptyDirectories)
        {
            this._includeEmptyDirectories = includeEmptyDirectories;
            this.Files = new Collection<IPackageFile>();
            this.DependencySets = new Collection<PackageDependencySet>();
            this.FrameworkReferences = new Collection<FrameworkAssemblyReference>();
            this.PackageAssemblyReferences = new Collection<PackageReferenceSet>();
            this.Authors = (ISet<string>) new HashSet<string>();
            this.Owners = (ISet<string>) new HashSet<string>();
            this.Tags = (ISet<string>) new HashSet<string>();
            this.Properties = new Dictionary<string, string>();
        }

        public PackageBuilder(Stream stream, string basePath) : this(stream, basePath, NullPropertyProvider.Instance)
        {
        }

        public PackageBuilder(Stream stream, string basePath, IPropertyProvider propertyProvider) : this()
        {
            this.ReadManifest(stream, basePath, propertyProvider);
        }

        public PackageBuilder(string path, IPropertyProvider propertyProvider, bool includeEmptyDirectories) : this(path, Path.GetDirectoryName(path), propertyProvider, includeEmptyDirectories)
        {
        }

        public PackageBuilder(string path, string basePath, IPropertyProvider propertyProvider, bool includeEmptyDirectories) : this(includeEmptyDirectories)
        {
            using (Stream stream = File.OpenRead(path))
            {
                this.ReadManifest(stream, basePath, propertyProvider);
            }
        }

        private void AddFiles(string basePath, string source, string destination, string exclude = null)
        {
            List<PhysicalPackageFile> searchFiles = PathResolver.ResolveSearchPattern(basePath, source, destination, this._includeEmptyDirectories).ToList<PhysicalPackageFile>();
            if (this._includeEmptyDirectories)
            {
                searchFiles.RemoveAll(file => (file.TargetFramework == null) && (Path.GetFileName(file.TargetPath) == "_._"));
            }
            ExcludeFiles(searchFiles, basePath, exclude);
            if (PathResolver.IsWildcardSearch(source) || (PathResolver.IsDirectoryPath(source) || searchFiles.Any<PhysicalPackageFile>()))
            {
                this.Files.AddRange<IPackageFile>(searchFiles);
            }
            else
            {
                object[] args = new object[] { source };
                throw new FileNotFoundException(string.Format(CultureInfo.CurrentCulture, NuGetResources.PackageAuthoring_FileNotFound, args));
            }
        }

        private static void CreatePart(Package package, string path, Stream sourceStream)
        {
            if (!PackageHelper.IsManifest(path))
            {
                Uri partUri = UriUtility.CreatePartUri(path);
                using (Stream stream = package.CreatePart(partUri, "application/octet", CompressionOption.Maximum).GetStream())
                {
                    sourceStream.CopyTo(stream);
                }
            }
        }

        private static string CreatorInfo()
        {
            List<string> values = new List<string>();
            Assembly assembly = typeof(PackageBuilder).Assembly;
            values.Add(assembly.FullName);
            values.Add(Environment.OSVersion.ToString());
            object[] customAttributes = assembly.GetCustomAttributes(typeof(TargetFrameworkAttribute), true);
            if (customAttributes.Length != 0)
            {
                values.Add(((TargetFrameworkAttribute) customAttributes[0]).FrameworkDisplayName);
            }
            return string.Join(";", values);
        }

        private static int DetermineMinimumSchemaVersion(Collection<IPackageFile> Files) => 
            (!HasXdtTransformFile(Files) ? (!RequiresV4TargetFrameworkSchema(Files) ? 1 : 4) : 6);

        private static void ExcludeFiles(List<PhysicalPackageFile> searchFiles, string basePath, string exclude)
        {
            if (!string.IsNullOrEmpty(exclude))
            {
                char[] separator = new char[] { ';' };
                foreach (string str in exclude.Split(separator, StringSplitOptions.RemoveEmptyEntries))
                {
                    string str2 = PathResolver.NormalizeWildcardForExcludedFiles(basePath, str);
                    string[] wildcards = new string[] { str2 };
                    PathResolver.FilterPackageFiles<PhysicalPackageFile>(searchFiles, p => p.SourcePath, wildcards);
                }
            }
        }

        private static bool HasXdtTransformFile(ICollection<IPackageFile> contentFiles) => 
            Enumerable.Any<IPackageFile>(contentFiles, file => (file.Path != null) && (file.Path.StartsWith(Constants.ContentDirectory + Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase) && (file.Path.EndsWith(".install.xdt", StringComparison.OrdinalIgnoreCase) || file.Path.EndsWith(".uninstall.xdt", StringComparison.OrdinalIgnoreCase))));

        private static bool IsPrereleaseDependency(PackageDependency dependency)
        {
            IVersionSpec versionSpec = dependency.VersionSpec;
            return ((versionSpec != null) && (((versionSpec.MinVersion == null) || string.IsNullOrEmpty(dependency.VersionSpec.MinVersion.SpecialVersion)) ? ((versionSpec.MaxVersion != null) && !string.IsNullOrEmpty(dependency.VersionSpec.MaxVersion.SpecialVersion)) : true));
        }

        private static IEnumerable<string> ParseTags(string tags)
        {
            char[] separator = new char[] { ' ' };
            return (from tag in tags.Split(separator, StringSplitOptions.RemoveEmptyEntries) select tag.Trim());
        }

        public void Populate(ManifestMetadata manifestMetadata)
        {
            IPackageMetadata metadata = manifestMetadata;
            this.Id = metadata.Id;
            this.Version = metadata.Version;
            this.Title = metadata.Title;
            this.Authors.AddRange<string>(metadata.Authors);
            this.Owners.AddRange<string>(metadata.Owners);
            this.IconUrl = metadata.IconUrl;
            this.LicenseUrl = metadata.LicenseUrl;
            this.ProjectUrl = metadata.ProjectUrl;
            this.RequireLicenseAcceptance = metadata.RequireLicenseAcceptance;
            this.DevelopmentDependency = metadata.DevelopmentDependency;
            this.Description = metadata.Description;
            this.Summary = metadata.Summary;
            this.ReleaseNotes = metadata.ReleaseNotes;
            this.Language = metadata.Language;
            this.Copyright = metadata.Copyright;
            this.MinClientVersion = metadata.MinClientVersion;
            if (metadata.Tags != null)
            {
                this.Tags.AddRange<string>(ParseTags(metadata.Tags));
            }
            this.DependencySets.AddRange<PackageDependencySet>(metadata.DependencySets);
            this.FrameworkReferences.AddRange<FrameworkAssemblyReference>(metadata.FrameworkAssemblies);
            if (manifestMetadata.ReferenceSets != null)
            {
                this.PackageAssemblyReferences.AddRange<PackageReferenceSet>(from r in manifestMetadata.ReferenceSets select new PackageReferenceSet(r));
            }
        }

        public void PopulateFiles(string basePath, IEnumerable<ManifestFile> files)
        {
            foreach (ManifestFile file in files)
            {
                this.AddFiles(basePath, file.Source, file.Target, file.Exclude);
            }
        }

        private void ReadManifest(Stream stream, string basePath, IPropertyProvider propertyProvider)
        {
            Manifest manifest = Manifest.ReadFrom(stream, propertyProvider, true);
            this.Populate(manifest.Metadata);
            if (basePath != null)
            {
                if (manifest.Files == null)
                {
                    this.AddFiles(basePath, @"**\*", null, null);
                }
                else
                {
                    this.PopulateFiles(basePath, manifest.Files);
                }
            }
        }

        private static bool RequiresV4TargetFrameworkSchema(ICollection<IPackageFile> files) => 
            (!Enumerable.Any<IPackageFile>(files, f => ((f.TargetFramework != null) && (f.TargetFramework != VersionUtility.UnsupportedFrameworkName)) && (f.Path.StartsWith(Constants.ContentDirectory + Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase) || f.Path.StartsWith(Constants.ToolsDirectory + Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))) ? Enumerable.Any<IPackageFile>(files, f => (f.TargetFramework != null) && (f.Path.StartsWith(Constants.LibDirectory + Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase) && (f.EffectivePath == "_._"))) : true);

        public void Save(Stream stream)
        {
            PackageIdValidator.ValidatePackageId(this.Id);
            if (!this.Files.Any<IPackageFile>() && (!(from d in this.DependencySets select d.Dependencies).Any<PackageDependency>() && !this.FrameworkReferences.Any<FrameworkAssemblyReference>()))
            {
                throw new InvalidOperationException(NuGetResources.CannotCreateEmptyPackage);
            }
            if (!ValidateSpecialVersionLength(this.Version))
            {
                throw new InvalidOperationException(NuGetResources.SemVerSpecialVersionTooLong);
            }
            ValidateDependencySets(this.Version, this.DependencySets);
            ValidateReferenceAssemblies(this.Files, this.PackageAssemblyReferences);
            using (Package package = Package.Open(stream, FileMode.Create))
            {
                this.WriteManifest(package, DetermineMinimumSchemaVersion(this.Files));
                this.WriteFiles(package);
                package.PackageProperties.Creator = string.Join(",", (IEnumerable<string>) this.Authors);
                package.PackageProperties.Description = this.Description;
                package.PackageProperties.Identifier = this.Id;
                package.PackageProperties.Version = this.Version.ToString();
                package.PackageProperties.Language = this.Language;
                package.PackageProperties.Keywords = ((IPackageMetadata) this).Tags;
                package.PackageProperties.Title = this.Title;
                package.PackageProperties.LastModifiedBy = CreatorInfo();
            }
        }

        internal static void ValidateDependencySets(SemanticVersion version, IEnumerable<PackageDependencySet> dependencies)
        {
            if (version != null)
            {
                using (IEnumerator<PackageDependency> enumerator = (from s in dependencies select s.Dependencies).GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        PackageIdValidator.ValidatePackageId(enumerator.Current.Id);
                    }
                }
                if (string.IsNullOrEmpty(version.SpecialVersion))
                {
                    PackageDependency dependency = Enumerable.FirstOrDefault<PackageDependency>(from set in dependencies select set.Dependencies, new Func<PackageDependency, bool>(PackageBuilder.IsPrereleaseDependency));
                    if (dependency != null)
                    {
                        object[] args = new object[] { dependency.ToString() };
                        throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, NuGetResources.Manifest_InvalidPrereleaseDependency, args));
                    }
                }
            }
        }

        internal static void ValidateReferenceAssemblies(IEnumerable<IPackageFile> files, IEnumerable<PackageReferenceSet> packageAssemblyReferences)
        {
            HashSet<string> set = new HashSet<string>(from file in files
                where !string.IsNullOrEmpty(file.Path) && file.Path.StartsWith("lib", StringComparison.OrdinalIgnoreCase)
                select Path.GetFileName(file.Path), StringComparer.OrdinalIgnoreCase);
            foreach (string str in from p in packageAssemblyReferences select p.References)
            {
                if (!set.Contains(str) && (!set.Contains(str + ".dll") && (!set.Contains(str + ".exe") && !set.Contains(str + ".winmd"))))
                {
                    object[] args = new object[] { str };
                    throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, NuGetResources.Manifest_InvalidReference, args));
                }
            }
        }

        private static bool ValidateSpecialVersionLength(SemanticVersion version) => 
            ((version == null) || ((version.SpecialVersion == null) || (version.SpecialVersion.Length <= 20)));

        private void WriteFiles(Package package)
        {
            foreach (IPackageFile file in new HashSet<IPackageFile>(this.Files))
            {
                Stream sourceStream = file.GetStream();
                try
                {
                    CreatePart(package, file.Path, sourceStream);
                }
                catch
                {
                    Console.WriteLine(file.Path);
                    throw;
                }
                finally
                {
                    if (sourceStream != null)
                    {
                        sourceStream.Dispose();
                    }
                }
            }
            using (IEnumerator<IGrouping<Uri, PackagePart>> enumerator2 = (from s in package.GetParts()
                group s by s.Uri into _
                where _.Count<PackagePart>() > 1
                select _).GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    Console.WriteLine(enumerator2.Current.Key);
                }
            }
        }

        private void WriteManifest(Package package, int minimumManifestVersion)
        {
            Uri targetUri = UriUtility.CreatePartUri(this.Id + Constants.ManifestExtension);
            package.CreateRelationship(targetUri, TargetMode.Internal, "http://schemas.microsoft.com/packaging/2010/07/manifest");
            using (Stream stream = package.CreatePart(targetUri, "application/octet", CompressionOption.Maximum).GetStream())
            {
                Manifest.Create(this).Save(stream, minimumManifestVersion);
            }
        }

        public string Id { get; set; }

        public SemanticVersion Version { get; set; }

        public string Title { get; set; }

        public ISet<string> Authors { get; private set; }

        public ISet<string> Owners { get; private set; }

        public Uri IconUrl { get; set; }

        public Uri LicenseUrl { get; set; }

        public Uri ProjectUrl { get; set; }

        public bool RequireLicenseAcceptance { get; set; }

        public bool DevelopmentDependency { get; set; }

        public string Description { get; set; }

        public string Summary { get; set; }

        public string ReleaseNotes { get; set; }

        public string Language { get; set; }

        public ISet<string> Tags { get; private set; }

        public Dictionary<string, string> Properties { get; private set; }

        public string Copyright { get; set; }

        public Collection<PackageDependencySet> DependencySets { get; private set; }

        public Collection<IPackageFile> Files { get; private set; }

        public Collection<FrameworkAssemblyReference> FrameworkReferences { get; private set; }

        public ICollection<PackageReferenceSet> PackageAssemblyReferences { get; private set; }

        IEnumerable<string> IPackageMetadata.Authors =>
            this.Authors;

        IEnumerable<string> IPackageMetadata.Owners =>
            this.Owners;

        string IPackageMetadata.Tags =>
            string.Join(" ", (IEnumerable<string>) this.Tags);

        IEnumerable<PackageDependencySet> IPackageMetadata.DependencySets =>
            this.DependencySets;

        IEnumerable<FrameworkAssemblyReference> IPackageMetadata.FrameworkAssemblies =>
            this.FrameworkReferences;

        public System.Version MinClientVersion { get; set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PackageBuilder.<>c <>9 = new PackageBuilder.<>c();
            public static Func<PackageDependencySet, IEnumerable<PackageDependency>> <>9__107_0;
            public static Func<IPackageFile, bool> <>9__110_0;
            public static Func<IPackageFile, bool> <>9__110_1;
            public static Func<IPackageFile, bool> <>9__111_0;
            public static Func<PackageDependencySet, IEnumerable<PackageDependency>> <>9__112_0;
            public static Func<PackageDependencySet, IEnumerable<PackageDependency>> <>9__112_1;
            public static Func<IPackageFile, bool> <>9__113_0;
            public static Func<IPackageFile, string> <>9__113_1;
            public static Func<PackageReferenceSet, IEnumerable<string>> <>9__113_2;
            public static Func<ManifestReferenceSet, PackageReferenceSet> <>9__115_0;
            public static Func<PackagePart, Uri> <>9__118_0;
            public static Func<IGrouping<Uri, PackagePart>, bool> <>9__118_1;
            public static Predicate<PhysicalPackageFile> <>9__119_0;
            public static Func<PhysicalPackageFile, string> <>9__120_0;
            public static Func<string, string> <>9__122_0;

            internal bool <AddFiles>b__119_0(PhysicalPackageFile file) => 
                ((file.TargetFramework == null) && (Path.GetFileName(file.TargetPath) == "_._"));

            internal string <ExcludeFiles>b__120_0(PhysicalPackageFile p) => 
                p.SourcePath;

            internal bool <HasXdtTransformFile>b__111_0(IPackageFile file) => 
                ((file.Path != null) && (file.Path.StartsWith(Constants.ContentDirectory + Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase) && (file.Path.EndsWith(".install.xdt", StringComparison.OrdinalIgnoreCase) || file.Path.EndsWith(".uninstall.xdt", StringComparison.OrdinalIgnoreCase))));

            internal string <ParseTags>b__122_0(string tag) => 
                tag.Trim();

            internal PackageReferenceSet <Populate>b__115_0(ManifestReferenceSet r) => 
                new PackageReferenceSet(r);

            internal bool <RequiresV4TargetFrameworkSchema>b__110_0(IPackageFile f) => 
                (((f.TargetFramework != null) && (f.TargetFramework != VersionUtility.UnsupportedFrameworkName)) && (f.Path.StartsWith(Constants.ContentDirectory + Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase) || f.Path.StartsWith(Constants.ToolsDirectory + Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase)));

            internal bool <RequiresV4TargetFrameworkSchema>b__110_1(IPackageFile f) => 
                ((f.TargetFramework != null) && (f.Path.StartsWith(Constants.LibDirectory + Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase) && (f.EffectivePath == "_._")));

            internal IEnumerable<PackageDependency> <Save>b__107_0(PackageDependencySet d) => 
                d.Dependencies;

            internal IEnumerable<PackageDependency> <ValidateDependencySets>b__112_0(PackageDependencySet s) => 
                s.Dependencies;

            internal IEnumerable<PackageDependency> <ValidateDependencySets>b__112_1(PackageDependencySet set) => 
                set.Dependencies;

            internal bool <ValidateReferenceAssemblies>b__113_0(IPackageFile file) => 
                (!string.IsNullOrEmpty(file.Path) && file.Path.StartsWith("lib", StringComparison.OrdinalIgnoreCase));

            internal string <ValidateReferenceAssemblies>b__113_1(IPackageFile file) => 
                Path.GetFileName(file.Path);

            internal IEnumerable<string> <ValidateReferenceAssemblies>b__113_2(PackageReferenceSet p) => 
                p.References;

            internal Uri <WriteFiles>b__118_0(PackagePart s) => 
                s.Uri;

            internal bool <WriteFiles>b__118_1(IGrouping<Uri, PackagePart> _) => 
                (_.Count<PackagePart>() > 1);
        }
    }
}

