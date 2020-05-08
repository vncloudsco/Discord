namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;

    internal class InstallWalker : PackageWalker, IPackageOperationResolver
    {
        private readonly bool _ignoreDependencies;
        private bool _allowPrereleaseVersions;
        private readonly OperationLookup _operations;
        private bool _isDowngrade;
        private readonly HashSet<IPackage> _packagesToKeep;
        private IDictionary<string, IList<IPackage>> _packagesByDependencyOrder;

        internal InstallWalker(IPackageRepository localRepository, IDependencyResolver2 dependencyResolver, ILogger logger, bool ignoreDependencies, bool allowPrereleaseVersions, DependencyVersion dependencyVersion) : this(localRepository, dependencyResolver, null, logger, ignoreDependencies, allowPrereleaseVersions, dependencyVersion)
        {
        }

        public InstallWalker(IPackageRepository localRepository, IDependencyResolver2 dependencyResolver, FrameworkName targetFramework, ILogger logger, bool ignoreDependencies, bool allowPrereleaseVersions, DependencyVersion dependencyVersion) : this(localRepository, dependencyResolver, NullConstraintProvider.Instance, targetFramework, logger, ignoreDependencies, allowPrereleaseVersions, dependencyVersion)
        {
        }

        public InstallWalker(IPackageRepository localRepository, IDependencyResolver2 dependencyResolver, IPackageConstraintProvider constraintProvider, FrameworkName targetFramework, ILogger logger, bool ignoreDependencies, bool allowPrereleaseVersions, DependencyVersion dependencyVersion) : base(targetFramework)
        {
            this._packagesToKeep = new HashSet<IPackage>((IEqualityComparer<IPackage>) PackageEqualityComparer.IdAndVersion);
            if (dependencyResolver == null)
            {
                throw new ArgumentNullException("dependencyResolver");
            }
            if (localRepository == null)
            {
                throw new ArgumentNullException("localRepository");
            }
            if (logger == null)
            {
                throw new ArgumentNullException("logger");
            }
            this.Repository = localRepository;
            this.Logger = logger;
            this.DependencyResolver = dependencyResolver;
            this._ignoreDependencies = ignoreDependencies;
            this.ConstraintProvider = constraintProvider;
            this._operations = new OperationLookup();
            this._allowPrereleaseVersions = allowPrereleaseVersions;
            base.DependencyVersion = dependencyVersion;
            this.CheckDowngrade = true;
        }

        private static InvalidOperationException CreatePackageConflictException(IPackage resolvedPackage, IPackage package, IEnumerable<IPackage> dependents)
        {
            if (dependents.Count<IPackage>() == 1)
            {
                object[] objArray1 = new object[] { package.GetFullName(), resolvedPackage.GetFullName(), dependents.Single<IPackage>().Id };
                return new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.ConflictErrorWithDependent, objArray1));
            }
            object[] args = new object[] { package.GetFullName(), resolvedPackage.GetFullName(), string.Join(", ", (IEnumerable<string>) (from d in dependents select d.Id)) };
            return new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.ConflictErrorWithDependents, args));
        }

        private static IEnumerable<IPackage> FindCompatiblePackages(IDependencyResolver2 dependencyResolver, IPackageConstraintProvider constraintProvider, IEnumerable<string> packageIds, IPackage package, FrameworkName targetFramework, bool allowPrereleaseVersions) => 
            (from p in dependencyResolver.FindPackages(packageIds)
                where allowPrereleaseVersions || p.IsReleaseVersion()
                let dependency = p.FindDependency(package.Id, targetFramework)
                let otherConstaint = constraintProvider.GetConstraint(p.Id)
                where (dependency != null) && (dependency.VersionSpec.Satisfies(package.Version) && ((otherConstaint == null) || otherConstaint.Satisfies(package.Version)))
                select p);

        protected virtual ConflictResult GetConflict(IPackage package)
        {
            IPackage conflictingPackage = base.Marker.FindPackage(package.Id);
            return ((conflictingPackage == null) ? null : new ConflictResult(conflictingPackage, base.Marker, base.Marker));
        }

        private IEnumerable<IPackage> GetDependents(ConflictResult conflict)
        {
            IEnumerable<IPackage> packages = this._operations.GetPackages(PackageAction.Uninstall);
            return conflict.DependentsResolver.GetDependents(conflict.Package).Except<IPackage>(packages, ((IEqualityComparer<IPackage>) PackageEqualityComparer.IdAndVersion));
        }

        protected override void OnAfterPackageWalk(IPackage package)
        {
            if (this.Repository.Exists(package))
            {
                this._operations.RemoveOperation(package, PackageAction.Uninstall);
                this._packagesToKeep.Add(package);
            }
            else
            {
                PackageOperation operation = new PackageOperation(package, PackageAction.Install);
                if (GetPackageTarget(package) == PackageTargets.External)
                {
                    operation.Target = PackageOperationTarget.PackagesFolder;
                }
                this._operations.AddOperation(operation);
            }
            if (this._packagesByDependencyOrder != null)
            {
                IList<IPackage> list;
                if (!this._packagesByDependencyOrder.TryGetValue(package.Id, out list))
                {
                    this._packagesByDependencyOrder[package.Id] = list = new List<IPackage>();
                }
                list.Add(package);
            }
        }

        protected override void OnBeforePackageWalk(IPackage package)
        {
            ConflictResult conflict = this.GetConflict(package);
            if ((conflict != null) && !PackageEqualityComparer.IdAndVersion.Equals(package, conflict.Package))
            {
                IEnumerable<IPackage> source = from dependentPackage in this.GetDependents(conflict)
                    let dependency = dependentPackage.FindDependency(package.Id, this.TargetFramework)
                    where (dependency != null) && !dependency.VersionSpec.Satisfies(package.Version)
                    select dependentPackage;
                if (source.Any<IPackage>() && !this.TryUpdate(source, conflict, package, out source))
                {
                    throw CreatePackageConflictException(package, conflict.Package, source);
                }
                if (this._isDowngrade || (package.Version >= conflict.Package.Version))
                {
                    this.Uninstall(conflict.Package, conflict.DependentsResolver, conflict.Repository);
                }
                else
                {
                    object[] args = new object[] { package.Id };
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.NewerVersionAlreadyReferenced, args));
                }
            }
        }

        protected override void OnDependencyResolveError(PackageDependency dependency)
        {
            IVersionSpec constraint = this.ConstraintProvider.GetConstraint(dependency.Id);
            string str = string.Empty;
            if (constraint != null)
            {
                object[] objArray1 = new object[] { dependency.Id, VersionUtility.PrettyPrint(constraint), this.ConstraintProvider.Source };
                str = string.Format(CultureInfo.CurrentCulture, NuGetResources.AdditonalConstraintsDefined, objArray1);
            }
            object[] args = new object[] { dependency };
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.UnableToResolveDependency + str, args));
        }

        protected override IPackage ResolveDependency(PackageDependency dependency)
        {
            object[] args = new object[] { dependency };
            this.Logger.Log(MessageLevel.Info, NuGetResources.Log_AttemptingToRetrievePackageFromSource, args);
            if (!this._isDowngrade)
            {
                IPackage package = DependencyResolveUtility.ResolveDependency(this.Repository, dependency, this.ConstraintProvider, true, false, base.DependencyVersion);
                if (package != null)
                {
                    return package;
                }
            }
            return this.DependencyResolver.ResolveDependency(dependency, this.ConstraintProvider, this.AllowPrereleaseVersions, true, base.DependencyVersion);
        }

        public IEnumerable<PackageOperation> ResolveOperations(IPackage package)
        {
            if (!this.CheckDowngrade)
            {
                this._isDowngrade = false;
            }
            else
            {
                IPackage package2 = this.Repository.FindPackage(package.Id);
                if ((package2 != null) && (package2.Version > package.Version))
                {
                    this._isDowngrade = true;
                }
            }
            this._operations.Clear();
            base.Marker.Clear();
            this._packagesToKeep.Clear();
            base.Walk(package);
            return this.Operations.Reduce();
        }

        public IList<PackageOperation> ResolveOperations(IEnumerable<IPackage> packages, out IList<IPackage> packagesByDependencyOrder, bool allowPrereleaseVersionsBasedOnPackage = false)
        {
            this._packagesByDependencyOrder = new Dictionary<string, IList<IPackage>>();
            this._operations.Clear();
            base.Marker.Clear();
            this._packagesToKeep.Clear();
            foreach (IPackage package in packages)
            {
                if (!this._operations.Contains(package, PackageAction.Install))
                {
                    bool flag = this._allowPrereleaseVersions;
                    try
                    {
                        if (allowPrereleaseVersionsBasedOnPackage)
                        {
                            this._allowPrereleaseVersions = this._allowPrereleaseVersions || !package.IsReleaseVersion();
                        }
                        base.Walk(package);
                    }
                    finally
                    {
                        this._allowPrereleaseVersions = flag;
                    }
                }
            }
            IEnumerable<IPackage> enumerable = (from p in this._packagesByDependencyOrder select p.Value).Distinct<IPackage>();
            packagesByDependencyOrder = (from p in enumerable
                where Enumerable.Any<IPackage>(packages, q => (p.Id == q.Id) && (p.Version == q.Version))
                select p).ToList<IPackage>();
            this._packagesByDependencyOrder.Clear();
            this._packagesByDependencyOrder = null;
            return this.Operations.Reduce();
        }

        private IPackage SelectDependency(IEnumerable<IPackage> dependencies) => 
            dependencies.SelectDependency(base.DependencyVersion);

        private bool TryUpdate(IEnumerable<IPackage> dependents, ConflictResult conflictResult, IPackage package, out IEnumerable<IPackage> incompatiblePackages)
        {
            bool flag;
            Dictionary<string, IPackage> dependentsLookup = Enumerable.ToDictionary<IPackage, string>(dependents, d => d.Id, StringComparer.OrdinalIgnoreCase);
            Dictionary<IPackage, IPackage> dictionary = new Dictionary<IPackage, IPackage>();
            foreach (IPackage package2 in dependents)
            {
                dictionary[package2] = null;
            }
            foreach (var type in from p in FindCompatiblePackages(this.DependencyResolver, this.ConstraintProvider, dependentsLookup.Keys, package, base.TargetFramework, this.AllowPrereleaseVersions)
                group p by p.Id into g
                let oldPackage = dependentsLookup[g.Key]
                select new { 
                    OldPackage = oldPackage,
                    NewPackage = this.SelectDependency(from p in g
                        where p.Version > oldPackage.Version
                        orderby p.Version
                        select p)
                })
            {
                dictionary[type.OldPackage] = type.NewPackage;
            }
            incompatiblePackages = from p in dictionary
                where p.Value == null
                select p.Key;
            if (incompatiblePackages.Any<IPackage>())
            {
                return false;
            }
            IPackageConstraintProvider constraintProvider = this.ConstraintProvider;
            try
            {
                DefaultConstraintProvider provider2 = new DefaultConstraintProvider();
                provider2.AddConstraint(package.Id, new VersionSpec(package.Version));
                IPackageConstraintProvider[] constraintProviders = new IPackageConstraintProvider[] { this.ConstraintProvider, provider2 };
                this.ConstraintProvider = new AggregateConstraintProvider(constraintProviders);
                base.Marker.MarkVisited(package);
                List<IPackage> list = new List<IPackage>();
                foreach (KeyValuePair<IPackage, IPackage> pair in dictionary)
                {
                    try
                    {
                        this.Uninstall(pair.Key, conflictResult.DependentsResolver, conflictResult.Repository);
                        base.Walk(pair.Value);
                    }
                    catch
                    {
                        list.Add(pair.Key);
                    }
                }
                incompatiblePackages = list;
                flag = !incompatiblePackages.Any<IPackage>();
            }
            finally
            {
                this.ConstraintProvider = constraintProvider;
                base.Marker.MarkProcessing(package);
            }
            return flag;
        }

        private void Uninstall(IPackage package, IDependentsResolver dependentsResolver, IPackageRepository repository)
        {
            this._packagesToKeep.Remove(package);
            if (base.Marker.Contains(package) || !this._operations.Contains(package, PackageAction.Uninstall))
            {
                UninstallWalker walker1 = new UninstallWalker(repository, dependentsResolver, base.TargetFramework, NullLogger.Instance, !this.IgnoreDependencies, false);
                walker1.DisableWalkInfo = this.DisableWalkInfo;
                walker1.ThrowOnConflicts = false;
                foreach (PackageOperation operation in walker1.ResolveOperations(package))
                {
                    if ((operation.Action == PackageAction.Install) || !this._packagesToKeep.Contains(operation.Package))
                    {
                        this._operations.AddOperation(operation);
                    }
                }
            }
        }

        internal bool DisableWalkInfo { get; set; }

        internal bool CheckDowngrade { get; set; }

        protected override bool IgnoreWalkInfo =>
            (this.DisableWalkInfo || base.IgnoreWalkInfo);

        protected ILogger Logger { get; private set; }

        protected IPackageRepository Repository { get; private set; }

        protected override bool IgnoreDependencies =>
            this._ignoreDependencies;

        protected override bool AllowPrereleaseVersions =>
            this._allowPrereleaseVersions;

        protected IDependencyResolver2 DependencyResolver { get; private set; }

        private IPackageConstraintProvider ConstraintProvider { get; set; }

        protected IList<PackageOperation> Operations =>
            this._operations.ToList();

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly InstallWalker.<>c <>9 = new InstallWalker.<>c();
            public static Func<<>f__AnonymousType21<IPackage, PackageDependency>, IPackage> <>9__42_2;
            public static Func<<>f__AnonymousType23<<>f__AnonymousType22<IPackage, PackageDependency>, IVersionSpec>, IPackage> <>9__45_4;
            public static Func<IPackage, string> <>9__46_0;
            public static Func<IPackage, string> <>9__46_1;
            public static Func<IPackage, SemanticVersion> <>9__46_5;
            public static Func<KeyValuePair<IPackage, IPackage>, bool> <>9__46_6;
            public static Func<KeyValuePair<IPackage, IPackage>, IPackage> <>9__46_7;
            public static Func<KeyValuePair<string, IList<IPackage>>, IEnumerable<IPackage>> <>9__51_0;
            public static Func<IPackage, string> <>9__53_0;

            internal string <CreatePackageConflictException>b__53_0(IPackage d) => 
                d.Id;

            internal IPackage <FindCompatiblePackages>b__45_4(<>f__AnonymousType23<<>f__AnonymousType22<IPackage, PackageDependency>, IVersionSpec> <>h__TransparentIdentifier1) => 
                <>h__TransparentIdentifier1.<>h__TransparentIdentifier0.p;

            internal IPackage <OnBeforePackageWalk>b__42_2(<>f__AnonymousType21<IPackage, PackageDependency> <>h__TransparentIdentifier0) => 
                <>h__TransparentIdentifier0.dependentPackage;

            internal IEnumerable<IPackage> <ResolveOperations>b__51_0(KeyValuePair<string, IList<IPackage>> p) => 
                p.Value;

            internal string <TryUpdate>b__46_0(IPackage d) => 
                d.Id;

            internal string <TryUpdate>b__46_1(IPackage p) => 
                p.Id;

            internal SemanticVersion <TryUpdate>b__46_5(IPackage p) => 
                p.Version;

            internal bool <TryUpdate>b__46_6(KeyValuePair<IPackage, IPackage> p) => 
                (p.Value == null);

            internal IPackage <TryUpdate>b__46_7(KeyValuePair<IPackage, IPackage> p) => 
                p.Key;
        }

        private class OperationLookup
        {
            private readonly List<PackageOperation> _operations = new List<PackageOperation>();
            private readonly Dictionary<PackageAction, Dictionary<IPackage, PackageOperation>> _operationLookup = new Dictionary<PackageAction, Dictionary<IPackage, PackageOperation>>();

            internal void AddOperation(PackageOperation operation)
            {
                Dictionary<IPackage, PackageOperation> packageLookup = this.GetPackageLookup(operation.Action, true);
                if (!packageLookup.ContainsKey(operation.Package))
                {
                    packageLookup.Add(operation.Package, operation);
                    this._operations.Add(operation);
                }
            }

            internal void Clear()
            {
                this._operations.Clear();
                this._operationLookup.Clear();
            }

            internal bool Contains(IPackage package, PackageAction action)
            {
                Dictionary<IPackage, PackageOperation> packageLookup = this.GetPackageLookup(action, false);
                return ((packageLookup != null) && packageLookup.ContainsKey(package));
            }

            private Dictionary<IPackage, PackageOperation> GetPackageLookup(PackageAction action, bool createIfNotExists = false)
            {
                Dictionary<IPackage, PackageOperation> dictionary;
                if (!this._operationLookup.TryGetValue(action, out dictionary) & createIfNotExists)
                {
                    dictionary = new Dictionary<IPackage, PackageOperation>((IEqualityComparer<IPackage>) PackageEqualityComparer.IdAndVersion);
                    this._operationLookup.Add(action, dictionary);
                }
                return dictionary;
            }

            internal IEnumerable<IPackage> GetPackages(PackageAction action)
            {
                Dictionary<IPackage, PackageOperation> packageLookup = this.GetPackageLookup(action, false);
                return ((packageLookup == null) ? Enumerable.Empty<IPackage>() : packageLookup.Keys);
            }

            internal void RemoveOperation(IPackage package, PackageAction action)
            {
                PackageOperation operation;
                Dictionary<IPackage, PackageOperation> packageLookup = this.GetPackageLookup(action, false);
                if ((packageLookup != null) && packageLookup.TryGetValue(package, out operation))
                {
                    packageLookup.Remove(package);
                    this._operations.Remove(operation);
                }
            }

            internal IList<PackageOperation> ToList() => 
                this._operations;
        }
    }
}

