namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class PackageManager : IPackageManager
    {
        private ILogger _logger;
        [CompilerGenerated]
        private EventHandler<PackageOperationEventArgs> PackageInstalling;
        [CompilerGenerated]
        private EventHandler<PackageOperationEventArgs> PackageInstalled;
        [CompilerGenerated]
        private EventHandler<PackageOperationEventArgs> PackageUninstalling;
        [CompilerGenerated]
        private EventHandler<PackageOperationEventArgs> PackageUninstalled;
        private bool _bindingRedirectEnabled;

        public event EventHandler<PackageOperationEventArgs> PackageInstalled
        {
            [CompilerGenerated] add
            {
                EventHandler<PackageOperationEventArgs> packageInstalled = this.PackageInstalled;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> a = packageInstalled;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Combine(a, value);
                    packageInstalled = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageInstalled, handler3, a);
                    if (ReferenceEquals(packageInstalled, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<PackageOperationEventArgs> packageInstalled = this.PackageInstalled;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> source = packageInstalled;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Remove(source, value);
                    packageInstalled = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageInstalled, handler3, source);
                    if (ReferenceEquals(packageInstalled, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EventHandler<PackageOperationEventArgs> PackageInstalling
        {
            [CompilerGenerated] add
            {
                EventHandler<PackageOperationEventArgs> packageInstalling = this.PackageInstalling;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> a = packageInstalling;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Combine(a, value);
                    packageInstalling = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageInstalling, handler3, a);
                    if (ReferenceEquals(packageInstalling, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<PackageOperationEventArgs> packageInstalling = this.PackageInstalling;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> source = packageInstalling;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Remove(source, value);
                    packageInstalling = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageInstalling, handler3, source);
                    if (ReferenceEquals(packageInstalling, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EventHandler<PackageOperationEventArgs> PackageUninstalled
        {
            [CompilerGenerated] add
            {
                EventHandler<PackageOperationEventArgs> packageUninstalled = this.PackageUninstalled;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> a = packageUninstalled;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Combine(a, value);
                    packageUninstalled = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageUninstalled, handler3, a);
                    if (ReferenceEquals(packageUninstalled, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<PackageOperationEventArgs> packageUninstalled = this.PackageUninstalled;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> source = packageUninstalled;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Remove(source, value);
                    packageUninstalled = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageUninstalled, handler3, source);
                    if (ReferenceEquals(packageUninstalled, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EventHandler<PackageOperationEventArgs> PackageUninstalling
        {
            [CompilerGenerated] add
            {
                EventHandler<PackageOperationEventArgs> packageUninstalling = this.PackageUninstalling;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> a = packageUninstalling;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Combine(a, value);
                    packageUninstalling = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageUninstalling, handler3, a);
                    if (ReferenceEquals(packageUninstalling, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<PackageOperationEventArgs> packageUninstalling = this.PackageUninstalling;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> source = packageUninstalling;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Remove(source, value);
                    packageUninstalling = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageUninstalling, handler3, source);
                    if (ReferenceEquals(packageUninstalling, source))
                    {
                        return;
                    }
                }
            }
        }

        public PackageManager(IPackageRepository sourceRepository, string path) : this(sourceRepository, new DefaultPackagePathResolver(path), new PhysicalFileSystem(path))
        {
        }

        public PackageManager(IPackageRepository sourceRepository, IPackagePathResolver pathResolver, IFileSystem fileSystem) : this(sourceRepository, pathResolver, fileSystem, new SharedPackageRepository(pathResolver, fileSystem, fileSystem))
        {
        }

        public PackageManager(IPackageRepository sourceRepository, IPackagePathResolver pathResolver, IFileSystem fileSystem, ISharedPackageRepository localRepository)
        {
            this._bindingRedirectEnabled = true;
            if (sourceRepository == null)
            {
                throw new ArgumentNullException("sourceRepository");
            }
            if (pathResolver == null)
            {
                throw new ArgumentNullException("pathResolver");
            }
            if (fileSystem == null)
            {
                throw new ArgumentNullException("fileSystem");
            }
            if (localRepository == null)
            {
                throw new ArgumentNullException("localRepository");
            }
            this.SourceRepository = sourceRepository;
            this.DependencyResolver = new DependencyResolverFromRepo(sourceRepository);
            this.PathResolver = pathResolver;
            this.FileSystem = fileSystem;
            this.LocalRepository = localRepository;
            this.DependencyVersion = NuGet.DependencyVersion.Lowest;
            this.CheckDowngrade = true;
        }

        public virtual void AddBindingRedirects(IProjectManager projectManager)
        {
        }

        public PackageOperationEventArgs CreateOperation(IPackage package) => 
            new PackageOperationEventArgs(package, this.FileSystem, this.PathResolver.GetInstallPath(package));

        public void Execute(PackageOperation operation)
        {
            bool flag = this.LocalRepository.Exists(operation.Package);
            if (operation.Action != PackageAction.Install)
            {
                if (flag)
                {
                    this.ExecuteUninstall(operation.Package);
                }
            }
            else if (!flag)
            {
                this.ExecuteInstall(operation.Package);
            }
            else
            {
                object[] args = new object[] { operation.Package.GetFullName() };
                this.Logger.Log(MessageLevel.Info, NuGetResources.Log_PackageAlreadyInstalled, args);
            }
        }

        protected void ExecuteInstall(IPackage package)
        {
            string fullName = package.GetFullName();
            object[] objArray1 = new object[] { fullName };
            this.Logger.Log(MessageLevel.Info, NuGetResources.Log_BeginInstallPackage, objArray1);
            PackageOperationEventArgs e = this.CreateOperation(package);
            this.OnInstalling(e);
            if (!e.Cancel)
            {
                this.OnExpandFiles(e);
                this.LocalRepository.AddPackage(package);
                object[] args = new object[] { fullName };
                this.Logger.Log(MessageLevel.Info, NuGetResources.Log_PackageInstalledSuccessfully, args);
                this.OnInstalled(e);
            }
        }

        protected virtual void ExecuteUninstall(IPackage package)
        {
            string fullName = package.GetFullName();
            object[] objArray1 = new object[] { fullName };
            this.Logger.Log(MessageLevel.Info, NuGetResources.Log_BeginUninstallPackage, objArray1);
            PackageOperationEventArgs e = this.CreateOperation(package);
            this.OnUninstalling(e);
            if (!e.Cancel)
            {
                this.OnRemoveFiles(e);
                this.LocalRepository.RemovePackage(package);
                object[] args = new object[] { fullName };
                this.Logger.Log(MessageLevel.Info, NuGetResources.Log_SuccessfullyUninstalledPackage, args);
                this.OnUninstalled(e);
            }
        }

        private void ExpandFiles(IPackage package)
        {
            IBatchProcessor<string> fileSystem = this.FileSystem as IBatchProcessor<string>;
            try
            {
                IPackage package2;
                List<IPackageFile> files = package.GetFiles().ToList<IPackageFile>();
                if (fileSystem != null)
                {
                    fileSystem.BeginProcessing(from p in files select p.Path, PackageAction.Install);
                }
                this.FileSystem.AddFiles(files, this.PathResolver.GetPackageDirectory(package));
                if (PackageHelper.IsSatellitePackage(package, this.LocalRepository, null, out package2))
                {
                    IEnumerable<IPackageFile> satelliteFiles = package.GetSatelliteFiles();
                    this.FileSystem.AddFiles(satelliteFiles, this.PathResolver.GetPackageDirectory(package2));
                }
            }
            finally
            {
                if (fileSystem != null)
                {
                    fileSystem.EndProcessing();
                }
            }
        }

        public bool IsProjectLevel(IPackage package) => 
            (package.HasProjectContent() || ((from p in package.DependencySets select p.Dependencies).Any<PackageDependency>() || this.LocalRepository.IsReferenced(package.Id, package.Version)));

        public virtual IPackage LocatePackageToUninstall(IProjectManager projectManager, string id, SemanticVersion version)
        {
            IPackage local1 = this.LocalRepository.FindPackagesById(id).SingleOrDefault<IPackage>();
            if (local1 == null)
            {
                throw new InvalidOperationException();
            }
            return local1;
        }

        protected virtual void OnExpandFiles(PackageOperationEventArgs e)
        {
            this.ExpandFiles(e.Package);
        }

        protected virtual void OnInstalled(PackageOperationEventArgs e)
        {
            if (this.PackageInstalled != null)
            {
                this.PackageInstalled(this, e);
            }
        }

        protected virtual void OnInstalling(PackageOperationEventArgs e)
        {
            if (this.PackageInstalling != null)
            {
                this.PackageInstalling(this, e);
            }
        }

        protected virtual void OnRemoveFiles(PackageOperationEventArgs e)
        {
            this.RemoveFiles(e.Package);
        }

        protected virtual void OnUninstalled(PackageOperationEventArgs e)
        {
            if (this.PackageUninstalled != null)
            {
                this.PackageUninstalled(this, e);
            }
        }

        protected virtual void OnUninstalling(PackageOperationEventArgs e)
        {
            if (this.PackageUninstalling != null)
            {
                this.PackageUninstalling(this, e);
            }
        }

        private void RemoveFiles(IPackage package)
        {
            IPackage package2;
            string packageDirectory = this.PathResolver.GetPackageDirectory(package);
            if (PackageHelper.IsSatellitePackage(package, this.LocalRepository, null, out package2))
            {
                IEnumerable<IPackageFile> satelliteFiles = package.GetSatelliteFiles();
                this.FileSystem.DeleteFiles(satelliteFiles, this.PathResolver.GetPackageDirectory(package2));
            }
            this.FileSystem.DeleteFiles(package.GetFiles(), packageDirectory);
        }

        public IFileSystem FileSystem { get; set; }

        public IPackageRepository SourceRepository { get; private set; }

        public IDependencyResolver2 DependencyResolver { get; private set; }

        public ISharedPackageRepository LocalRepository { get; private set; }

        public IPackagePathResolver PathResolver { get; private set; }

        public ILogger Logger
        {
            get => 
                (this._logger ?? NullLogger.Instance);
            set => 
                (this._logger = value);
        }

        public NuGet.DependencyVersion DependencyVersion { get; set; }

        public bool CheckDowngrade { get; set; }

        public bool BindingRedirectEnabled
        {
            get => 
                this._bindingRedirectEnabled;
            set => 
                (this._bindingRedirectEnabled = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PackageManager.<>c <>9 = new PackageManager.<>c();
            public static Func<IPackageFile, string> <>9__45_0;
            public static Func<PackageDependencySet, IEnumerable<PackageDependency>> <>9__59_0;

            internal string <ExpandFiles>b__45_0(IPackageFile p) => 
                p.Path;

            internal IEnumerable<PackageDependency> <IsProjectLevel>b__59_0(PackageDependencySet p) => 
                p.Dependencies;
        }
    }
}

