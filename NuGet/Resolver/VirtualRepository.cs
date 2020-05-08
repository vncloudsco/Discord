namespace NuGet.Resolver
{
    using NuGet;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal class VirtualRepository : IPackageRepository
    {
        private HashSet<IPackage> _packages = new HashSet<IPackage>((IEqualityComparer<IPackage>) PackageEqualityComparer.IdAndVersion);

        public VirtualRepository(IPackageRepository repo)
        {
            if (repo != null)
            {
                this._packages.AddRange<IPackage>(repo.GetPackages());
            }
        }

        public void AddPackage(IPackage package)
        {
            this._packages.Add(package);
        }

        public IQueryable<IPackage> GetPackages() => 
            this._packages.AsQueryable<IPackage>();

        public void RemovePackage(IPackage package)
        {
            this._packages.Remove(package);
        }

        public string Source =>
            string.Empty;

        public PackageSaveModes PackageSaveMode { get; set; }

        public bool SupportsPrereleasePackages =>
            true;
    }
}

