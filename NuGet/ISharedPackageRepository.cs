namespace NuGet
{
    using System;
    using System.Collections.Generic;

    internal interface ISharedPackageRepository : IPackageRepository
    {
        bool IsReferenced(string packageId, SemanticVersion version);
        bool IsSolutionReferenced(string packageId, SemanticVersion version);
        IEnumerable<IPackageRepository> LoadProjectRepositories();
        void RegisterRepository(PackageReferenceFile packageReferenceFile);
        void UnregisterRepository(PackageReferenceFile packageReferenceFile);
    }
}

