namespace NuGet
{
    using System;
    using System.Runtime.InteropServices;

    internal interface ILatestPackageLookup
    {
        bool TryFindLatestPackageById(string id, out SemanticVersion latestVersion);
        bool TryFindLatestPackageById(string id, bool includePrerelease, out IPackage package);
    }
}

