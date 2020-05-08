namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal interface IServiceBasedRepository : IPackageRepository
    {
        IEnumerable<IPackage> GetUpdates(IEnumerable<IPackageName> packages, bool includePrerelease, bool includeAllVersions, IEnumerable<FrameworkName> targetFrameworks, IEnumerable<IVersionSpec> versionConstraints);
        IQueryable<IPackage> Search(string searchTerm, IEnumerable<string> targetFrameworks, bool allowPrereleaseVersions, bool includeDelisted);
    }
}

