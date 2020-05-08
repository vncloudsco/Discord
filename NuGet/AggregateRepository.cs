namespace NuGet
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    internal class AggregateRepository : PackageRepositoryBase, IPackageLookup, IPackageRepository, IDependencyResolver, IServiceBasedRepository, ICloneableRepository, IOperationAwareRepository
    {
        private readonly ConcurrentBag<IPackageRepository> _failingRepositories;
        private readonly IEnumerable<IPackageRepository> _repositories;
        private readonly Lazy<bool> _supportsPrereleasePackages;
        private const string SourceValue = "(Aggregate source)";
        private ILogger _logger;

        public AggregateRepository(IEnumerable<IPackageRepository> repositories)
        {
            this._failingRepositories = new ConcurrentBag<IPackageRepository>();
            if (repositories == null)
            {
                throw new ArgumentNullException("repositories");
            }
            this._repositories = Flatten(repositories);
            Func<IPackageRepository, bool> supportsPrereleasePackages = this.Wrap<bool>(r => r.SupportsPrereleasePackages, true);
            this._supportsPrereleasePackages = new Lazy<bool>(() => Enumerable.All<IPackageRepository>(this._repositories, supportsPrereleasePackages));
            this.IgnoreFailingRepositories = true;
        }

        public AggregateRepository(IPackageRepositoryFactory repositoryFactory, IEnumerable<string> packageSources, bool ignoreFailingRepositories)
        {
            this._failingRepositories = new ConcurrentBag<IPackageRepository>();
            this.IgnoreFailingRepositories = ignoreFailingRepositories;
            Func<string, IPackageRepository> createRepository = new Func<string, IPackageRepository>(repositoryFactory.CreateRepository);
            if (ignoreFailingRepositories)
            {
                createRepository = delegate (string source) {
                    try
                    {
                        return repositoryFactory.CreateRepository(source);
                    }
                    catch
                    {
                        return null;
                    }
                };
            }
            this._repositories = (from source in packageSources
                let repository = createRepository(source)
                where repository != null
                select repository).ToArray<IPackageRepository>();
            Func<IPackageRepository, bool> supportsPrereleasePackages = this.Wrap<bool>(r => r.SupportsPrereleasePackages, true);
            this._supportsPrereleasePackages = new Lazy<bool>(() => Enumerable.All<IPackageRepository>(this._repositories, supportsPrereleasePackages));
        }

        public IPackageRepository Clone() => 
            new AggregateRepository(Enumerable.Select<IPackageRepository, IPackageRepository>(this.Repositories, new Func<IPackageRepository, IPackageRepository>(PackageRepositoryExtensions.Clone)));

        public static IPackageRepository Create(IPackageRepositoryFactory factory, IList<PackageSource> sources, bool ignoreFailingRepositories)
        {
            if (sources.Count == 0)
            {
                return null;
            }
            if (sources.Count == 1)
            {
                return factory.CreateRepository(sources[0].Source);
            }
            Func<string, IPackageRepository> createRepository = new Func<string, IPackageRepository>(factory.CreateRepository);
            if (ignoreFailingRepositories)
            {
                createRepository = delegate (string source) {
                    try
                    {
                        return factory.CreateRepository(source);
                    }
                    catch (InvalidOperationException)
                    {
                        return null;
                    }
                };
            }
            AggregateRepository repository1 = new AggregateRepository(from source in sources
                let repository = createRepository(source.Source)
                where repository != null
                select repository);
            repository1.IgnoreFailingRepositories = ignoreFailingRepositories;
            return repository1;
        }

        private AggregateQuery<IPackage> CreateAggregateQuery(IEnumerable<IQueryable<IPackage>> queries) => 
            new AggregateQuery<IPackage>(queries, (IEqualityComparer<IPackage>) PackageEqualityComparer.IdAndVersion, this.Logger, this.IgnoreFailingRepositories);

        public bool Exists(string packageId, SemanticVersion version)
        {
            Func<IPackageRepository, bool> func = this.Wrap<bool>(r => r.Exists(packageId, version), false);
            return Enumerable.Any<IPackageRepository>(this.Repositories, func);
        }

        public IPackage FindPackage(string packageId, SemanticVersion version)
        {
            Func<IPackageRepository, IPackage> func = this.Wrap<IPackage>(r => r.FindPackage(packageId, version), null);
            return Enumerable.FirstOrDefault<IPackage>(Enumerable.Select<IPackageRepository, IPackage>(this.Repositories, func), p => p != null);
        }

        public IEnumerable<IPackage> FindPackagesById(string packageId)
        {
            Task<IEnumerable<IPackage>>[] tasks = (from p in this._repositories select Task.Factory.StartNew<IEnumerable<IPackage>>(state => p.FindPackagesById(packageId), p)).ToArray<Task<IEnumerable<IPackage>>>();
            try
            {
                Task.WaitAll(tasks);
            }
            catch (AggregateException)
            {
                if (!this.IgnoreFailingRepositories)
                {
                    throw;
                }
            }
            List<IPackage> list = new List<IPackage>();
            foreach (Task<IEnumerable<IPackage>> task in tasks)
            {
                if (task.IsFaulted)
                {
                    this.LogRepository((IPackageRepository) task.AsyncState, task.Exception);
                }
                else if (task.Result != null)
                {
                    list.AddRange(task.Result);
                }
            }
            return list;
        }

        internal static IEnumerable<IPackageRepository> Flatten(IEnumerable<IPackageRepository> repositories) => 
            Enumerable.SelectMany<IPackageRepository, IPackageRepository>(repositories, delegate (IPackageRepository repository) {
                AggregateRepository repository2 = repository as AggregateRepository;
                if (repository2 != null)
                {
                    return repository2.Repositories.ToArray<IPackageRepository>();
                }
                return new IPackageRepository[] { repository };
            });

        public override IQueryable<IPackage> GetPackages()
        {
            IQueryable<IPackage> defaultValue = Enumerable.Empty<IPackage>().AsQueryable<IPackage>();
            Func<IPackageRepository, IQueryable<IPackage>> func = this.Wrap<IQueryable<IPackage>>(r => r.GetPackages(), defaultValue);
            return this.CreateAggregateQuery(Enumerable.Select<IPackageRepository, IQueryable<IPackage>>(this.Repositories, func));
        }

        public IEnumerable<IPackage> GetUpdates(IEnumerable<IPackageName> packages, bool includePrerelease, bool includeAllVersions, IEnumerable<FrameworkName> targetFrameworks, IEnumerable<IVersionSpec> versionConstraints)
        {
            Task<IEnumerable<IPackage>>[] tasks = (from p in this._repositories select Task.Factory.StartNew<IEnumerable<IPackage>>(state => p.GetUpdates(packages, includePrerelease, includeAllVersions, targetFrameworks, versionConstraints), p)).ToArray<Task<IEnumerable<IPackage>>>();
            try
            {
                Task.WaitAll(tasks);
            }
            catch (AggregateException)
            {
                if (!this.IgnoreFailingRepositories)
                {
                    throw;
                }
            }
            HashSet<IPackage> collection = new HashSet<IPackage>((IEqualityComparer<IPackage>) PackageEqualityComparer.IdAndVersion);
            foreach (Task<IEnumerable<IPackage>> task in tasks)
            {
                if (task.IsFaulted)
                {
                    this.LogRepository((IPackageRepository) task.AsyncState, task.Exception);
                }
                else if (task.Result != null)
                {
                    collection.AddRange<IPackage>(task.Result);
                }
            }
            return (!includeAllVersions ? collection.CollapseById() : Enumerable.ThenBy<IPackage, SemanticVersion>(Enumerable.OrderBy<IPackage, string>(collection, p => p.Id, StringComparer.OrdinalIgnoreCase), p => p.Version));
        }

        public void LogRepository(IPackageRepository repository, Exception ex)
        {
            this._failingRepositories.Add(repository);
            this.Logger.Log(MessageLevel.Warning, ExceptionUtility.Unwrap(ex).Message, new object[0]);
        }

        public IPackage ResolveDependency(PackageDependency dependency, IPackageConstraintProvider constraintProvider, bool allowPrereleaseVersions, bool preferListedPackages, DependencyVersion dependencyVersion)
        {
            if (!this.ResolveDependenciesVertically)
            {
                return DependencyResolveUtility.ResolveDependencyCore(this, dependency, constraintProvider, allowPrereleaseVersions, preferListedPackages, dependencyVersion);
            }
            Func<IPackageRepository, IPackage> resolveDependency = this.Wrap<IPackage>(r => DependencyResolveUtility.ResolveDependency(r, dependency, constraintProvider, allowPrereleaseVersions, preferListedPackages, dependencyVersion), null);
            return (from r in this.Repositories select Task.Factory.StartNew<IPackage>(() => resolveDependency(r))).ToArray<Task<IPackage>>().WhenAny<IPackage>(package => (package != null));
        }

        public IQueryable<IPackage> Search(string searchTerm, IEnumerable<string> targetFrameworks, bool allowPrereleaseVersions, bool includeDelisted) => 
            this.CreateAggregateQuery(from r in this.Repositories select r.Search(searchTerm, targetFrameworks, allowPrereleaseVersions, includeDelisted));

        public IDisposable StartOperation(string operation, string mainPackageId, string mainPackageVersion) => 
            DisposableAction.All((IEnumerable<IDisposable>) (from r in this.Repositories select r.StartOperation(operation, mainPackageId, mainPackageVersion)));

        private Func<IPackageRepository, T> Wrap<T>(Func<IPackageRepository, T> factory, T defaultValue = null) => 
            (!this.IgnoreFailingRepositories ? factory : delegate (IPackageRepository repository) {
                if (this._failingRepositories.Contains<IPackageRepository>(repository))
                {
                    return defaultValue;
                }
                try
                {
                    return factory(repository);
                }
                catch (Exception exception)
                {
                    this.LogRepository(repository, exception);
                    return defaultValue;
                }
            });

        public override string Source =>
            "(Aggregate source)";

        public ILogger Logger
        {
            get => 
                (this._logger ?? NullLogger.Instance);
            set => 
                (this._logger = value);
        }

        public bool ResolveDependenciesVertically { get; set; }

        public bool IgnoreFailingRepositories { get; set; }

        public IEnumerable<IPackageRepository> Repositories =>
            this._repositories;

        public override bool SupportsPrereleasePackages =>
            this._supportsPrereleasePackages.Value;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly AggregateRepository.<>c <>9 = new AggregateRepository.<>c();
            public static Func<IPackageRepository, bool> <>9__22_0;
            public static Func<<>f__AnonymousType6<string, IPackageRepository>, bool> <>9__23_2;
            public static Func<<>f__AnonymousType6<string, IPackageRepository>, IPackageRepository> <>9__23_3;
            public static Func<IPackageRepository, bool> <>9__23_4;
            public static Func<IPackageRepository, IQueryable<IPackage>> <>9__24_0;
            public static Func<IPackage, bool> <>9__25_1;
            public static Predicate<IPackage> <>9__27_3;
            public static Func<IPackageRepository, IEnumerable<IPackageRepository>> <>9__33_0;
            public static Func<IPackage, string> <>9__35_2;
            public static Func<IPackage, SemanticVersion> <>9__35_3;
            public static Func<<>f__AnonymousType6<PackageSource, IPackageRepository>, bool> <>9__37_2;
            public static Func<<>f__AnonymousType6<PackageSource, IPackageRepository>, IPackageRepository> <>9__37_3;

            internal bool <.ctor>b__22_0(IPackageRepository r) => 
                r.SupportsPrereleasePackages;

            internal bool <.ctor>b__23_2(<>f__AnonymousType6<string, IPackageRepository> <>h__TransparentIdentifier0) => 
                (<>h__TransparentIdentifier0.repository != null);

            internal IPackageRepository <.ctor>b__23_3(<>f__AnonymousType6<string, IPackageRepository> <>h__TransparentIdentifier0) => 
                <>h__TransparentIdentifier0.repository;

            internal bool <.ctor>b__23_4(IPackageRepository r) => 
                r.SupportsPrereleasePackages;

            internal bool <Create>b__37_2(<>f__AnonymousType6<PackageSource, IPackageRepository> <>h__TransparentIdentifier0) => 
                (<>h__TransparentIdentifier0.repository != null);

            internal IPackageRepository <Create>b__37_3(<>f__AnonymousType6<PackageSource, IPackageRepository> <>h__TransparentIdentifier0) => 
                <>h__TransparentIdentifier0.repository;

            internal bool <FindPackage>b__25_1(IPackage p) => 
                (p != null);

            internal IEnumerable<IPackageRepository> <Flatten>b__33_0(IPackageRepository repository)
            {
                AggregateRepository repository2 = repository as AggregateRepository;
                if (repository2 != null)
                {
                    return repository2.Repositories.ToArray<IPackageRepository>();
                }
                return new IPackageRepository[] { repository };
            }

            internal IQueryable<IPackage> <GetPackages>b__24_0(IPackageRepository r) => 
                r.GetPackages();

            internal string <GetUpdates>b__35_2(IPackage p) => 
                p.Id;

            internal SemanticVersion <GetUpdates>b__35_3(IPackage p) => 
                p.Version;

            internal bool <ResolveDependency>b__27_3(IPackage package) => 
                (package != null);
        }
    }
}

