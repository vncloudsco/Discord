namespace NuGet
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    internal class PackageOperationEventArgs : CancelEventArgs
    {
        public PackageOperationEventArgs(IPackage package, IFileSystem fileSystem, string installPath)
        {
            this.Package = package;
            this.InstallPath = installPath;
            this.FileSystem = fileSystem;
        }

        public string InstallPath { get; private set; }

        public IPackage Package { get; private set; }

        public IFileSystem FileSystem { get; private set; }
    }
}

