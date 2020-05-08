namespace NuGet
{
    using System;
    using System.Linq;

    internal interface IPackageRepository
    {
        void AddPackage(IPackage package);
        IQueryable<IPackage> GetPackages();
        void RemovePackage(IPackage package);

        string Source { get; }

        PackageSaveModes PackageSaveMode { get; set; }

        bool SupportsPrereleasePackages { get; }
    }
}

