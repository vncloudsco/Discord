namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;

    internal static class PackageHelper
    {
        public static bool IsAssembly(string path) => 
            (path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || (path.EndsWith(".winmd", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)));

        public static bool IsManifest(string path) => 
            Path.GetExtension(path).Equals(Constants.ManifestExtension, StringComparison.OrdinalIgnoreCase);

        public static bool IsPackageFile(string path) => 
            Path.GetExtension(path).Equals(Constants.PackageExtension, StringComparison.OrdinalIgnoreCase);

        public static bool IsSatellitePackage(IPackageMetadata package, IPackageRepository repository, FrameworkName targetFramework, out IPackage runtimePackage)
        {
            runtimePackage = null;
            if (package.IsSatellitePackage())
            {
                string packageId = package.Id.Substring(0, package.Id.Length - (package.Language.Length + 1));
                PackageDependency dependency = package.FindDependency(packageId, targetFramework);
                if (dependency != null)
                {
                    runtimePackage = repository.FindPackage(packageId, dependency.VersionSpec, true, true);
                }
            }
            return (runtimePackage != null);
        }

        public static IPackage ResolvePackage(IPackageRepository repository, string packageId, SemanticVersion version)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            if (string.IsNullOrEmpty(packageId))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "packageId");
            }
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            IPackage package = repository.FindPackage(packageId, version);
            if (package != null)
            {
                return package;
            }
            object[] args = new object[] { packageId, version };
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.UnknownPackageSpecificVersion, args));
        }
    }
}

