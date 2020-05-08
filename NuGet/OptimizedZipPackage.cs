namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.IO.Packaging;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;

    internal class OptimizedZipPackage : LocalPackage
    {
        private static readonly ConcurrentDictionary<PackageName, Tuple<string, DateTimeOffset>> _cachedExpandedFolder = new ConcurrentDictionary<PackageName, Tuple<string, DateTimeOffset>>();
        private static readonly IFileSystem _tempFileSystem = new PhysicalFileSystem(Path.Combine(Path.GetTempPath(), "nuget"));
        private Dictionary<string, PhysicalPackageFile> _files;
        private ICollection<FrameworkName> _supportedFrameworks;
        private readonly IFileSystem _fileSystem;
        private readonly IFileSystem _expandedFileSystem;
        private readonly string _packagePath;
        private string _expandedFolderPath;
        private readonly bool _forceUseCache;

        public OptimizedZipPackage(string fullPackagePath)
        {
            if (string.IsNullOrEmpty(fullPackagePath))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "fullPackagePath");
            }
            if (!File.Exists(fullPackagePath))
            {
                object[] args = new object[] { fullPackagePath };
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, NuGetResources.FileDoesNotExit, args), "fullPackagePath");
            }
            string directoryName = Path.GetDirectoryName(fullPackagePath);
            this._fileSystem = new PhysicalFileSystem(directoryName);
            this._packagePath = Path.GetFileName(fullPackagePath);
            this._expandedFileSystem = _tempFileSystem;
            this.EnsureManifest();
        }

        public OptimizedZipPackage(IFileSystem fileSystem, string packagePath)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException("fileSystem");
            }
            if (string.IsNullOrEmpty(packagePath))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "packagePath");
            }
            this._fileSystem = fileSystem;
            this._packagePath = packagePath;
            this._expandedFileSystem = _tempFileSystem;
            this.EnsureManifest();
        }

        public OptimizedZipPackage(IFileSystem fileSystem, string packagePath, IFileSystem expandedFileSystem)
        {
            if (fileSystem == null)
            {
                throw new ArgumentNullException("fileSystem");
            }
            if (expandedFileSystem == null)
            {
                throw new ArgumentNullException("expandedFileSystem");
            }
            if (string.IsNullOrEmpty(packagePath))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "packagePath");
            }
            this._fileSystem = fileSystem;
            this._packagePath = packagePath;
            this._expandedFileSystem = expandedFileSystem;
            this.EnsureManifest();
        }

        internal OptimizedZipPackage(IFileSystem fileSystem, string packagePath, IFileSystem expandedFileSystem, bool forceUseCache) : this(fileSystem, packagePath, expandedFileSystem)
        {
            this._forceUseCache = forceUseCache;
        }

        private void EnsureManifest()
        {
            using (Stream stream = this._fileSystem.OpenFile(this._packagePath))
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

        private void EnsurePackageFiles()
        {
            if ((this._files == null) || ((this._expandedFolderPath == null) || !this._expandedFileSystem.DirectoryExists(this._expandedFolderPath)))
            {
                this._files = new Dictionary<string, PhysicalPackageFile>();
                this._supportedFrameworks = null;
                PackageName key = new PackageName(base.Id, base.Version);
                if (!ReferenceEquals(this._expandedFileSystem, _tempFileSystem) && !this._forceUseCache)
                {
                    this._expandedFolderPath = this.GetExpandedFolderPath();
                }
                else
                {
                    Tuple<string, DateTimeOffset> tuple;
                    DateTimeOffset lastModified = this._fileSystem.GetLastModified(this._packagePath);
                    if (!_cachedExpandedFolder.TryGetValue(key, out tuple) || (tuple.Item2 < lastModified))
                    {
                        tuple = Tuple.Create<string, DateTimeOffset>(this.GetExpandedFolderPath(), lastModified);
                        _cachedExpandedFolder[key] = tuple;
                    }
                    this._expandedFolderPath = tuple.Item1;
                }
                using (Stream stream = this.GetStream())
                {
                    using (IEnumerator<PackagePart> enumerator = (from part in Package.Open(stream).GetParts()
                        where ZipPackage.IsPackageFile(part)
                        select part).GetEnumerator())
                    {
                        string path;
                        string str2;
                        goto TR_0023;
                    TR_000E:
                        PhysicalPackageFile file1 = new PhysicalPackageFile();
                        file1.SourcePath = this._expandedFileSystem.GetFullPath(str2);
                        file1.TargetPath = path;
                        this._files[path] = file1;
                    TR_0023:
                        while (true)
                        {
                            if (enumerator.MoveNext())
                            {
                                PackagePart current = enumerator.Current;
                                path = UriUtility.GetPath(current.Uri);
                                str2 = Path.Combine(this._expandedFolderPath, path);
                                bool flag = true;
                                if (this._expandedFileSystem.FileExists(str2))
                                {
                                    using (Stream stream2 = current.GetStream())
                                    {
                                        using (Stream stream3 = this._expandedFileSystem.OpenFile(str2))
                                        {
                                            flag = stream2.Length != stream3.Length;
                                        }
                                    }
                                }
                                if (flag)
                                {
                                    Stream stream4 = current.GetStream();
                                    try
                                    {
                                        using (Stream stream5 = this._expandedFileSystem.CreateFile(str2))
                                        {
                                            stream4.CopyTo(stream5);
                                        }
                                    }
                                    catch (Exception)
                                    {
                                    }
                                    finally
                                    {
                                        if (stream4 != null)
                                        {
                                            stream4.Dispose();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                return;
                            }
                            break;
                        }
                        goto TR_000E;
                    }
                }
            }
        }

        protected override IEnumerable<IPackageAssemblyReference> GetAssemblyReferencesCore()
        {
            this.EnsurePackageFiles();
            return (from file in this._files.Values
                where IsAssemblyReference(file.Path)
                select new PhysicalPackageAssemblyReference(file));
        }

        protected virtual string GetExpandedFolderPath() => 
            Path.GetRandomFileName();

        protected override IEnumerable<IPackageFile> GetFilesBase()
        {
            this.EnsurePackageFiles();
            return (IEnumerable<IPackageFile>) this._files.Values;
        }

        public override Stream GetStream() => 
            this._fileSystem.OpenFile(this._packagePath);

        public override IEnumerable<FrameworkName> GetSupportedFrameworks()
        {
            this.EnsurePackageFiles();
            if (this._supportedFrameworks == null)
            {
                IEnumerable<FrameworkName> second = from c in this._files.Values select c.TargetFramework;
                IEnumerable<FrameworkName> source = (from f in base.GetSupportedFrameworks().Concat<FrameworkName>(second)
                    where f != null
                    select f).Distinct<FrameworkName>();
                this._supportedFrameworks = new ReadOnlyCollection<FrameworkName>(source.ToList<FrameworkName>());
            }
            return this._supportedFrameworks;
        }

        public static void PurgeCache()
        {
            ConcurrentDictionary<PackageName, Tuple<string, DateTimeOffset>> dictionary = _cachedExpandedFolder;
            lock (dictionary)
            {
                if (_cachedExpandedFolder.Count > 0)
                {
                    foreach (Tuple<string, DateTimeOffset> tuple in _cachedExpandedFolder.Values)
                    {
                        try
                        {
                            string path = tuple.Item1;
                            _tempFileSystem.DeleteDirectory(path, true);
                        }
                        catch (Exception)
                        {
                        }
                    }
                    _cachedExpandedFolder.Clear();
                }
            }
        }

        public bool IsValid =>
            this._fileSystem.FileExists(this._packagePath);

        protected IFileSystem FileSystem =>
            this._fileSystem;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly OptimizedZipPackage.<>c <>9 = new OptimizedZipPackage.<>c();
            public static Func<PhysicalPackageFile, bool> <>9__19_0;
            public static Func<PhysicalPackageFile, IPackageAssemblyReference> <>9__19_1;
            public static Func<PhysicalPackageFile, FrameworkName> <>9__20_0;
            public static Func<FrameworkName, bool> <>9__20_1;
            public static Func<PackagePart, bool> <>9__22_0;

            internal bool <EnsurePackageFiles>b__22_0(PackagePart part) => 
                ZipPackage.IsPackageFile(part);

            internal bool <GetAssemblyReferencesCore>b__19_0(PhysicalPackageFile file) => 
                LocalPackage.IsAssemblyReference(file.Path);

            internal IPackageAssemblyReference <GetAssemblyReferencesCore>b__19_1(PhysicalPackageFile file) => 
                new PhysicalPackageAssemblyReference(file);

            internal FrameworkName <GetSupportedFrameworks>b__20_0(PhysicalPackageFile c) => 
                c.TargetFramework;

            internal bool <GetSupportedFrameworks>b__20_1(FrameworkName f) => 
                (f != null);
        }
    }
}

