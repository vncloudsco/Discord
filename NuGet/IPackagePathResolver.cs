namespace NuGet
{
    using System;

    internal interface IPackagePathResolver
    {
        string GetInstallPath(IPackage package);
        string GetPackageDirectory(IPackage package);
        string GetPackageDirectory(string packageId, SemanticVersion version);
        string GetPackageFileName(IPackage package);
        string GetPackageFileName(string packageId, SemanticVersion version);
    }
}

