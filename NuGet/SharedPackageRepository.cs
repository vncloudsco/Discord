namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Linq;

    internal class SharedPackageRepository : LocalPackageRepository, ISharedPackageRepository, IPackageRepository
    {
        private const string StoreFilePath = "repositories.config";
        private readonly NuGet.PackageReferenceFile _packageReferenceFile;
        private readonly IFileSystem _storeFileSystem;

        public SharedPackageRepository(string path) : base(path)
        {
            this._storeFileSystem = base.FileSystem;
        }

        public SharedPackageRepository(IPackagePathResolver resolver, IFileSystem fileSystem, IFileSystem configSettingsFileSystem) : this(resolver, fileSystem, fileSystem, configSettingsFileSystem)
        {
        }

        public SharedPackageRepository(IPackagePathResolver resolver, IFileSystem fileSystem, IFileSystem storeFileSystem, IFileSystem configSettingsFileSystem) : base(resolver, fileSystem)
        {
            if (configSettingsFileSystem == null)
            {
                throw new ArgumentNullException("configSettingsFileSystem");
            }
            this._storeFileSystem = storeFileSystem ?? fileSystem;
            this._packageReferenceFile = new NuGet.PackageReferenceFile(configSettingsFileSystem, Constants.PackageReferenceFile);
        }

        private void AddEntry(string path)
        {
            path = this.NormalizePath(path);
            XDocument storeDocument = this.GetStoreDocument(true);
            if (this.FindEntry(storeDocument, path) == null)
            {
                storeDocument.Root.Add(new XElement("repository", new XAttribute("path", path)));
                this.SaveDocument(storeDocument);
            }
        }

        public override void AddPackage(IPackage package)
        {
            base.AddPackage(package);
            if ((this._packageReferenceFile != null) && this.IsSolutionLevel(package))
            {
                this._packageReferenceFile.AddEntry(package.Id, package.Version);
            }
        }

        public void AddPackageReferenceEntry(string packageId, SemanticVersion version)
        {
            if (this._packageReferenceFile != null)
            {
                this._packageReferenceFile.AddEntry(packageId, version);
            }
        }

        protected virtual IPackageRepository CreateRepository(string path) => 
            new PackageReferenceRepository(PathUtility.GetAbsolutePath(PathUtility.EnsureTrailingSlash(base.FileSystem.Root), path), this);

        private void DeleteEntry(string path)
        {
            path = this.NormalizePath(path);
            XDocument storeDocument = this.GetStoreDocument(false);
            if (storeDocument != null)
            {
                XElement element = this.FindEntry(storeDocument, path);
                if (element != null)
                {
                    element.Remove();
                    if (!storeDocument.Root.HasElements)
                    {
                        this._storeFileSystem.DeleteFile("repositories.config");
                    }
                    else
                    {
                        this.SaveDocument(storeDocument);
                    }
                }
            }
        }

        public override bool Exists(string packageId, SemanticVersion version) => 
            (((version == null) || !Enumerable.Any<string>(from v in version.GetComparableVersionStrings() select packageId + "." + v, path => base.FileSystem.FileExists(Path.Combine(path, path + Constants.PackageExtension)) || base.FileSystem.FileExists(Path.Combine(path, path + Constants.ManifestExtension)))) ? (this.FindPackage(packageId, version) != null) : true);

        private XElement FindEntry(XDocument document, string path)
        {
            path = this.NormalizePath(path);
            return (from e in GetRepositoryElements(document)
                let entryPath = this.NormalizePath(e.GetOptionalAttributeValue("path", null))
                where path.Equals(entryPath, StringComparison.OrdinalIgnoreCase)
                select e).FirstOrDefault<XElement>();
        }

        public override IPackage FindPackage(string packageId, SemanticVersion version)
        {
            IPackage package = base.FindPackage(packageId, version);
            if (package != null)
            {
                return package;
            }
            if (version != null)
            {
                string manifestFilePath = this.GetManifestFilePath(packageId, version);
                if (base.FileSystem.FileExists(manifestFilePath))
                {
                    return new UnzippedPackage(base.FileSystem, base.PathResolver.GetPackageDirectory(packageId, version));
                }
            }
            return null;
        }

        private string GetManifestFilePath(string packageId, SemanticVersion version)
        {
            string packageDirectory = base.PathResolver.GetPackageDirectory(packageId, version);
            return Path.Combine(packageDirectory, packageDirectory + Constants.ManifestExtension);
        }

        public override IQueryable<IPackage> GetPackages() => 
            this.SearchPackages().AsQueryable<IPackage>();

        private static IEnumerable<XElement> GetRepositoryElements(XDocument document) => 
            (from e in document.Root.Elements("repository") select e);

        internal IEnumerable<string> GetRepositoryPaths()
        {
            XDocument storeDocument = this.GetStoreDocument(false);
            if (storeDocument == null)
            {
                return Enumerable.Empty<string>();
            }
            bool flag = false;
            HashSet<string> set = new HashSet<string>();
            foreach (XElement element in GetRepositoryElements(storeDocument).ToList<XElement>())
            {
                string str = this.NormalizePath(element.GetOptionalAttributeValue("path", null));
                if (string.IsNullOrEmpty(str) || (!base.FileSystem.FileExists(str) || !set.Add(str)))
                {
                    element.Remove();
                    flag = true;
                }
            }
            if (flag)
            {
                this.SaveDocument(storeDocument);
            }
            return set;
        }

        private XDocument GetStoreDocument(bool createIfNotExists = false)
        {
            XDocument document;
            try
            {
                if (this._storeFileSystem.FileExists("repositories.config"))
                {
                    Stream input = this._storeFileSystem.OpenFile("repositories.config");
                    try
                    {
                        return XmlUtility.LoadSafe(input);
                    }
                    catch (XmlException)
                    {
                    }
                    finally
                    {
                        if (input != null)
                        {
                            input.Dispose();
                        }
                    }
                }
                if (!createIfNotExists)
                {
                    document = null;
                }
                else
                {
                    object[] content = new object[] { new XElement("repositories") };
                    document = new XDocument(content);
                }
            }
            catch (Exception exception)
            {
                object[] args = new object[] { this._storeFileSystem.GetFullPath("repositories.config") };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.ErrorReadingFile, args), exception);
            }
            return document;
        }

        private bool HasProjectLevelPackageDependency(IPackage package)
        {
            IEnumerable<PackageDependency> sequence = from p in package.DependencySets select p.Dependencies;
            if (sequence.IsEmpty<PackageDependency>())
            {
                return false;
            }
            HashSet<string> solutionLevelPackages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            solutionLevelPackages.AddRange<string>(from packageReference in this.PackageReferenceFile.GetPackageReferences() select packageReference.Id);
            return Enumerable.Any<PackageDependency>(sequence, dependency => !solutionLevelPackages.Contains(dependency.Id));
        }

        public bool IsReferenced(string packageId, SemanticVersion version) => 
            Enumerable.Any<IPackageRepository>(this.LoadProjectRepositories(), r => r.Exists(packageId, version));

        private bool IsSolutionLevel(IPackage package) => 
            (!package.HasProjectContent() ? (!this.HasProjectLevelPackageDependency(package) ? !this.IsReferenced(package.Id, package.Version) : false) : false);

        public bool IsSolutionReferenced(string packageId, SemanticVersion version) => 
            ((this._packageReferenceFile != null) && this._packageReferenceFile.EntryExists(packageId, version));

        public IEnumerable<IPackageRepository> LoadProjectRepositories() => 
            Enumerable.Select<string, IPackageRepository>(this.GetRepositoryPaths(), new Func<string, IPackageRepository>(this.CreateRepository));

        private string NormalizePath(string path) => 
            (!string.IsNullOrEmpty(path) ? (!Path.IsPathRooted(path) ? path : PathUtility.GetRelativePath(PathUtility.EnsureTrailingSlash(base.FileSystem.Root), path)) : path);

        protected override IPackage OpenPackage(string path)
        {
            IPackage package;
            if (!base.FileSystem.FileExists(path))
            {
                return null;
            }
            string extension = Path.GetExtension(path);
            if (!extension.Equals(Constants.PackageExtension, StringComparison.OrdinalIgnoreCase))
            {
                return (!extension.Equals(Constants.ManifestExtension, StringComparison.OrdinalIgnoreCase) ? null : new UnzippedPackage(base.FileSystem, Path.GetDirectoryName(path)));
            }
            try
            {
                package = new SharedOptimizedZipPackage(base.FileSystem, path);
            }
            catch (FileFormatException exception)
            {
                object[] args = new object[] { path };
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, NuGetResources.ErrorReadingPackage, args), exception);
            }
            return package;
        }

        public void RegisterRepository(NuGet.PackageReferenceFile packageReferenceFile)
        {
            this.AddEntry(packageReferenceFile.FullPath);
        }

        public override void RemovePackage(IPackage package)
        {
            string manifestFilePath = this.GetManifestFilePath(package.Id, package.Version);
            if (base.FileSystem.FileExists(manifestFilePath))
            {
                base.FileSystem.DeleteFileSafe(manifestFilePath);
            }
            string packageFilePath = this.GetPackageFilePath(package);
            if (base.FileSystem.FileExists(packageFilePath))
            {
                base.FileSystem.DeleteFileSafe(packageFilePath);
            }
            base.FileSystem.DeleteDirectorySafe(base.PathResolver.GetPackageDirectory(package), true);
            if (!base.FileSystem.GetFilesSafe(string.Empty).Any<string>() && !base.FileSystem.GetDirectoriesSafe(string.Empty).Any<string>())
            {
                base.FileSystem.DeleteDirectorySafe(string.Empty, false);
            }
            if (this._packageReferenceFile != null)
            {
                this._packageReferenceFile.DeleteEntry(package.Id, package.Version);
            }
        }

        private void SaveDocument(XDocument document)
        {
            document.Root.RemoveAll();
            (from e in GetRepositoryElements(document)
                let path = e.GetOptionalAttributeValue("path", null)
                where !string.IsNullOrEmpty(path)
                orderby path.ToUpperInvariant()
                select e).ToList<XElement>().ForEach(e => document.Root.Add(e));
            this._storeFileSystem.AddFile("repositories.config", new Action<Stream>(document.Save));
        }

        [IteratorStateMachine(typeof(<SearchPackages>d__17))]
        protected IEnumerable<IPackage> SearchPackages()
        {
            IEnumerator<string> enumerator = this.FileSystem.GetDirectories("").GetEnumerator();
            while (true)
            {
                if (!enumerator.MoveNext())
                {
                    enumerator = null;
                    yield break;
                    break;
                }
                string current = enumerator.Current;
                string <partialPath>5__1 = Path.Combine(current, current);
                string path = <partialPath>5__1 + Constants.PackageExtension;
                if (this.FileSystem.FileExists(path))
                {
                    yield return new SharedOptimizedZipPackage(this.FileSystem, path);
                    yield break;
                    break;
                }
                if (this.FileSystem.FileExists(<partialPath>5__1 + Constants.ManifestExtension))
                {
                    yield return new UnzippedPackage(this.FileSystem, current);
                    yield break;
                    break;
                }
                <partialPath>5__1 = null;
                current = null;
            }
        }

        public void UnregisterRepository(NuGet.PackageReferenceFile packageReferenceFile)
        {
            this.DeleteEntry(packageReferenceFile.FullPath);
        }

        public NuGet.PackageReferenceFile PackageReferenceFile =>
            this._packageReferenceFile;

        public override bool SupportsPrereleasePackages =>
            true;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly SharedPackageRepository.<>c <>9 = new SharedPackageRepository.<>c();
            public static Func<XElement, XElement> <>9__27_0;
            public static Func<<>f__AnonymousType15<XElement, string>, XElement> <>9__28_2;
            public static Func<XElement, <>f__AnonymousType16<XElement, string>> <>9__29_0;
            public static Func<<>f__AnonymousType16<XElement, string>, bool> <>9__29_1;
            public static Func<<>f__AnonymousType16<XElement, string>, string> <>9__29_2;
            public static Func<<>f__AnonymousType16<XElement, string>, XElement> <>9__29_3;
            public static Func<PackageDependencySet, IEnumerable<PackageDependency>> <>9__33_0;
            public static Func<PackageReference, string> <>9__33_1;

            internal XElement <FindEntry>b__28_2(<>f__AnonymousType15<XElement, string> <>h__TransparentIdentifier0) => 
                <>h__TransparentIdentifier0.e;

            internal XElement <GetRepositoryElements>b__27_0(XElement e) => 
                e;

            internal IEnumerable<PackageDependency> <HasProjectLevelPackageDependency>b__33_0(PackageDependencySet p) => 
                p.Dependencies;

            internal string <HasProjectLevelPackageDependency>b__33_1(PackageReference packageReference) => 
                packageReference.Id;

            internal <>f__AnonymousType16<XElement, string> <SaveDocument>b__29_0(XElement e) => 
                new { 
                    e = e,
                    path = e.GetOptionalAttributeValue("path", null)
                };

            internal bool <SaveDocument>b__29_1(<>f__AnonymousType16<XElement, string> <>h__TransparentIdentifier0) => 
                !string.IsNullOrEmpty(<>h__TransparentIdentifier0.path);

            internal string <SaveDocument>b__29_2(<>f__AnonymousType16<XElement, string> <>h__TransparentIdentifier0) => 
                <>h__TransparentIdentifier0.path.ToUpperInvariant();

            internal XElement <SaveDocument>b__29_3(<>f__AnonymousType16<XElement, string> <>h__TransparentIdentifier0) => 
                <>h__TransparentIdentifier0.e;
        }


        private class SharedOptimizedZipPackage : OptimizedZipPackage
        {
            private readonly string _folderPath;

            public SharedOptimizedZipPackage(IFileSystem fileSystem, string packagePath) : base(fileSystem, packagePath, fileSystem)
            {
                this._folderPath = Path.GetDirectoryName(packagePath);
            }

            protected override string GetExpandedFolderPath() => 
                this._folderPath;
        }
    }
}

