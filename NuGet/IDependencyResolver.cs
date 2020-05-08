namespace NuGet
{
    using System;

    internal interface IDependencyResolver
    {
        IPackage ResolveDependency(PackageDependency dependency, IPackageConstraintProvider constraintProvider, bool allowPrereleaseVersions, bool preferListedPackages, DependencyVersion dependencyVersion);
    }
}

