namespace NuGet
{
    using System;

    internal interface IPackageCacheRepository : IPackageRepository
    {
        bool InvokeOnPackage(string packageId, SemanticVersion version, Action<Stream> action);
    }
}

