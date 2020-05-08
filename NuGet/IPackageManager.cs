namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;

    internal interface IPackageManager
    {
        event EventHandler<PackageOperationEventArgs> PackageInstalled;

        event EventHandler<PackageOperationEventArgs> PackageInstalling;

        event EventHandler<PackageOperationEventArgs> PackageUninstalled;

        event EventHandler<PackageOperationEventArgs> PackageUninstalling;

        void AddBindingRedirects(IProjectManager projectManager);
        void Execute(PackageOperation operation);
        bool IsProjectLevel(IPackage package);
        IPackage LocatePackageToUninstall(IProjectManager projectManager, string id, SemanticVersion version);

        IFileSystem FileSystem { get; set; }

        ISharedPackageRepository LocalRepository { get; }

        ILogger Logger { get; set; }

        NuGet.DependencyVersion DependencyVersion { get; set; }

        IPackageRepository SourceRepository { get; }

        IDependencyResolver2 DependencyResolver { get; }

        IPackagePathResolver PathResolver { get; }

        bool BindingRedirectEnabled { get; set; }
    }
}

