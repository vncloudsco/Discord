namespace NuGet
{
    using System;
    using System.Collections.Generic;

    internal class DependencyResolverFromRepo : IDependencyResolver2
    {
        private IPackageRepository _repo;

        public DependencyResolverFromRepo(IPackageRepository repo)
        {
            this._repo = repo;
        }

        public IEnumerable<IPackage> FindPackages(IEnumerable<string> packageIds) => 
            (from id in packageIds select this._repo.FindPackagesById(id));

        public IPackage ResolveDependency(PackageDependency dependency, IPackageConstraintProvider constraintProvider, bool allowPrereleaseVersions, bool preferListedPackages, DependencyVersion dependencyVersion)
        {
            IDependencyResolver resolver = this._repo as IDependencyResolver;
            return ((resolver == null) ? DependencyResolveUtility.ResolveDependencyCore(this._repo, dependency, constraintProvider, allowPrereleaseVersions, preferListedPackages, dependencyVersion) : resolver.ResolveDependency(dependency, constraintProvider, allowPrereleaseVersions, preferListedPackages, dependencyVersion));
        }
    }
}

