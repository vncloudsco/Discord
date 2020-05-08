namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.IO.Packaging;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;

    internal class ZipPackage : LocalPackage
    {
        private const string CacheKeyFormat = "NUGET_ZIP_PACKAGE_{0}_{1}{2}";
        private const string AssembliesCacheKey = "ASSEMBLIES";
        private const string FilesCacheKey = "FILES";
        private readonly bool _enableCaching;
        private static readonly TimeSpan CacheTimeout = TimeSpan.FromSeconds(15.0);
        private static readonly string[] ExcludePaths = new string[] { "_rels", "package" };
        private readonly Func<Stream> _streamFactory;

        public ZipPackage(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            this._enableCaching = false;
            this._streamFactory = stream.ToStreamFactory();
            this.EnsureManifest();
        }

        public ZipPackage(string filePath) : this(filePath, false)
        {
        }

        internal ZipPackage(Func<Stream> streamFactory, bool enableCaching)
        {
            if (streamFactory == null)
            {
                throw new ArgumentNullException("streamFactory");
            }
            this._enableCaching = enableCaching;
            this._streamFactory = streamFactory;
            this.EnsureManifest();
        }

        private ZipPackage(string filePath, bool enableCaching)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "filePath");
            }
            this._enableCaching = enableCaching;
            this._streamFactory = () => File.OpenRead(filePath);
            this.EnsureManifest();
        }

        internal static void ClearCache(IPackage package)
        {
            ZipPackage package2 = package as ZipPackage;
            if (package2 != null)
            {
                MemoryCache.Instance.Remove(package2.GetAssembliesCacheKey());
                MemoryCache.Instance.Remove(package2.GetFilesCacheKey());
            }
        }

        private void EnsureManifest()
        {
            using (Stream stream = this._streamFactory())
            {
                Package package1 = Package.Open(stream);
                PackageRelationship relationship = package1.GetRelationshipsByType("http://schemas.microsoft.com/packaging/2010/07/manifest").SingleOrDefault<PackageRelationship>();
                if (relationship == null)
                {
                    throw new InvalidOperationException(NuGetResources.PackageDoesNotContainManifest);
                }
                PackagePart part = package1.GetPart(relationship.TargetUri);
                if (part == null)
                {
                    throw new InvalidOperationException(NuGetResources.PackageDoesNotContainManifest);
                }
                using (Stream stream2 = part.GetStream())
                {
                    base.ReadManifest(stream2);
                }
            }
        }

        private string GetAssembliesCacheKey()
        {
            object[] args = new object[] { "ASSEMBLIES", base.Id, base.Version };
            return string.Format(CultureInfo.InvariantCulture, "NUGET_ZIP_PACKAGE_{0}_{1}{2}", args);
        }

        private List<IPackageAssemblyReference> GetAssembliesNoCache() => 
            (from file in base.GetFiles()
                where IsAssemblyReference(file.Path)
                select new ZipPackageAssemblyReference(file)).ToList<IPackageAssemblyReference>();

        protected override IEnumerable<IPackageAssemblyReference> GetAssemblyReferencesCore() => 
            (!this._enableCaching ? this.GetAssembliesNoCache() : ((IEnumerable<IPackageAssemblyReference>) MemoryCache.Instance.GetOrAdd<List<IPackageAssemblyReference>>(this.GetAssembliesCacheKey(), new Func<List<IPackageAssemblyReference>>(this.GetAssembliesNoCache), CacheTimeout, false)));

        protected override IEnumerable<IPackageFile> GetFilesBase() => 
            (!this._enableCaching ? this.GetFilesNoCache() : ((IEnumerable<IPackageFile>) MemoryCache.Instance.GetOrAdd<List<IPackageFile>>(this.GetFilesCacheKey(), new Func<List<IPackageFile>>(this.GetFilesNoCache), CacheTimeout, false)));

        private string GetFilesCacheKey()
        {
            object[] args = new object[] { "FILES", base.Id, base.Version };
            return string.Format(CultureInfo.InvariantCulture, "NUGET_ZIP_PACKAGE_{0}_{1}{2}", args);
        }

        private List<IPackageFile> GetFilesNoCache()
        {
            using (Stream stream = this._streamFactory())
            {
                return (from part in Package.Open(stream).GetParts()
                    where IsPackageFile(part)
                    select new ZipPackageFile(part)).ToList<IPackageFile>();
            }
        }

        public override Stream GetStream() => 
            this._streamFactory();

        public override IEnumerable<FrameworkName> GetSupportedFrameworks()
        {
            IEnumerable<FrameworkName> enumerable;
            IEnumerable<IPackageFile> enumerable2;
            if (this._enableCaching && MemoryCache.Instance.TryGetValue<IEnumerable<IPackageFile>>(this.GetFilesCacheKey(), out enumerable2))
            {
                enumerable = from c in enumerable2 select c.TargetFramework;
            }
            else
            {
                using (Stream stream = this._streamFactory())
                {
                    enumerable = Enumerable.Select<PackagePart, FrameworkName>(from part in Package.Open(stream).GetParts()
                        where IsPackageFile(part)
                        select part, delegate (PackagePart part) {
                        string effectivePath;
                        return VersionUtility.ParseFrameworkNameFromFilePath(UriUtility.GetPath(part.Uri), out effectivePath);
                    });
                }
            }
            return (from f in base.GetSupportedFrameworks().Concat<FrameworkName>(enumerable)
                where f != null
                select f).Distinct<FrameworkName>();
        }

        internal static bool IsPackageFile(PackagePart part)
        {
            string path = UriUtility.GetPath(part.Uri);
            string directory = Path.GetDirectoryName(path);
            return (!Enumerable.Any<string>(ExcludePaths, p => directory.StartsWith(p, StringComparison.OrdinalIgnoreCase)) && !PackageHelper.IsManifest(path));
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ZipPackage.<>c <>9 = new ZipPackage.<>c();
            public static Func<IPackageFile, FrameworkName> <>9__12_0;
            public static Func<PackagePart, bool> <>9__12_1;
            public static Func<FrameworkName, bool> <>9__12_3;
            public static Func<IPackageFile, bool> <>9__15_0;
            public static Func<IPackageFile, IPackageAssemblyReference> <>9__15_1;
            public static Func<PackagePart, bool> <>9__16_0;
            public static Func<PackagePart, IPackageFile> <>9__16_1;

            internal bool <GetAssembliesNoCache>b__15_0(IPackageFile file) => 
                LocalPackage.IsAssemblyReference(file.Path);

            internal IPackageAssemblyReference <GetAssembliesNoCache>b__15_1(IPackageFile file) => 
                new ZipPackageAssemblyReference(file);

            internal bool <GetFilesNoCache>b__16_0(PackagePart part) => 
                ZipPackage.IsPackageFile(part);

            internal IPackageFile <GetFilesNoCache>b__16_1(PackagePart part) => 
                new ZipPackageFile(part);

            internal FrameworkName <GetSupportedFrameworks>b__12_0(IPackageFile c) => 
                c.TargetFramework;

            internal bool <GetSupportedFrameworks>b__12_1(PackagePart part) => 
                ZipPackage.IsPackageFile(part);

            internal bool <GetSupportedFrameworks>b__12_3(FrameworkName f) => 
                (f != null);
        }
    }
}

