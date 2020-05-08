namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;

    internal class PackageReferenceRepository : IPackageReferenceRepository, IPackageRepository, IPackageLookup, IPackageConstraintProvider, ILatestPackageLookup, IPackageReferenceRepository2
    {
        private readonly PackageReferenceFile _packageReferenceFile;

        public PackageReferenceRepository(string configFilePath, ISharedPackageRepository sourceRepository)
        {
            if (string.IsNullOrEmpty(configFilePath))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "configFilePath");
            }
            if (sourceRepository == null)
            {
                throw new ArgumentNullException("sourceRepository");
            }
            this._packageReferenceFile = new PackageReferenceFile(configFilePath);
            this.SourceRepository = sourceRepository;
        }

        public PackageReferenceRepository(IFileSystem fileSystem, string projectName, ISharedPackageRepository sourceRepository)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException("fileSystem");
            }
            if (sourceRepository == null)
            {
                throw new ArgumentNullException("sourceRepository");
            }
            this._packageReferenceFile = new PackageReferenceFile(fileSystem, Constants.PackageReferenceFile, projectName);
            this.SourceRepository = sourceRepository;
        }

        public void AddPackage(IPackage package)
        {
            this.AddPackage(package.Id, package.Version, package.DevelopmentDependency, null);
        }

        public void AddPackage(string packageId, SemanticVersion version, bool developmentDependency, FrameworkName targetFramework)
        {
            this._packageReferenceFile.AddEntry(packageId, version, developmentDependency, targetFramework);
            this.SourceRepository.RegisterRepository(this._packageReferenceFile);
        }

        public bool Exists(string packageId, SemanticVersion version) => 
            this._packageReferenceFile.EntryExists(packageId, version);

        public IPackage FindPackage(string packageId, SemanticVersion version) => 
            (this._packageReferenceFile.EntryExists(packageId, version) ? this.SourceRepository.FindPackage(packageId, version) : null);

        public IEnumerable<IPackage> FindPackagesById(string packageId) => 
            (from p in Enumerable.Select<PackageReference, IPackage>(this.GetPackageReferences(packageId), new Func<PackageReference, IPackage>(this.GetPackage))
                where p != null
                select p);

        public IVersionSpec GetConstraint(string packageId)
        {
            PackageReference packageReference = this.GetPackageReference(packageId);
            return packageReference?.VersionConstraint;
        }

        private IPackage GetPackage(PackageReference reference) => 
            (!IsValidReference(reference) ? null : this.SourceRepository.FindPackage(reference.Id, reference.Version));

        public PackageReference GetPackageReference(string packageId) => 
            this.GetPackageReferences(packageId).FirstOrDefault<PackageReference>();

        public IEnumerable<PackageReference> GetPackageReferences() => 
            this._packageReferenceFile.GetPackageReferences();

        public IEnumerable<PackageReference> GetPackageReferences(string packageId) => 
            (from reference in this._packageReferenceFile.GetPackageReferences()
                where IsValidReference(reference) && reference.Id.Equals(packageId, StringComparison.OrdinalIgnoreCase)
                select reference);

        public IQueryable<IPackage> GetPackages() => 
            this.GetPackagesCore().AsQueryable<IPackage>();

        private IEnumerable<IPackage> GetPackagesCore() => 
            (from p in Enumerable.Select<PackageReference, IPackage>(this._packageReferenceFile.GetPackageReferences(), new Func<PackageReference, IPackage>(this.GetPackage))
                where p != null
                select p);

        public FrameworkName GetPackageTargetFramework(string packageId)
        {
            PackageReference packageReference = this.GetPackageReference(packageId);
            return packageReference?.TargetFramework;
        }

        private static bool IsValidReference(PackageReference reference) => 
            (!string.IsNullOrEmpty(reference.Id) && (reference.Version != null));

        public void RegisterIfNecessary()
        {
            if (this.GetPackages().Any<IPackage>())
            {
                this.SourceRepository.RegisterRepository(this._packageReferenceFile);
            }
        }

        public void RemovePackage(IPackage package)
        {
            if (this._packageReferenceFile.DeleteEntry(package.Id, package.Version))
            {
                this.SourceRepository.UnregisterRepository(this._packageReferenceFile);
            }
        }

        public bool TryFindLatestPackageById(string id, out SemanticVersion latestVersion)
        {
            PackageReference reference = (from r in this.GetPackageReferences(id)
                orderby r.Version descending
                select r).FirstOrDefault<PackageReference>();
            if (reference == null)
            {
                latestVersion = null;
                return false;
            }
            latestVersion = reference.Version;
            return true;
        }

        public bool TryFindLatestPackageById(string id, bool includePrerelease, out IPackage package)
        {
            IEnumerable<PackageReference> packageReferences = this.GetPackageReferences(id);
            if (!includePrerelease)
            {
                packageReferences = from r in packageReferences
                    where string.IsNullOrEmpty(r.Version.SpecialVersion)
                    select r;
            }
            PackageReference reference = (from r in packageReferences
                orderby r.Version descending
                select r).FirstOrDefault<PackageReference>();
            if (reference != null)
            {
                package = this.GetPackage(reference);
                return true;
            }
            package = null;
            return false;
        }

        public string Source =>
            Constants.PackageReferenceFile;

        public PackageSaveModes PackageSaveMode
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public bool SupportsPrereleasePackages =>
            true;

        private ISharedPackageRepository SourceRepository { get; set; }

        public PackageReferenceFile ReferenceFile =>
            this._packageReferenceFile;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PackageReferenceRepository.<>c <>9 = new PackageReferenceRepository.<>c();
            public static Func<IPackage, bool> <>9__17_0;
            public static Func<IPackage, bool> <>9__21_0;
            public static Func<PackageReference, SemanticVersion> <>9__25_0;
            public static Func<PackageReference, bool> <>9__26_0;
            public static Func<PackageReference, SemanticVersion> <>9__26_1;

            internal bool <FindPackagesById>b__21_0(IPackage p) => 
                (p != null);

            internal bool <GetPackagesCore>b__17_0(IPackage p) => 
                (p != null);

            internal SemanticVersion <TryFindLatestPackageById>b__25_0(PackageReference r) => 
                r.Version;

            internal bool <TryFindLatestPackageById>b__26_0(PackageReference r) => 
                string.IsNullOrEmpty(r.Version.SpecialVersion);

            internal SemanticVersion <TryFindLatestPackageById>b__26_1(PackageReference r) => 
                r.Version;
        }
    }
}

