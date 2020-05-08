namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal abstract class LocalPackage : IPackage, IPackageMetadata, IPackageName, IServerPackageMetadata
    {
        private const string ResourceAssemblyExtension = ".resources.dll";
        private IList<IPackageAssemblyReference> _assemblyReferences;

        protected LocalPackage()
        {
            this.Listed = true;
        }

        protected abstract IEnumerable<IPackageAssemblyReference> GetAssemblyReferencesCore();
        public IEnumerable<IPackageFile> GetFiles() => 
            this.GetFilesBase();

        protected abstract IEnumerable<IPackageFile> GetFilesBase();
        public abstract Stream GetStream();
        public virtual IEnumerable<FrameworkName> GetSupportedFrameworks() => 
            (from f in this.FrameworkAssemblies select f.SupportedFrameworks).Distinct<FrameworkName>();

        protected internal static bool IsAssemblyReference(string filePath) => 
            (filePath.StartsWith(Constants.LibDirectory + Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase) ? ((Path.GetFileName(filePath) != "_._") ? (!filePath.EndsWith(".resources.dll", StringComparison.OrdinalIgnoreCase) && Constants.AssemblyReferencesExtensions.Contains<string>(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase)) : true) : false);

        protected void ReadManifest(Stream manifestStream)
        {
            IPackageMetadata metadata = Manifest.ReadFrom(manifestStream, false).Metadata;
            this.Id = metadata.Id;
            this.Version = metadata.Version;
            this.Title = metadata.Title;
            this.Authors = metadata.Authors;
            this.Owners = metadata.Owners;
            this.IconUrl = metadata.IconUrl;
            this.LicenseUrl = metadata.LicenseUrl;
            this.ProjectUrl = metadata.ProjectUrl;
            this.RequireLicenseAcceptance = metadata.RequireLicenseAcceptance;
            this.DevelopmentDependency = metadata.DevelopmentDependency;
            this.Description = metadata.Description;
            this.Summary = metadata.Summary;
            this.ReleaseNotes = metadata.ReleaseNotes;
            this.Language = metadata.Language;
            this.Tags = metadata.Tags;
            this.DependencySets = metadata.DependencySets;
            this.FrameworkAssemblies = metadata.FrameworkAssemblies;
            this.Copyright = metadata.Copyright;
            this.PackageAssemblyReferences = metadata.PackageAssemblyReferences;
            this.MinClientVersion = metadata.MinClientVersion;
            if (!string.IsNullOrEmpty(this.Tags))
            {
                this.Tags = " " + this.Tags + " ";
            }
        }

        public override string ToString() => 
            this.GetFullName();

        public string Id { get; set; }

        public SemanticVersion Version { get; set; }

        public string Title { get; set; }

        public IEnumerable<string> Authors { get; set; }

        public IEnumerable<string> Owners { get; set; }

        public Uri IconUrl { get; set; }

        public Uri LicenseUrl { get; set; }

        public Uri ProjectUrl { get; set; }

        public Uri ReportAbuseUrl =>
            null;

        public int DownloadCount =>
            -1;

        public bool RequireLicenseAcceptance { get; set; }

        public bool DevelopmentDependency { get; set; }

        public string Description { get; set; }

        public string Summary { get; set; }

        public string ReleaseNotes { get; set; }

        public string Language { get; set; }

        public string Tags { get; set; }

        public System.Version MinClientVersion { get; private set; }

        public bool IsAbsoluteLatestVersion =>
            true;

        public bool IsLatestVersion =>
            this.IsReleaseVersion();

        public bool Listed { get; set; }

        public DateTimeOffset? Published { get; set; }

        public string Copyright { get; set; }

        public IEnumerable<PackageDependencySet> DependencySets { get; set; }

        public IEnumerable<FrameworkAssemblyReference> FrameworkAssemblies { get; set; }

        public IEnumerable<IPackageAssemblyReference> AssemblyReferences
        {
            get
            {
                if (this._assemblyReferences == null)
                {
                    this._assemblyReferences = this.GetAssemblyReferencesCore().ToList<IPackageAssemblyReference>();
                }
                return this._assemblyReferences;
            }
        }

        public ICollection<PackageReferenceSet> PackageAssemblyReferences { get; private set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly LocalPackage.<>c <>9 = new LocalPackage.<>c();
            public static Func<FrameworkAssemblyReference, IEnumerable<FrameworkName>> <>9__101_0;

            internal IEnumerable<FrameworkName> <GetSupportedFrameworks>b__101_0(FrameworkAssemblyReference f) => 
                f.SupportedFrameworks;
        }
    }
}

