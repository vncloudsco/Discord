namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;

    internal class DependentsWalker : PackageWalker, IDependentsResolver
    {
        internal DependentsWalker(IPackageRepository repository) : this(repository, null)
        {
        }

        public DependentsWalker(IPackageRepository repository, FrameworkName targetFramework) : base(targetFramework)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            this.Repository = repository;
        }

        public IEnumerable<IPackage> GetDependents(IPackage package)
        {
            HashSet<IPackage> set;
            if (this.DependentsLookup == null)
            {
                this.DependentsLookup = new Dictionary<IPackage, HashSet<IPackage>>((IEqualityComparer<IPackage>) PackageEqualityComparer.IdAndVersion);
                foreach (IPackage package2 in this.Repository.GetPackages())
                {
                    base.Walk(package2);
                }
            }
            return (!this.DependentsLookup.TryGetValue(package, out set) ? Enumerable.Empty<IPackage>() : set);
        }

        protected override bool OnAfterResolveDependency(IPackage package, IPackage dependency)
        {
            HashSet<IPackage> set;
            if (!this.DependentsLookup.TryGetValue(dependency, out set))
            {
                set = new HashSet<IPackage>((IEqualityComparer<IPackage>) PackageEqualityComparer.IdAndVersion);
                this.DependentsLookup[dependency] = set;
            }
            set.Add(package);
            return base.OnAfterResolveDependency(package, dependency);
        }

        protected override IPackage ResolveDependency(PackageDependency dependency) => 
            DependencyResolveUtility.ResolveDependency(this.Repository, dependency, true, false);

        protected override bool RaiseErrorOnCycle =>
            false;

        protected override bool IgnoreWalkInfo =>
            true;

        protected IPackageRepository Repository { get; private set; }

        private IDictionary<IPackage, HashSet<IPackage>> DependentsLookup { get; set; }
    }
}

