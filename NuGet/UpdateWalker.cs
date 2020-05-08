namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;

    internal class UpdateWalker : InstallWalker
    {
        private readonly IDependentsResolver _dependentsResolver;

        internal UpdateWalker(IPackageRepository localRepository, IDependencyResolver2 sourceRepository, IDependentsResolver dependentsResolver, IPackageConstraintProvider constraintProvider, ILogger logger, bool updateDependencies, bool allowPrereleaseVersions) : this(localRepository, sourceRepository, dependentsResolver, constraintProvider, null, logger, updateDependencies, allowPrereleaseVersions)
        {
        }

        public UpdateWalker(IPackageRepository localRepository, IDependencyResolver2 sourceRepository, IDependentsResolver dependentsResolver, IPackageConstraintProvider constraintProvider, FrameworkName targetFramework, ILogger logger, bool updateDependencies, bool allowPrereleaseVersions) : base(localRepository, sourceRepository, constraintProvider, targetFramework, logger, !updateDependencies, allowPrereleaseVersions, DependencyVersion.Lowest)
        {
            this._dependentsResolver = dependentsResolver;
            this.AcceptedTargets = PackageTargets.All;
        }

        protected override ConflictResult GetConflict(IPackage package)
        {
            ConflictResult conflict = base.GetConflict(package);
            if (conflict == null)
            {
                IPackage conflictingPackage = base.Repository.FindPackage(package.Id);
                if (conflictingPackage != null)
                {
                    conflict = new ConflictResult(conflictingPackage, base.Repository, this._dependentsResolver);
                }
            }
            return conflict;
        }

        protected override void OnAfterPackageWalk(IPackage package)
        {
            if (base.DisableWalkInfo)
            {
                base.OnAfterPackageWalk(package);
            }
            else
            {
                PackageWalkInfo packageInfo = base.GetPackageInfo(package);
                if (this.AcceptedTargets.HasFlag(packageInfo.Target))
                {
                    base.OnAfterPackageWalk(package);
                }
            }
        }

        public PackageTargets AcceptedTargets { get; set; }
    }
}

