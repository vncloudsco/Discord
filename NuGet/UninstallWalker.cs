namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;

    internal class UninstallWalker : PackageWalker, IPackageOperationResolver
    {
        private readonly IDictionary<IPackage, IEnumerable<IPackage>> _forcedRemoved;
        private readonly IDictionary<IPackage, IEnumerable<IPackage>> _skippedPackages;
        private readonly bool _removeDependencies;

        internal UninstallWalker(IPackageRepository repository, IDependentsResolver dependentsResolver, ILogger logger, bool removeDependencies, bool forceRemove) : this(repository, dependentsResolver, null, logger, removeDependencies, forceRemove)
        {
        }

        public UninstallWalker(IPackageRepository repository, IDependentsResolver dependentsResolver, FrameworkName targetFramework, ILogger logger, bool removeDependencies, bool forceRemove) : base(targetFramework)
        {
            this._forcedRemoved = new Dictionary<IPackage, IEnumerable<IPackage>>((IEqualityComparer<IPackage>) PackageEqualityComparer.IdAndVersion);
            this._skippedPackages = new Dictionary<IPackage, IEnumerable<IPackage>>((IEqualityComparer<IPackage>) PackageEqualityComparer.IdAndVersion);
            if (dependentsResolver == null)
            {
                throw new ArgumentNullException("dependentsResolver");
            }
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            this.Logger = logger;
            this.Repository = repository;
            this.DependentsResolver = dependentsResolver;
            this.Force = forceRemove;
            this.ThrowOnConflicts = true;
            this.Operations = new Stack<PackageOperation>();
            this._removeDependencies = removeDependencies;
        }

        protected virtual InvalidOperationException CreatePackageHasDependentsException(IPackage package, IEnumerable<IPackage> dependents)
        {
            if (dependents.Count<IPackage>() == 1)
            {
                object[] objArray1 = new object[] { package.GetFullName(), dependents.Single<IPackage>().GetFullName() };
                return new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.PackageHasDependent, objArray1));
            }
            object[] args = new object[] { package.GetFullName(), string.Join(", ", (IEnumerable<string>) (from d in dependents select d.GetFullName())) };
            return new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.PackageHasDependents, args));
        }

        private IEnumerable<IPackage> GetDependents(IPackage package) => 
            (from p in this.DependentsResolver.GetDependents(package)
                where !this.IsConnected(p)
                select p);

        private bool IsConnected(IPackage package)
        {
            if (base.Marker.Contains(package))
            {
                return true;
            }
            IEnumerable<IPackage> dependents = this.DependentsResolver.GetDependents(package);
            return (dependents.Any<IPackage>() && Enumerable.All<IPackage>(dependents, new Func<IPackage, bool>(this.IsConnected)));
        }

        protected override void OnAfterPackageWalk(IPackage package)
        {
            this.Operations.Push(new PackageOperation(package, PackageAction.Uninstall));
        }

        protected override bool OnAfterResolveDependency(IPackage package, IPackage dependency)
        {
            if (!this.Force)
            {
                IEnumerable<IPackage> dependents = this.GetDependents(dependency);
                if (dependents.Any<IPackage>())
                {
                    this._skippedPackages[dependency] = dependents;
                    return false;
                }
            }
            return true;
        }

        protected override void OnBeforePackageWalk(IPackage package)
        {
            IEnumerable<IPackage> dependents = this.GetDependents(package);
            if (dependents.Any<IPackage>())
            {
                if (this.Force)
                {
                    this._forcedRemoved[package] = dependents;
                }
                else if (this.ThrowOnConflicts)
                {
                    throw this.CreatePackageHasDependentsException(package, dependents);
                }
            }
        }

        protected override void OnDependencyResolveError(PackageDependency dependency)
        {
            object[] args = new object[] { dependency };
            this.Logger.Log(MessageLevel.Warning, NuGetResources.UnableToLocateDependency, args);
        }

        protected override IPackage ResolveDependency(PackageDependency dependency) => 
            DependencyResolveUtility.ResolveDependency(this.Repository, dependency, true, false);

        public IEnumerable<PackageOperation> ResolveOperations(IPackage package)
        {
            this.Operations.Clear();
            base.Marker.Clear();
            base.Walk(package);
            foreach (KeyValuePair<IPackage, IEnumerable<IPackage>> pair in this._forcedRemoved)
            {
                object[] args = new object[] { pair.Key, string.Join(", ", (IEnumerable<string>) (from p in pair.Value select p.GetFullName())) };
                this.Logger.Log(MessageLevel.Warning, NuGetResources.Warning_UninstallingPackageWillBreakDependents, args);
            }
            foreach (KeyValuePair<IPackage, IEnumerable<IPackage>> pair2 in this._skippedPackages)
            {
                object[] args = new object[] { pair2.Key, string.Join(", ", (IEnumerable<string>) (from p in pair2.Value select p.GetFullName())) };
                this.Logger.Log(MessageLevel.Warning, NuGetResources.Warning_PackageSkippedBecauseItIsInUse, args);
            }
            return this.Operations.Reduce();
        }

        protected virtual void WarnRemovingPackageBreaksDependents(IPackage package, IEnumerable<IPackage> dependents)
        {
            object[] args = new object[] { package.GetFullName(), string.Join(", ", (IEnumerable<string>) (from d in dependents select d.GetFullName())) };
            this.Logger.Log(MessageLevel.Warning, NuGetResources.Warning_UninstallingPackageWillBreakDependents, args);
        }

        protected ILogger Logger { get; private set; }

        protected IPackageRepository Repository { get; private set; }

        protected override bool IgnoreDependencies =>
            !this._removeDependencies;

        protected override bool SkipDependencyResolveError =>
            true;

        internal bool DisableWalkInfo { get; set; }

        protected override bool IgnoreWalkInfo =>
            (this.DisableWalkInfo || base.IgnoreWalkInfo);

        private Stack<PackageOperation> Operations { get; set; }

        public bool Force { get; private set; }

        public bool ThrowOnConflicts { get; set; }

        protected IDependentsResolver DependentsResolver { get; private set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly UninstallWalker.<>c <>9 = new UninstallWalker.<>c();
            public static Func<IPackage, string> <>9__43_0;
            public static Func<IPackage, string> <>9__44_0;
            public static Func<IPackage, string> <>9__46_0;
            public static Func<IPackage, string> <>9__46_1;

            internal string <CreatePackageHasDependentsException>b__44_0(IPackage d) => 
                d.GetFullName();

            internal string <ResolveOperations>b__46_0(IPackage p) => 
                p.GetFullName();

            internal string <ResolveOperations>b__46_1(IPackage p) => 
                p.GetFullName();

            internal string <WarnRemovingPackageBreaksDependents>b__43_0(IPackage d) => 
                d.GetFullName();
        }
    }
}

