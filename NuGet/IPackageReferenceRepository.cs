namespace NuGet
{
    using System;
    using System.Runtime.Versioning;

    internal interface IPackageReferenceRepository : IPackageRepository
    {
        void AddPackage(string packageId, SemanticVersion version, bool developmentDependency, FrameworkName targetFramework);
        FrameworkName GetPackageTargetFramework(string packageId);
    }
}

