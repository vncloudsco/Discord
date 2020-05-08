namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;

    internal class LocalPackageRepository : PackageRepositoryBase, IPackageLookup, IPackageRepository
    {
        private readonly ConcurrentDictionary<string, PackageCacheEntry> _packageCache;
        private readonly ConcurrentDictionary<PackageName, string> _packagePathLookup;
        private readonly bool _enableCaching;

        public LocalPackageRepository(string physicalPath) : this(physicalPath, true)
        {
        }

        public LocalPackageRepository(IPackagePathResolver pathResolver, IFileSystem fileSystem) : this(pathResolver, fileSystem, true)
        {
        }

        public LocalPackageRepository(string physicalPath, bool enableCaching) : this(new DefaultPackagePathResolver(physicalPath), new PhysicalFileSystem(physicalPath), enableCaching)
        {
        }

        public LocalPackageRepository(IPackagePathResolver pathResolver, IFileSystem fileSystem, bool enableCaching)
        {
            this._packageCache = new ConcurrentDictionary<string, PackageCacheEntry>(StringComparer.OrdinalIgnoreCase);
            this._packagePathLookup = new ConcurrentDictionary<PackageName, string>();
            if (pathResolver == null)
            {
                throw new ArgumentNullException("pathResolver");
            }
            if (fileSystem == null)
            {
                throw new ArgumentNullException("fileSystem");
            }
            this.FileSystem = fileSystem;
            this.PathResolver = pathResolver;
            this._enableCaching = enableCaching;
        }

        public override void AddPackage(IPackage package)
        {
            if (base.PackageSaveMode.HasFlag(PackageSaveModes.Nuspec))
            {
                string manifestFilePath = this.GetManifestFilePath(package.Id, package.Version);
                Manifest manifest = Manifest.Create(package);
                manifest.Metadata.ReferenceSets = Enumerable.Select<IGrouping<FrameworkName, IPackageAssemblyReference>, ManifestReferenceSet>(from f in package.AssemblyReferences group f by f.TargetFramework, delegate (IGrouping<FrameworkName, IPackageAssemblyReference> g) {
                    ManifestReferenceSet set1 = new ManifestReferenceSet();
                    ManifestReferenceSet set2 = new ManifestReferenceSet();
                    set2.TargetFramework = (g.Key == null) ? null : VersionUtility.GetFrameworkString(g.Key);
                    ManifestReferenceSet local2 = set2;
                    ManifestReferenceSet local3 = set2;
                    local3.References = Enumerable.Select<IPackageAssemblyReference, ManifestReference>(g, delegate (IPackageAssemblyReference p) {
                        ManifestReference reference1 = new ManifestReference();
                        reference1.File = p.Name;
                        return reference1;
                    }).ToList<ManifestReference>();
                    return local3;
                }).ToList<ManifestReferenceSet>();
                this.FileSystem.AddFileWithCheck(manifestFilePath, new Action<Stream>(manifest.Save));
            }
            if (base.PackageSaveMode.HasFlag(PackageSaveModes.Nupkg))
            {
                string packageFilePath = this.GetPackageFilePath(package);
                this.FileSystem.AddFileWithCheck(packageFilePath, new Func<Stream>(package.GetStream));
            }
        }

        public virtual bool Exists(string packageId, SemanticVersion version) => 
            (this.FindPackage(packageId, version) != null);

        private static bool FileNameMatchesPattern(string packageId, SemanticVersion version, string path)
        {
            SemanticVersion version2;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
            return ((fileNameWithoutExtension.Length > packageId.Length) && (SemanticVersion.TryParse(fileNameWithoutExtension.Substring(packageId.Length + 1), out version2) && (version2 == version)));
        }

        public virtual IPackage FindPackage(string packageId, SemanticVersion version)
        {
            if (string.IsNullOrEmpty(packageId))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "packageId");
            }
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            return this.FindPackage(new Func<string, IPackage>(this.OpenPackage), packageId, version);
        }

        internal IPackage FindPackage(Func<string, IPackage> openPackage, string packageId, SemanticVersion version)
        {
            string str;
            PackageName lookupPackageName = new PackageName(packageId, version);
            return ((!this._enableCaching || (!this._packagePathLookup.TryGetValue(lookupPackageName, out str) || !this.FileSystem.FileExists(str))) ? (from path in this.GetPackageLookupPaths(packageId, version)
                let package = this.GetPackage(openPackage, path)
                where lookupPackageName.Equals(new PackageName(package.Id, package.Version))
                select package).FirstOrDefault<IPackage>() : this.GetPackage(openPackage, str));
        }

        public virtual IEnumerable<IPackage> FindPackagesById(string packageId)
        {
            if (string.IsNullOrEmpty(packageId))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "packageId");
            }
            return this.FindPackagesById(new Func<string, IPackage>(this.OpenPackage), packageId);
        }

        internal IEnumerable<IPackage> FindPackagesById(Func<string, IPackage> openPackage, string packageId)
        {
            HashSet<IPackage> collection = new HashSet<IPackage>((IEqualityComparer<IPackage>) PackageEqualityComparer.IdAndVersion);
            collection.AddRange<IPackage>(this.GetPackages(openPackage, packageId, this.GetPackageFiles(packageId + "*" + Constants.PackageExtension)));
            collection.AddRange<IPackage>(this.GetPackages(openPackage, packageId, this.GetPackageFiles(packageId + "*" + Constants.ManifestExtension)));
            return collection;
        }

        private string GetManifestFilePath(string packageId, SemanticVersion version)
        {
            string packageDirectory = this.PathResolver.GetPackageDirectory(packageId, version);
            return Path.Combine(packageDirectory, packageDirectory + Constants.ManifestExtension);
        }

        private IPackage GetPackage(Func<string, IPackage> openPackage, string path)
        {
            PackageCacheEntry entry;
            DateTimeOffset lastModified = this.FileSystem.GetLastModified(path);
            if (!this._packageCache.TryGetValue(path, out entry) || ((entry != null) && (lastModified > entry.LastModifiedTime)))
            {
                string arg = path;
                IPackage package = openPackage(arg);
                entry = new PackageCacheEntry(package, lastModified);
                if (this._enableCaching)
                {
                    this._packageCache[arg] = entry;
                    this._packagePathLookup.GetOrAdd(new PackageName(package.Id, package.Version), path);
                }
            }
            return entry.Package;
        }

        protected virtual string GetPackageFilePath(IPackage package) => 
            Path.Combine(this.PathResolver.GetPackageDirectory(package), this.PathResolver.GetPackageFileName(package));

        protected virtual string GetPackageFilePath(string id, SemanticVersion version) => 
            Path.Combine(this.PathResolver.GetPackageDirectory(id, version), this.PathResolver.GetPackageFileName(id, version));

        [IteratorStateMachine(typeof(<GetPackageFiles>d__31))]
        internal IEnumerable<string> GetPackageFiles(string filter = null)
        {
            <GetPackageFiles>d__31 d__1 = new <GetPackageFiles>d__31(-2);
            d__1.<>4__this = this;
            d__1.<>3__filter = filter;
            return d__1;
        }

        public virtual IEnumerable<string> GetPackageLookupPaths(string packageId, SemanticVersion version)
        {
            string text1;
            string packageFileName = this.PathResolver.GetPackageFileName(packageId, version);
            string filter = Path.ChangeExtension(packageFileName, Constants.ManifestExtension);
            IEnumerable<string> first = this.GetPackageFiles(packageFileName).Concat<string>(this.GetPackageFiles(filter));
            if ((version == null) || (version.Version.Revision >= 1))
            {
                return first;
            }
            if (version.Version.Build < 1)
            {
                object[] values = new object[] { packageId, version.Version.Major, version.Version.Minor };
                text1 = string.Join(".", values);
            }
            else
            {
                object[] values = new object[] { packageId, version.Version.Major, version.Version.Minor, version.Version.Build };
                text1 = string.Join(".", values);
            }
            string str3 = text1;
            string str4 = str3 + "*" + Constants.ManifestExtension;
            str3 = str3 + "*" + Constants.PackageExtension;
            IEnumerable<string> second = from path in this.GetPackageFiles(str4)
                where FileNameMatchesPattern(packageId, version, path)
                select path;
            return first.Concat<string>((from path in this.GetPackageFiles(str3)
                where FileNameMatchesPattern(packageId, version, path)
                select path)).Concat<string>(second);
        }

        public override IQueryable<IPackage> GetPackages() => 
            this.GetPackages(new Func<string, IPackage>(this.OpenPackage)).AsQueryable<IPackage>();

        internal IEnumerable<IPackage> GetPackages(Func<string, IPackage> openPackage) => 
            (from path in this.GetPackageFiles(null) select this.GetPackage(openPackage, path));

        [IteratorStateMachine(typeof(<GetPackages>d__28))]
        internal IEnumerable<IPackage> GetPackages(Func<string, IPackage> openPackage, string packageId, IEnumerable<string> packagePaths)
        {
            <GetPackages>d__28 d__1 = new <GetPackages>d__28(-2);
            d__1.<>4__this = this;
            d__1.<>3__openPackage = openPackage;
            d__1.<>3__packageId = packageId;
            d__1.<>3__packagePaths = packagePaths;
            return d__1;
        }

        protected virtual IPackage OpenPackage(string path)
        {
            OptimizedZipPackage package;
            if (!this.FileSystem.FileExists(path))
            {
                return null;
            }
            if (Path.GetExtension(path) != Constants.PackageExtension)
            {
                return (((Path.GetExtension(path) != Constants.ManifestExtension) || !this.FileSystem.FileExists(path)) ? null : new UnzippedPackage(this.FileSystem, Path.GetFileNameWithoutExtension(path)));
            }
            try
            {
                package = new OptimizedZipPackage(this.FileSystem, path);
            }
            catch (FileFormatException exception)
            {
                object[] args = new object[] { path };
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, NuGetResources.ErrorReadingPackage, args), exception);
            }
            package.Published = new DateTimeOffset?(this.FileSystem.GetLastModified(path));
            return package;
        }

        public override void RemovePackage(IPackage package)
        {
            string manifestFilePath = this.GetManifestFilePath(package.Id, package.Version);
            if (this.FileSystem.FileExists(manifestFilePath))
            {
                this.FileSystem.DeleteFileSafe(manifestFilePath);
            }
            string packageFilePath = this.GetPackageFilePath(package);
            this.FileSystem.DeleteFileSafe(packageFilePath);
            this.FileSystem.DeleteDirectorySafe(this.PathResolver.GetPackageDirectory(package), false);
            if (!this.FileSystem.GetFilesSafe(string.Empty).Any<string>() && !this.FileSystem.GetDirectoriesSafe(string.Empty).Any<string>())
            {
                this.FileSystem.DeleteDirectorySafe(string.Empty, false);
            }
        }

        public override string Source =>
            this.FileSystem.Root;

        public IPackagePathResolver PathResolver { get; set; }

        public override bool SupportsPrereleasePackages =>
            true;

        protected IFileSystem FileSystem { get; private set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly LocalPackageRepository.<>c <>9 = new LocalPackageRepository.<>c();
            public static Func<IPackageAssemblyReference, FrameworkName> <>9__20_0;
            public static Func<IPackageAssemblyReference, ManifestReference> <>9__20_2;
            public static Func<IGrouping<FrameworkName, IPackageAssemblyReference>, ManifestReferenceSet> <>9__20_1;
            public static Func<<>f__AnonymousType30<string, IPackage>, IPackage> <>9__26_2;

            internal FrameworkName <AddPackage>b__20_0(IPackageAssemblyReference f) => 
                f.TargetFramework;

            internal ManifestReferenceSet <AddPackage>b__20_1(IGrouping<FrameworkName, IPackageAssemblyReference> g)
            {
                ManifestReferenceSet set1 = new ManifestReferenceSet();
                ManifestReferenceSet set2 = new ManifestReferenceSet();
                set2.TargetFramework = (g.Key == null) ? null : VersionUtility.GetFrameworkString(g.Key);
                ManifestReferenceSet local2 = set2;
                ManifestReferenceSet local3 = set2;
                local3.References = Enumerable.Select<IPackageAssemblyReference, ManifestReference>(g, delegate (IPackageAssemblyReference p) {
                    ManifestReference reference1 = new ManifestReference();
                    reference1.File = p.Name;
                    return reference1;
                }).ToList<ManifestReference>();
                return local3;
            }

            internal ManifestReference <AddPackage>b__20_2(IPackageAssemblyReference p)
            {
                ManifestReference reference1 = new ManifestReference();
                reference1.File = p.Name;
                return reference1;
            }

            internal IPackage <FindPackage>b__26_2(<>f__AnonymousType30<string, IPackage> <>h__TransparentIdentifier0) => 
                <>h__TransparentIdentifier0.package;
        }

        [CompilerGenerated]
        private sealed class <GetPackageFiles>d__31 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private string <>2__current;
            private int <>l__initialThreadId;
            private string filter;
            public string <>3__filter;
            public LocalPackageRepository <>4__this;
            private IEnumerator<string> <>7__wrap1;
            private IEnumerator<string> <>7__wrap2;

            [DebuggerHidden]
            public <GetPackageFiles>d__31(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap1 != null)
                {
                    this.<>7__wrap1.Dispose();
                }
            }

            private void <>m__Finally2()
            {
                this.<>1__state = -3;
                if (this.<>7__wrap2 != null)
                {
                    this.<>7__wrap2.Dispose();
                }
            }

            private void <>m__Finally3()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap1 != null)
                {
                    this.<>7__wrap1.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.filter = this.filter ?? ("*" + Constants.PackageExtension);
                            this.<>7__wrap1 = this.<>4__this.FileSystem.GetDirectories(string.Empty).GetEnumerator();
                            this.<>1__state = -3;
                            goto TR_000C;

                        case 1:
                            this.<>1__state = -4;
                            goto TR_0009;

                        case 2:
                            this.<>1__state = -5;
                            goto TR_0005;

                        default:
                            flag = false;
                            break;
                    }
                    return flag;
                TR_0005:
                    if (!this.<>7__wrap1.MoveNext())
                    {
                        this.<>m__Finally3();
                        this.<>7__wrap1 = null;
                        flag = false;
                    }
                    else
                    {
                        string current = this.<>7__wrap1.Current;
                        this.<>2__current = current;
                        this.<>1__state = 2;
                        flag = true;
                    }
                    return flag;
                TR_0009:
                    if (this.<>7__wrap2.MoveNext())
                    {
                        string current = this.<>7__wrap2.Current;
                        this.<>2__current = current;
                        this.<>1__state = 1;
                        flag = true;
                    }
                    else
                    {
                        this.<>m__Finally2();
                        this.<>7__wrap2 = null;
                        goto TR_000C;
                    }
                    return flag;
                TR_000C:
                    while (true)
                    {
                        if (this.<>7__wrap1.MoveNext())
                        {
                            string current = this.<>7__wrap1.Current;
                            this.<>7__wrap2 = this.<>4__this.FileSystem.GetFiles(current, this.filter).GetEnumerator();
                            this.<>1__state = -4;
                        }
                        else
                        {
                            this.<>m__Finally1();
                            this.<>7__wrap1 = null;
                            this.<>7__wrap1 = this.<>4__this.FileSystem.GetFiles(string.Empty, this.filter).GetEnumerator();
                            this.<>1__state = -5;
                            goto TR_0005;
                        }
                        break;
                    }
                    goto TR_0009;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<string> IEnumerable<string>.GetEnumerator()
            {
                LocalPackageRepository.<GetPackageFiles>d__31 d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new LocalPackageRepository.<GetPackageFiles>d__31(0) {
                        <>4__this = this.<>4__this
                    };
                }
                d__.filter = this.<>3__filter;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<System.String>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                int num = this.<>1__state;
                switch (num)
                {
                    case -5:
                    case 2:
                        try
                        {
                        }
                        finally
                        {
                            this.<>m__Finally3();
                        }
                        break;

                    case -4:
                    case -3:
                    case 1:
                        try
                        {
                            if ((num == -4) || (num == 1))
                            {
                                try
                                {
                                }
                                finally
                                {
                                    this.<>m__Finally2();
                                }
                            }
                        }
                        finally
                        {
                            this.<>m__Finally1();
                        }
                        break;

                    case -2:
                    case -1:
                    case 0:
                        break;

                    default:
                        return;
                }
            }

            string IEnumerator<string>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }

        [CompilerGenerated]
        private sealed class <GetPackages>d__28 : IEnumerable<IPackage>, IEnumerable, IEnumerator<IPackage>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private IPackage <>2__current;
            private int <>l__initialThreadId;
            private IEnumerable<string> packagePaths;
            public IEnumerable<string> <>3__packagePaths;
            public LocalPackageRepository <>4__this;
            private Func<string, IPackage> openPackage;
            public Func<string, IPackage> <>3__openPackage;
            private string packageId;
            public string <>3__packageId;
            private IEnumerator<string> <>7__wrap1;

            [DebuggerHidden]
            public <GetPackages>d__28(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap1 != null)
                {
                    this.<>7__wrap1.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    int num = this.<>1__state;
                    if (num == 0)
                    {
                        this.<>1__state = -1;
                        this.<>7__wrap1 = this.packagePaths.GetEnumerator();
                        this.<>1__state = -3;
                    }
                    else if (num == 1)
                    {
                        this.<>1__state = -3;
                    }
                    else
                    {
                        return false;
                    }
                    while (true)
                    {
                        if (!this.<>7__wrap1.MoveNext())
                        {
                            this.<>m__Finally1();
                            this.<>7__wrap1 = null;
                            flag = false;
                        }
                        else
                        {
                            string current = this.<>7__wrap1.Current;
                            IPackage package = null;
                            try
                            {
                                package = this.<>4__this.GetPackage(this.openPackage, current);
                            }
                            catch (InvalidOperationException)
                            {
                                if (!string.Equals(Constants.ManifestExtension, Path.GetExtension(current), StringComparison.OrdinalIgnoreCase))
                                {
                                    throw;
                                }
                            }
                            if ((package == null) || !package.Id.Equals(this.packageId, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                            this.<>2__current = package;
                            this.<>1__state = 1;
                            flag = true;
                        }
                        break;
                    }
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<IPackage> IEnumerable<IPackage>.GetEnumerator()
            {
                LocalPackageRepository.<GetPackages>d__28 d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new LocalPackageRepository.<GetPackages>d__28(0) {
                        <>4__this = this.<>4__this
                    };
                }
                d__.openPackage = this.<>3__openPackage;
                d__.packageId = this.<>3__packageId;
                d__.packagePaths = this.<>3__packagePaths;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<NuGet.IPackage>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                int num = this.<>1__state;
                if ((num == -3) || (num == 1))
                {
                    try
                    {
                    }
                    finally
                    {
                        this.<>m__Finally1();
                    }
                }
            }

            IPackage IEnumerator<IPackage>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }

        private class PackageCacheEntry
        {
            public PackageCacheEntry(IPackage package, DateTimeOffset lastModifiedTime)
            {
                this.Package = package;
                this.LastModifiedTime = lastModifiedTime;
            }

            public IPackage Package { get; private set; }

            public DateTimeOffset LastModifiedTime { get; private set; }
        }
    }
}

