namespace NuGet
{
    using System;
    using System.IO;

    internal class DefaultPackagePathResolver : IPackagePathResolver
    {
        private readonly IFileSystem _fileSystem;
        private readonly bool _useSideBySidePaths;

        public DefaultPackagePathResolver(IFileSystem fileSystem) : this(fileSystem, true)
        {
        }

        public DefaultPackagePathResolver(string path) : this(new PhysicalFileSystem(path))
        {
        }

        public DefaultPackagePathResolver(IFileSystem fileSystem, bool useSideBySidePaths)
        {
            this._useSideBySidePaths = useSideBySidePaths;
            if (fileSystem == null)
            {
                throw new ArgumentNullException("fileSystem");
            }
            this._fileSystem = fileSystem;
        }

        public DefaultPackagePathResolver(string path, bool useSideBySidePaths) : this(new PhysicalFileSystem(path), useSideBySidePaths)
        {
        }

        public virtual string GetInstallPath(IPackage package) => 
            Path.Combine(this._fileSystem.Root, this.GetPackageDirectory(package));

        public virtual string GetPackageDirectory(IPackage package) => 
            this.GetPackageDirectory(package.Id, package.Version);

        public virtual string GetPackageDirectory(string packageId, SemanticVersion version)
        {
            string str = packageId;
            if (this._useSideBySidePaths)
            {
                str = str + "." + version;
            }
            return str;
        }

        public virtual string GetPackageFileName(IPackage package) => 
            this.GetPackageFileName(package.Id, package.Version);

        public virtual string GetPackageFileName(string packageId, SemanticVersion version)
        {
            string str = packageId;
            if (this._useSideBySidePaths)
            {
                str = str + "." + version;
            }
            return (str + Constants.PackageExtension);
        }
    }
}

