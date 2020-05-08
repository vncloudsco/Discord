namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal static class DependencyResolveUtility
    {
        internal static IEnumerable<IPackage> FilterPackagesByConstraints(IPackageConstraintProvider constraintProvider, IEnumerable<IPackage> packages, string packageId, bool allowPrereleaseVersions)
        {
            constraintProvider = constraintProvider ?? NullConstraintProvider.Instance;
            IVersionSpec constraint = constraintProvider.GetConstraint(packageId);
            if (constraint != null)
            {
                packages = packages.FindByVersion(constraint);
            }
            if (!allowPrereleaseVersions)
            {
                packages = from p in packages
                    where p.IsReleaseVersion()
                    select p;
            }
            return packages;
        }

        public static IPackage ResolveDependency(object repository, PackageDependency dependency, bool allowPrereleaseVersions, bool preferListedPackages) => 
            ResolveDependency(repository, dependency, null, allowPrereleaseVersions, preferListedPackages, DependencyVersion.Lowest);

        public static IPackage ResolveDependency(object repository, PackageDependency dependency, IPackageConstraintProvider constraintProvider, bool allowPrereleaseVersions, bool preferListedPackages, DependencyVersion dependencyVersion)
        {
            IDependencyResolver resolver = repository as IDependencyResolver;
            return ((resolver == null) ? ResolveDependencyCore((IPackageRepository) repository, dependency, constraintProvider, allowPrereleaseVersions, preferListedPackages, dependencyVersion) : resolver.ResolveDependency(dependency, constraintProvider, allowPrereleaseVersions, preferListedPackages, dependencyVersion));
        }

        private static IPackage ResolveDependencyCore(IEnumerable<IPackage> packages, PackageDependency dependency, DependencyVersion dependencyVersion)
        {
            if (dependency.VersionSpec == null)
            {
                return (from p in packages
                    orderby p.Version descending
                    select p).FirstOrDefault<IPackage>();
            }
            packages = from p in packages.FindByVersion(dependency.VersionSpec)
                orderby p.Version
                select p;
            return packages.SelectDependency(dependencyVersion);
        }

        public static IPackage ResolveDependencyCore(IPackageRepository repository, PackageDependency dependency, IPackageConstraintProvider constraintProvider, bool allowPrereleaseVersions, bool preferListedPackages, DependencyVersion dependencyVersion)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            if (dependency == null)
            {
                throw new ArgumentNullException("dependency");
            }
            IEnumerable<IPackage> packages = repository.FindPackagesById(dependency.Id).ToList<IPackage>();
            IList<IPackage> list = FilterPackagesByConstraints(constraintProvider, packages, dependency.Id, allowPrereleaseVersions).ToList<IPackage>();
            if (preferListedPackages)
            {
                IPackage package = ResolveDependencyCore(Enumerable.Where<IPackage>(list, new Func<IPackage, bool>(PackageExtensions.IsListed)), dependency, dependencyVersion);
                if (package != null)
                {
                    return package;
                }
            }
            return ResolveDependencyCore(list, dependency, dependencyVersion);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly DependencyResolveUtility.<>c <>9 = new DependencyResolveUtility.<>c();
            public static Func<IPackage, bool> <>9__3_0;
            public static Func<IPackage, SemanticVersion> <>9__4_0;
            public static Func<IPackage, SemanticVersion> <>9__4_1;

            internal bool <FilterPackagesByConstraints>b__3_0(IPackage p) => 
                p.IsReleaseVersion();

            internal SemanticVersion <ResolveDependencyCore>b__4_0(IPackage p) => 
                p.Version;

            internal SemanticVersion <ResolveDependencyCore>b__4_1(IPackage p) => 
                p.Version;
        }
    }
}

