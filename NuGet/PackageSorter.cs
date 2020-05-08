namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Versioning;

    internal class PackageSorter : PackageWalker
    {
        private IPackageRepository _repository;
        private IList<IPackage> _sortedPackages;

        internal PackageSorter()
        {
        }

        public PackageSorter(FrameworkName targetFramework) : base(targetFramework)
        {
        }

        public IEnumerable<IPackage> GetPackagesByDependencyOrder(IPackageRepository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            base.Marker.Clear();
            this._repository = repository;
            this._sortedPackages = new List<IPackage>();
            foreach (IPackage package in this._repository.GetPackages())
            {
                base.Walk(package);
            }
            return this._sortedPackages;
        }

        protected override void OnAfterPackageWalk(IPackage package)
        {
            base.OnAfterPackageWalk(package);
            this._sortedPackages.Add(package);
        }

        protected override void OnDependencyResolveError(PackageDependency dependency)
        {
        }

        protected override IPackage ResolveDependency(PackageDependency dependency) => 
            DependencyResolveUtility.ResolveDependency(this._repository, dependency, true, false);

        protected override bool RaiseErrorOnCycle =>
            false;

        protected override bool IgnoreWalkInfo =>
            true;

        protected override bool SkipDependencyResolveError =>
            true;
    }
}

