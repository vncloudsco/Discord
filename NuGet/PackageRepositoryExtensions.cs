namespace NuGet
{
    using NuGet.Resources;
    using NuGet.V3Interop;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;

    internal static class PackageRepositoryExtensions
    {
        public static IPackageRepository Clone(this IPackageRepository repository)
        {
            ICloneableRepository repository2 = repository as ICloneableRepository;
            return ((repository2 == null) ? repository : repository2.Clone());
        }

        public static bool Exists(this IPackageRepository repository, IPackageName package) => 
            repository.Exists(package.Id, package.Version);

        public static bool Exists(this IPackageRepository repository, string packageId) => 
            repository.Exists(packageId, null);

        public static bool Exists(this IPackageRepository repository, string packageId, SemanticVersion version)
        {
            IPackageLookup lookup = repository as IPackageLookup;
            return (((lookup == null) || (string.IsNullOrEmpty(packageId) || (version == null))) ? (repository.FindPackage(packageId, version) != null) : lookup.Exists(packageId, version));
        }

        public static PackageDependency FindDependency(this IPackageMetadata package, string packageId, FrameworkName targetFramework) => 
            (from dependency in package.GetCompatiblePackageDependencies(targetFramework)
                where dependency.Id.Equals(packageId, StringComparison.OrdinalIgnoreCase)
                select dependency).FirstOrDefault<PackageDependency>();

        public static IPackage FindPackage(this IPackageRepository repository, string packageId) => 
            repository.FindPackage(packageId, null);

        public static IPackage FindPackage(this IPackageRepository repository, string packageId, SemanticVersion version) => 
            repository.FindPackage(packageId, version, NullConstraintProvider.Instance, true, true);

        public static IPackage FindPackage(this IPackageRepository repository, string packageId, IVersionSpec versionSpec, bool allowPrereleaseVersions, bool allowUnlisted) => 
            repository.FindPackages(packageId, versionSpec, allowPrereleaseVersions, allowUnlisted).FirstOrDefault<IPackage>();

        public static IPackage FindPackage(this IPackageRepository repository, string packageId, SemanticVersion version, bool allowPrereleaseVersions, bool allowUnlisted) => 
            repository.FindPackage(packageId, version, NullConstraintProvider.Instance, allowPrereleaseVersions, allowUnlisted);

        public static IPackage FindPackage(this IPackageRepository repository, string packageId, IVersionSpec versionSpec, IPackageConstraintProvider constraintProvider, bool allowPrereleaseVersions, bool allowUnlisted)
        {
            IEnumerable<IPackage> packages = repository.FindPackages(packageId, versionSpec, allowPrereleaseVersions, allowUnlisted);
            if (constraintProvider != null)
            {
                packages = DependencyResolveUtility.FilterPackagesByConstraints(constraintProvider, packages, packageId, allowPrereleaseVersions);
            }
            return packages.FirstOrDefault<IPackage>();
        }

        public static IPackage FindPackage(this IPackageRepository repository, string packageId, SemanticVersion version, IPackageConstraintProvider constraintProvider, bool allowPrereleaseVersions, bool allowUnlisted)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            if (packageId == null)
            {
                throw new ArgumentNullException("packageId");
            }
            if (version != null)
            {
                allowUnlisted = true;
            }
            else if (!allowUnlisted && ((constraintProvider == null) || ReferenceEquals(constraintProvider, NullConstraintProvider.Instance)))
            {
                IPackage package;
                ILatestPackageLookup lookup2 = repository as ILatestPackageLookup;
                if ((lookup2 != null) && lookup2.TryFindLatestPackageById(packageId, allowPrereleaseVersions, out package))
                {
                    return package;
                }
            }
            IPackageLookup lookup = repository as IPackageLookup;
            if ((lookup != null) && (version != null))
            {
                return lookup.FindPackage(packageId, version);
            }
            IEnumerable<IPackage> packages = from p in repository.FindPackagesById(packageId).ToList<IPackage>()
                orderby p.Version descending
                select p;
            if (!allowUnlisted)
            {
                packages = Enumerable.Where<IPackage>(packages, new Func<IPackage, bool>(PackageExtensions.IsListed));
            }
            if (version != null)
            {
                packages = from p in packages
                    where p.Version == version
                    select p;
            }
            else if (constraintProvider != null)
            {
                packages = DependencyResolveUtility.FilterPackagesByConstraints(constraintProvider, packages, packageId, allowPrereleaseVersions);
            }
            return packages.FirstOrDefault<IPackage>();
        }

        public static IEnumerable<IPackage> FindPackages(this IPackageRepository repository, IEnumerable<string> packageIds)
        {
            if (packageIds == null)
            {
                throw new ArgumentNullException("packageIds");
            }
            IV3InteropRepository v3Repo = repository as IV3InteropRepository;
            return ((v3Repo == null) ? repository.FindPackages<string>(packageIds, new Func<IEnumerable<string>, Expression<Func<IPackage, bool>>>(PackageRepositoryExtensions.GetFilterExpression)) : (from id in packageIds select v3Repo.FindPackagesById(id)).ToList<IPackage>());
        }

        [IteratorStateMachine(typeof(<FindPackages>d__13))]
        private static IEnumerable<IPackage> FindPackages<T>(this IPackageRepository repository, IEnumerable<T> items, Func<IEnumerable<T>, Expression<Func<IPackage, bool>>> filterSelector)
        {
            <FindPackages>d__13<T> d__1 = new <FindPackages>d__13<T>(-2);
            d__1.<>3__repository = repository;
            d__1.<>3__items = items;
            d__1.<>3__filterSelector = filterSelector;
            return d__1;
        }

        public static IEnumerable<IPackage> FindPackages(this IPackageRepository repository, string packageId, IVersionSpec versionSpec, bool allowPrereleaseVersions, bool allowUnlisted)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            if (packageId == null)
            {
                throw new ArgumentNullException("packageId");
            }
            IEnumerable<IPackage> source = from p in repository.FindPackagesById(packageId)
                orderby p.Version descending
                select p;
            if (!allowUnlisted)
            {
                source = Enumerable.Where<IPackage>(source, new Func<IPackage, bool>(PackageExtensions.IsListed));
            }
            if (versionSpec != null)
            {
                source = source.FindByVersion(versionSpec);
            }
            return DependencyResolveUtility.FilterPackagesByConstraints(NullConstraintProvider.Instance, source, packageId, allowPrereleaseVersions);
        }

        public static IEnumerable<IPackage> FindPackagesById(this IPackageRepository repository, string packageId)
        {
            IV3InteropRepository repository2 = repository as IV3InteropRepository;
            if (repository2 != null)
            {
                return repository2.FindPackagesById(packageId);
            }
            IPackageLookup lookup = repository as IPackageLookup;
            return ((lookup == null) ? FindPackagesByIdCore(repository, packageId) : lookup.FindPackagesById(packageId).ToList<IPackage>());
        }

        internal static IEnumerable<IPackage> FindPackagesByIdCore(IPackageRepository repository, string packageId)
        {
            <>c__DisplayClass12_0 class_;
            ICultureAwareRepository repository2 = repository as ICultureAwareRepository;
            packageId = (repository2 == null) ? packageId.ToLower(CultureInfo.CurrentCulture) : packageId.ToLower(repository2.Culture);
            ParameterExpression expression = Expression.Parameter(typeof(IPackage), "p");
            ParameterExpression[] parameters = new ParameterExpression[] { expression };
            IQueryable<IPackage> queryable1 = Queryable.Where<IPackage>(repository.GetPackages(), Expression.Lambda<Func<IPackage, bool>>(Expression.Equal(Expression.Call(Expression.Property(expression, (MethodInfo) methodof(IPackageName.get_Id)), (MethodInfo) methodof(string.ToLower), new Expression[0]), Expression.Field(Expression.Constant(class_, typeof(<>c__DisplayClass12_0)), fieldof(<>c__DisplayClass12_0.packageId))), parameters));
            expression = Expression.Parameter(typeof(IPackage), "p");
            ParameterExpression[] expressionArray2 = new ParameterExpression[] { expression };
            return Queryable.OrderBy<IPackage, string>(queryable1, Expression.Lambda<Func<IPackage, string>>(Expression.Property(expression, (MethodInfo) methodof(IPackageName.get_Id)), expressionArray2)).ToList<IPackage>();
        }

        private static Expression GetCompareExpression(Expression parameterExpression, object value) => 
            Expression.Equal(Expression.Call(Expression.Property(parameterExpression, "Id"), typeof(string).GetMethod("ToLower", Type.EmptyTypes)), Expression.Constant(value));

        private static Expression<Func<IPackage, bool>> GetFilterExpression(IEnumerable<IPackageName> packages) => 
            GetFilterExpression((IEnumerable<string>) (from p in packages select p.Id));

        private static Expression<Func<IPackage, bool>> GetFilterExpression(IEnumerable<string> ids)
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(IPackageName));
            ParameterExpression[] parameters = new ParameterExpression[] { parameterExpression };
            return Expression.Lambda<Func<IPackage, bool>>(Enumerable.Aggregate<Expression>(from id in ids select GetCompareExpression(parameterExpression, id.ToLower()), new Func<Expression, Expression, Expression>(Expression.OrElse)), parameters);
        }

        private static IEnumerable<IPackage> GetUpdateCandidates(IPackageRepository repository, IEnumerable<IPackageName> packages, bool includePrerelease)
        {
            IEnumerable<IPackage> enumerable = repository.FindPackages<IPackageName>(packages, new Func<IEnumerable<IPackageName>, Expression<Func<IPackage, bool>>>(PackageRepositoryExtensions.GetFilterExpression));
            if (!includePrerelease)
            {
                enumerable = from p in enumerable
                    where p.IsReleaseVersion()
                    select p;
            }
            return Enumerable.Where<IPackage>(enumerable, new Func<IPackage, bool>(PackageExtensions.IsListed));
        }

        public static IEnumerable<IPackage> GetUpdates(this IPackageRepository repository, IEnumerable<IPackageName> packages, bool includePrerelease, bool includeAllVersions, IEnumerable<FrameworkName> targetFrameworks = null, IEnumerable<IVersionSpec> versionConstraints = null)
        {
            if (packages.IsEmpty<IPackageName>())
            {
                return Enumerable.Empty<IPackage>();
            }
            IServiceBasedRepository repository2 = repository as IServiceBasedRepository;
            return ((repository2 != null) ? repository2.GetUpdates(packages, includePrerelease, includeAllVersions, targetFrameworks, versionConstraints) : repository.GetUpdatesCore(packages, includePrerelease, includeAllVersions, targetFrameworks, versionConstraints));
        }

        public static IEnumerable<IPackage> GetUpdatesCore(this IPackageRepository repository, IEnumerable<IPackageName> packages, bool includePrerelease, bool includeAllVersions, IEnumerable<FrameworkName> targetFramework, IEnumerable<IVersionSpec> versionConstraints)
        {
            List<IPackageName> source = packages.ToList<IPackageName>();
            if (!source.Any<IPackageName>())
            {
                return Enumerable.Empty<IPackage>();
            }
            IList<IVersionSpec> list2 = (versionConstraints != null) ? ((IList<IVersionSpec>) versionConstraints.ToList<IVersionSpec>()) : ((IList<IVersionSpec>) new IVersionSpec[source.Count]);
            if (source.Count != list2.Count)
            {
                throw new ArgumentException(NuGetResources.GetUpdatesParameterMismatch);
            }
            ILookup<string, IPackage> lookup = Enumerable.ToLookup<IPackage, string>(GetUpdateCandidates(repository, source, includePrerelease).ToList<IPackage>(), package => package.Id, StringComparer.OrdinalIgnoreCase);
            List<IPackage> list3 = new List<IPackage>();
            for (int i = 0; i < source.Count; i++)
            {
                IPackageName package = source[i];
                IVersionSpec constraint = list2[i];
                IEnumerable<IPackage> collection = from candidate in lookup[package.Id]
                    where (candidate.Version > package.Version) && (SupportsTargetFrameworks(targetFramework, candidate) && ((constraint == null) || constraint.Satisfies(candidate.Version)))
                    select candidate;
                list3.AddRange(collection);
            }
            return (includeAllVersions ? list3 : list3.CollapseById());
        }

        public static IQueryable<IPackage> Search(this IPackageRepository repository, string searchTerm, bool allowPrereleaseVersions) => 
            repository.Search(searchTerm, Enumerable.Empty<string>(), allowPrereleaseVersions, false);

        public static IQueryable<IPackage> Search(this IPackageRepository repository, string searchTerm, IEnumerable<string> targetFrameworks, bool allowPrereleaseVersions, bool includeDelisted = false)
        {
            if (targetFrameworks == null)
            {
                throw new ArgumentNullException("targetFrameworks");
            }
            IServiceBasedRepository repository2 = repository as IServiceBasedRepository;
            if (repository2 != null)
            {
                return repository2.Search(searchTerm, targetFrameworks, allowPrereleaseVersions, includeDelisted);
            }
            IEnumerable<IPackage> source = repository.GetPackages().Find<IPackage>(searchTerm).FilterByPrerelease(allowPrereleaseVersions);
            if (!includeDelisted)
            {
                source = from p in source
                    where p.IsListed()
                    select p;
            }
            return source.AsQueryable<IPackage>();
        }

        internal static IPackage SelectDependency(this IEnumerable<IPackage> packages, DependencyVersion dependencyVersion)
        {
            if ((packages == null) || !packages.Any<IPackage>())
            {
                return null;
            }
            if (dependencyVersion == DependencyVersion.Lowest)
            {
                return packages.FirstOrDefault<IPackage>();
            }
            if (dependencyVersion == DependencyVersion.Highest)
            {
                return packages.LastOrDefault<IPackage>();
            }
            if (dependencyVersion == DependencyVersion.HighestPatch)
            {
                return (from p in (from p in packages
                    group p by new { 
                        Major = p.Version.Version.Major,
                        Minor = p.Version.Version.Minor
                    } into g
                    orderby g.Key.Major, g.Key.Minor
                    select g).First()
                    orderby p.Version descending
                    select p).FirstOrDefault<IPackage>();
            }
            if (dependencyVersion != DependencyVersion.HighestMinor)
            {
                throw new ArgumentOutOfRangeException("dependencyVersion");
            }
            return (from p in (from p in packages
                group p by new { Major = p.Version.Version.Major } into g
                orderby g.Key.Major
                select g).First()
                orderby p.Version descending
                select p).FirstOrDefault<IPackage>();
        }

        public static IDisposable StartOperation(this IPackageRepository self, string operation, string mainPackageId, string mainPackageVersion)
        {
            IOperationAwareRepository repository = self as IOperationAwareRepository;
            return ((repository == null) ? DisposableAction.NoOp : repository.StartOperation(operation, mainPackageId, mainPackageVersion));
        }

        private static bool SupportsTargetFrameworks(IEnumerable<FrameworkName> targetFramework, IPackage package) => 
            (targetFramework.IsEmpty<FrameworkName>() || Enumerable.Any<FrameworkName>(targetFramework, (Func<FrameworkName, bool>) (t => VersionUtility.IsCompatible(t, package.GetSupportedFrameworks()))));

        public static bool TryFindPackage(this IPackageRepository repository, string packageId, SemanticVersion version, out IPackage package)
        {
            package = repository.FindPackage(packageId, version);
            return (package != null);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PackageRepositoryExtensions.<>c <>9 = new PackageRepositoryExtensions.<>c();
            public static Func<IPackage, SemanticVersion> <>9__8_0;
            public static Func<IPackage, SemanticVersion> <>9__14_0;
            public static Func<IPackage, bool> <>9__18_0;
            public static Func<IPackage, string> <>9__20_0;
            public static Func<IPackage, bool> <>9__23_0;
            public static Func<IPackageName, string> <>9__24_0;
            public static Func<IPackage, <>f__AnonymousType26<int, int>> <>9__27_0;
            public static Func<IGrouping<<>f__AnonymousType26<int, int>, IPackage>, int> <>9__27_1;
            public static Func<IGrouping<<>f__AnonymousType26<int, int>, IPackage>, int> <>9__27_2;
            public static Func<IPackage, SemanticVersion> <>9__27_3;
            public static Func<IPackage, <>f__AnonymousType27<int>> <>9__27_4;
            public static Func<IGrouping<<>f__AnonymousType27<int>, IPackage>, int> <>9__27_5;
            public static Func<IPackage, SemanticVersion> <>9__27_6;

            internal SemanticVersion <FindPackage>b__8_0(IPackage p) => 
                p.Version;

            internal SemanticVersion <FindPackages>b__14_0(IPackage p) => 
                p.Version;

            internal string <GetFilterExpression>b__24_0(IPackageName p) => 
                p.Id;

            internal bool <GetUpdateCandidates>b__23_0(IPackage p) => 
                p.IsReleaseVersion();

            internal string <GetUpdatesCore>b__20_0(IPackage package) => 
                package.Id;

            internal bool <Search>b__18_0(IPackage p) => 
                p.IsListed();

            internal <>f__AnonymousType26<int, int> <SelectDependency>b__27_0(IPackage p) => 
                new { 
                    Major = p.Version.Version.Major,
                    Minor = p.Version.Version.Minor
                };

            internal int <SelectDependency>b__27_1(IGrouping<<>f__AnonymousType26<int, int>, IPackage> g) => 
                g.Key.Major;

            internal int <SelectDependency>b__27_2(IGrouping<<>f__AnonymousType26<int, int>, IPackage> g) => 
                g.Key.Minor;

            internal SemanticVersion <SelectDependency>b__27_3(IPackage p) => 
                p.Version;

            internal <>f__AnonymousType27<int> <SelectDependency>b__27_4(IPackage p) => 
                new { Major = p.Version.Version.Major };

            internal int <SelectDependency>b__27_5(IGrouping<<>f__AnonymousType27<int>, IPackage> g) => 
                g.Key.Major;

            internal SemanticVersion <SelectDependency>b__27_6(IPackage p) => 
                p.Version;
        }

        [CompilerGenerated]
        private sealed class <FindPackages>d__13<T> : IEnumerable<IPackage>, IEnumerable, IEnumerator<IPackage>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private IPackage <>2__current;
            private int <>l__initialThreadId;
            private IEnumerable<T> items;
            public IEnumerable<T> <>3__items;
            private Func<IEnumerable<T>, Expression<Func<IPackage, bool>>> filterSelector;
            public Func<IEnumerable<T>, Expression<Func<IPackage, bool>>> <>3__filterSelector;
            private IPackageRepository repository;
            public IPackageRepository <>3__repository;
            private IEnumerator<IPackage> <>7__wrap1;

            [DebuggerHidden]
            public <FindPackages>d__13(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap1 != null)
                {
                    this.<>7__wrap1.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    int num = this.<>1__state;
                    if (num == 0)
                    {
                        this.<>1__state = -1;
                        goto TR_0005;
                    }
                    else if (num == 1)
                    {
                        this.<>1__state = -3;
                    }
                    else
                    {
                        return false;
                    }
                    goto TR_0009;
                TR_0005:
                    if (this.items.Any<T>())
                    {
                        IEnumerable<T> arg = this.items.Take<T>(10);
                        Expression<Func<IPackage, bool>> expression = this.filterSelector(arg);
                        ParameterExpression expression2 = Expression.Parameter(typeof(IPackage), "p");
                        ParameterExpression[] parameters = new ParameterExpression[] { expression2 };
                        this.<>7__wrap1 = Queryable.OrderBy<IPackage, string>(Queryable.Where<IPackage>(this.repository.GetPackages(), expression), Expression.Lambda<Func<IPackage, string>>(Expression.Property(expression2, (MethodInfo) methodof(IPackageName.get_Id)), parameters)).GetEnumerator();
                        this.<>1__state = -3;
                    }
                    else
                    {
                        return false;
                    }
                TR_0009:
                    while (true)
                    {
                        if (this.<>7__wrap1.MoveNext())
                        {
                            IPackage current = this.<>7__wrap1.Current;
                            this.<>2__current = current;
                            this.<>1__state = 1;
                            flag = true;
                        }
                        else
                        {
                            this.<>m__Finally1();
                            this.<>7__wrap1 = null;
                            this.items = this.items.Skip<T>(10);
                            goto TR_0005;
                        }
                        break;
                    }
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<IPackage> IEnumerable<IPackage>.GetEnumerator()
            {
                PackageRepositoryExtensions.<FindPackages>d__13<T> d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new PackageRepositoryExtensions.<FindPackages>d__13<T>(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = (PackageRepositoryExtensions.<FindPackages>d__13<T>) this;
                }
                d__.repository = this.<>3__repository;
                d__.items = this.<>3__items;
                d__.filterSelector = this.<>3__filterSelector;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<NuGet.IPackage>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                int num = this.<>1__state;
                if ((num == -3) || (num == 1))
                {
                    try
                    {
                    }
                    finally
                    {
                        this.<>m__Finally1();
                    }
                }
            }

            IPackage IEnumerator<IPackage>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

