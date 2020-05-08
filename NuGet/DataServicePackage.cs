namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Common;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;

    [EntityPropertyMapping("Authors", 2, 0, false), EntityPropertyMapping("LastUpdated", 7, 0, false), DataServiceKey(new string[] { "Id", "Version" }), CLSCompliant(false), EntityPropertyMapping("Summary", 10, 0, false), EntityPropertyMapping("Id", 11, 0, false)]
    internal class DataServicePackage : IPackage, IPackageMetadata, IPackageName, IServerPackageMetadata
    {
        private IHashProvider _hashProvider;
        private bool _usingMachineCache;
        private string _licenseNames;
        internal IPackage _package;

        internal void EnsurePackage(IPackageCacheRepository cacheRepository)
        {
            IPackageMetadata packageMetadata = this;
            if ((((this._package == null) || ((this._package is OptimizedZipPackage) && !((OptimizedZipPackage) this._package).IsValid)) || !string.Equals(this.OldHash, this.PackageHash, StringComparison.OrdinalIgnoreCase)) || (this._usingMachineCache && !cacheRepository.Exists(this.Id, packageMetadata.Version)))
            {
                IPackage package = null;
                bool flag = false;
                bool flag2 = false;
                if (TryGetPackage(cacheRepository, packageMetadata, out package) && this.MatchPackageHash(package))
                {
                    flag2 = true;
                }
                else
                {
                    if (cacheRepository.InvokeOnPackage(packageMetadata.Id, packageMetadata.Version, stream => this.Downloader.DownloadPackage(this.DownloadUrl, this, stream)))
                    {
                        package = cacheRepository.FindPackage(packageMetadata.Id, packageMetadata.Version);
                    }
                    else
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            this.Downloader.DownloadPackage(this.DownloadUrl, this, stream);
                            stream.Seek(0L, SeekOrigin.Begin);
                            package = new ZipPackage(stream);
                        }
                        flag = true;
                    }
                    flag2 = true;
                }
                if (!flag2)
                {
                    object[] args = new object[] { this.Version, this.Id };
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.Error_InvalidPackage, args));
                }
                this._package = package;
                this.Id = this._package.Id;
                this.Version = this._package.Version.ToString();
                this._usingMachineCache = !flag;
                this.OldHash = this.PackageHash;
            }
        }

        public IEnumerable<IPackageFile> GetFiles() => 
            this.Package.GetFiles();

        public Stream GetStream() => 
            this.Package.GetStream();

        public virtual IEnumerable<FrameworkName> GetSupportedFrameworks() => 
            this.Package.GetSupportedFrameworks();

        private bool MatchPackageHash(IPackage package) => 
            ((package != null) && package.GetHash(this.HashProvider).Equals(this.PackageHash, StringComparison.OrdinalIgnoreCase));

        private static Tuple<string, IVersionSpec, FrameworkName> ParseDependency(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }
            char[] separator = new char[] { ':' };
            string[] strArray = value.Trim().Split(separator);
            if (strArray.Length == 0)
            {
                return null;
            }
            IVersionSpec result = null;
            if (strArray.Length > 1)
            {
                VersionUtility.TryParseVersionSpec(strArray[1], out result);
            }
            return Tuple.Create<string, IVersionSpec, FrameworkName>(strArray[0].Trim(), result, ((strArray.Length <= 2) || string.IsNullOrEmpty(strArray[2])) ? null : VersionUtility.ParseFrameworkName(strArray[2]));
        }

        private static List<PackageDependencySet> ParseDependencySet(string value)
        {
            List<PackageDependencySet> list = new List<PackageDependencySet>();
            char[] separator = new char[] { '|' };
            list.AddRange(from d in Enumerable.Select<string, Tuple<string, IVersionSpec, FrameworkName>>(value.Split(separator), new Func<string, Tuple<string, IVersionSpec, FrameworkName>>(DataServicePackage.ParseDependency)).ToList<Tuple<string, IVersionSpec, FrameworkName>>()
                group d by d.Item3 into g
                select new PackageDependencySet(g.Key, from pair in g
                    where !string.IsNullOrEmpty(pair.Item1)
                    select new PackageDependency(pair.Item1, pair.Item2)));
            return list;
        }

        public override string ToString() => 
            this.GetFullName();

        private static bool TryGetPackage(IPackageRepository repository, IPackageMetadata packageMetadata, out IPackage package)
        {
            try
            {
                package = repository.FindPackage(packageMetadata.Id, packageMetadata.Version);
            }
            catch
            {
                package = null;
            }
            return (package != null);
        }

        public string Id { get; set; }

        public string Version { get; set; }

        public string Title { get; set; }

        public string Authors { get; set; }

        public string Owners { get; set; }

        public Uri IconUrl { get; set; }

        public Uri LicenseUrl { get; set; }

        public Uri ProjectUrl { get; set; }

        public Uri ReportAbuseUrl { get; set; }

        public Uri GalleryDetailsUrl { get; set; }

        public string LicenseNames
        {
            get => 
                this._licenseNames;
            set
            {
                string[] textArray1;
                this._licenseNames = value;
                if (string.IsNullOrEmpty(value))
                {
                    textArray1 = new string[0];
                }
                else
                {
                    char[] separator = new char[] { ';' };
                    textArray1 = value.Split(separator).ToArray<string>();
                }
                this.LicenseNameCollection = textArray1;
            }
        }

        public ICollection<string> LicenseNameCollection { get; private set; }

        public Uri LicenseReportUrl { get; set; }

        public Uri DownloadUrl { get; set; }

        public bool Listed { get; set; }

        public DateTimeOffset? Published { get; set; }

        public DateTimeOffset LastUpdated { get; set; }

        public int DownloadCount { get; set; }

        public bool RequireLicenseAcceptance { get; set; }

        public bool DevelopmentDependency { get; set; }

        public string Description { get; set; }

        public string Summary { get; set; }

        public string ReleaseNotes { get; set; }

        public string Language { get; set; }

        public string Tags { get; set; }

        public string Dependencies { get; set; }

        public string PackageHash { get; set; }

        public string PackageHashAlgorithm { get; set; }

        public bool IsLatestVersion { get; set; }

        public bool IsAbsoluteLatestVersion { get; set; }

        public string Copyright { get; set; }

        public string MinClientVersion { get; set; }

        private string OldHash { get; set; }

        private IPackage Package
        {
            get
            {
                this.EnsurePackage(MachineCache.Default);
                return this._package;
            }
        }

        internal PackageDownloader Downloader { get; set; }

        internal IHashProvider HashProvider
        {
            get => 
                (this._hashProvider ?? new CryptoHashProvider(this.PackageHashAlgorithm));
            set => 
                (this._hashProvider = value);
        }

        bool IPackage.Listed =>
            this.Listed;

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

        public IEnumerable<PackageDependencySet> DependencySets =>
            (!string.IsNullOrEmpty(this.Dependencies) ? ParseDependencySet(this.Dependencies) : Enumerable.Empty<PackageDependencySet>());

        public ICollection<PackageReferenceSet> PackageAssemblyReferences =>
            this.Package.PackageAssemblyReferences;

        SemanticVersion IPackageName.Version =>
            ((this.Version == null) ? null : new SemanticVersion(this.Version));

        System.Version IPackageMetadata.MinClientVersion =>
            (string.IsNullOrEmpty(this.MinClientVersion) ? null : new System.Version(this.MinClientVersion));

        public IEnumerable<IPackageAssemblyReference> AssemblyReferences =>
            this.Package.AssemblyReferences;

        public IEnumerable<FrameworkAssemblyReference> FrameworkAssemblies =>
            this.Package.FrameworkAssemblies;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly DataServicePackage.<>c <>9 = new DataServicePackage.<>c();
            public static Func<Tuple<string, IVersionSpec, FrameworkName>, FrameworkName> <>9__168_0;
            public static Func<Tuple<string, IVersionSpec, FrameworkName>, bool> <>9__168_2;
            public static Func<Tuple<string, IVersionSpec, FrameworkName>, PackageDependency> <>9__168_3;
            public static Func<IGrouping<FrameworkName, Tuple<string, IVersionSpec, FrameworkName>>, PackageDependencySet> <>9__168_1;

            internal FrameworkName <ParseDependencySet>b__168_0(Tuple<string, IVersionSpec, FrameworkName> d) => 
                d.Item3;

            internal PackageDependencySet <ParseDependencySet>b__168_1(IGrouping<FrameworkName, Tuple<string, IVersionSpec, FrameworkName>> g) => 
                new PackageDependencySet(g.Key, from pair in g
                    where !string.IsNullOrEmpty(pair.Item1)
                    select new PackageDependency(pair.Item1, pair.Item2));

            internal bool <ParseDependencySet>b__168_2(Tuple<string, IVersionSpec, FrameworkName> pair) => 
                !string.IsNullOrEmpty(pair.Item1);

            internal PackageDependency <ParseDependencySet>b__168_3(Tuple<string, IVersionSpec, FrameworkName> pair) => 
                new PackageDependency(pair.Item1, pair.Item2);
        }
    }
}

