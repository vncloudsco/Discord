namespace NuGet
{
    using System;
    using System.Collections.Generic;

    internal interface IDependencyResolver2
    {
        IEnumerable<IPackage> FindPackages(IEnumerable<string> packageIds);
        IPackage ResolveDependency(PackageDependency dependency, IPackageConstraintProvider constraintProvider, bool allowPrereleaseVersions, bool preferListedPackages, DependencyVersion dependencyVersion);
    }
}

