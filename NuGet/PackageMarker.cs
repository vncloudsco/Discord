namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal sealed class PackageMarker : IPackageRepository, IDependentsResolver, IPackageLookup
    {
        private readonly Dictionary<string, Dictionary<IPackage, VisitedState>> _visited = new Dictionary<string, Dictionary<IPackage, VisitedState>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<IPackage, HashSet<IPackage>> _dependents = new Dictionary<IPackage, HashSet<IPackage>>((IEqualityComparer<IPackage>) PackageEqualityComparer.IdAndVersion);

        public void AddDependent(IPackage package, IPackage dependency)
        {
            HashSet<IPackage> set;
            if (!this._dependents.TryGetValue(dependency, out set))
            {
                set = new HashSet<IPackage>((IEqualityComparer<IPackage>) PackageEqualityComparer.IdAndVersion);
                this._dependents.Add(dependency, set);
            }
            set.Add(package);
        }

        public void Clear()
        {
            this._visited.Clear();
            this._dependents.Clear();
        }

        public bool Contains(IPackage package)
        {
            Dictionary<IPackage, VisitedState> lookup = this.GetLookup(package.Id, true);
            return ((lookup != null) && lookup.ContainsKey(package));
        }

        public bool Exists(string packageId, SemanticVersion version) => 
            (this.FindPackage(packageId, version) != null);

        public IPackage FindPackage(string packageId, SemanticVersion version) => 
            (from p in this.FindPackagesById(packageId)
                where p.Version.Equals(version)
                select p).FirstOrDefault<IPackage>();

        public IEnumerable<IPackage> FindPackagesById(string packageId)
        {
            Dictionary<IPackage, VisitedState> packages = this.GetLookup(packageId, false);
            return ((packages == null) ? Enumerable.Empty<IPackage>() : (from p in packages.Keys
                where ((VisitedState) packages[p]) == VisitedState.Completed
                select p));
        }

        private Dictionary<IPackage, VisitedState> GetLookup(string packageId, bool createEntry = false)
        {
            Dictionary<IPackage, VisitedState> dictionary;
            if (!this._visited.TryGetValue(packageId, out dictionary) && createEntry)
            {
                dictionary = new Dictionary<IPackage, VisitedState>((IEqualityComparer<IPackage>) PackageEqualityComparer.IdAndVersion);
                this._visited[packageId] = dictionary;
            }
            return dictionary;
        }

        public bool IsCycle(IPackage package)
        {
            VisitedState state;
            Dictionary<IPackage, VisitedState> lookup = this.GetLookup(package.Id, false);
            return ((lookup != null) && (lookup.TryGetValue(package, out state) && (state == VisitedState.Processing)));
        }

        public bool IsVersionCycle(string packageId)
        {
            Dictionary<IPackage, VisitedState> lookup = this.GetLookup(packageId, false);
            return ((lookup != null) && Enumerable.Any<VisitedState>(lookup.Values, state => state == VisitedState.Processing));
        }

        public bool IsVisited(IPackage package)
        {
            VisitedState state;
            Dictionary<IPackage, VisitedState> lookup = this.GetLookup(package.Id, false);
            return ((lookup != null) && (lookup.TryGetValue(package, out state) && (state == VisitedState.Completed)));
        }

        public void MarkProcessing(IPackage package)
        {
            this.GetLookup(package.Id, true)[package] = VisitedState.Processing;
        }

        public void MarkVisited(IPackage package)
        {
            this.GetLookup(package.Id, true)[package] = VisitedState.Completed;
        }

        IEnumerable<IPackage> IDependentsResolver.GetDependents(IPackage package)
        {
            HashSet<IPackage> set;
            return (!this._dependents.TryGetValue(package, out set) ? Enumerable.Empty<IPackage>() : set);
        }

        void IPackageRepository.AddPackage(IPackage package)
        {
            throw new NotSupportedException();
        }

        IQueryable<IPackage> IPackageRepository.GetPackages() => 
            Enumerable.Where<IPackage>(this.Packages, new Func<IPackage, bool>(this.IsVisited)).AsQueryable<IPackage>();

        void IPackageRepository.RemovePackage(IPackage package)
        {
            throw new NotSupportedException();
        }

        public string Source =>
            string.Empty;

        public PackageSaveModes PackageSaveMode
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public bool SupportsPrereleasePackages =>
            true;

        public IEnumerable<IPackage> Packages =>
            (from p in this._visited.Values select p.Keys);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PackageMarker.<>c <>9 = new PackageMarker.<>c();
            public static Func<Dictionary<IPackage, PackageMarker.VisitedState>, IEnumerable<IPackage>> <>9__10_0;
            public static Func<PackageMarker.VisitedState, bool> <>9__14_0;

            internal IEnumerable<IPackage> <get_Packages>b__10_0(Dictionary<IPackage, PackageMarker.VisitedState> p) => 
                p.Keys;

            internal bool <IsVersionCycle>b__14_0(PackageMarker.VisitedState state) => 
                (state == PackageMarker.VisitedState.Processing);
        }

        internal enum VisitedState
        {
            Processing,
            Completed
        }
    }
}

