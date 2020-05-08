namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Globalization;

    internal static class PackageRepositoryHelper
    {
        public static IPackage ResolvePackage(IPackageRepository sourceRepository, IPackageRepository localRepository, string packageId, SemanticVersion version, bool allowPrereleaseVersions) => 
            ResolvePackage(sourceRepository, localRepository, NullConstraintProvider.Instance, packageId, version, allowPrereleaseVersions);

        public static IPackage ResolvePackage(IPackageRepository sourceRepository, IPackageRepository localRepository, IPackageConstraintProvider constraintProvider, string packageId, SemanticVersion version, bool allowPrereleaseVersions)
        {
            if (string.IsNullOrEmpty(packageId))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "packageId");
            }
            IPackage package = null;
            if (version != null)
            {
                package = localRepository.FindPackage(packageId, version, allowPrereleaseVersions, true);
            }
            if (package == null)
            {
                package = sourceRepository.FindPackage(packageId, version, constraintProvider, allowPrereleaseVersions, false);
                if (package != null)
                {
                    package = localRepository.FindPackage(package.Id, package.Version, allowPrereleaseVersions, true) ?? package;
                }
            }
            if (package != null)
            {
                return package;
            }
            if (version == null)
            {
                object[] objArray2 = new object[] { packageId };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.UnknownPackage, objArray2));
            }
            object[] args = new object[] { packageId, version };
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.UnknownPackageSpecificVersion, args));
        }
    }
}

