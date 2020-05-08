namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class PriorityPackageRepository : PackageRepositoryBase, IPackageLookup, IPackageRepository, IOperationAwareRepository
    {
        private readonly IPackageRepository _primaryRepository;
        private readonly IPackageRepository _secondaryRepository;

        public PriorityPackageRepository(IPackageRepository primaryRepository, IPackageRepository secondaryRepository)
        {
            if (primaryRepository == null)
            {
                throw new ArgumentNullException("primaryRepository");
            }
            if (secondaryRepository == null)
            {
                throw new ArgumentNullException("secondaryRepository");
            }
            this._primaryRepository = primaryRepository;
            this._secondaryRepository = secondaryRepository;
        }

        public bool Exists(string packageId, SemanticVersion version)
        {
            bool flag = this._primaryRepository.Exists(packageId, version);
            if (!flag)
            {
                flag = this._secondaryRepository.Exists(packageId, version);
            }
            return flag;
        }

        public IPackage FindPackage(string packageId, SemanticVersion version) => 
            (this._primaryRepository.FindPackage(packageId, version) ?? this._secondaryRepository.FindPackage(packageId, version));

        public IEnumerable<IPackage> FindPackagesById(string packageId)
        {
            IEnumerable<IPackage> sequence = this._primaryRepository.FindPackagesById(packageId);
            if (sequence.IsEmpty<IPackage>())
            {
                sequence = this._secondaryRepository.FindPackagesById(packageId);
            }
            return sequence.Distinct<IPackage>();
        }

        public override IQueryable<IPackage> GetPackages() => 
            this._primaryRepository.GetPackages();

        public IDisposable StartOperation(string operation, string mainPackageId, string mainPackageVersion)
        {
            IDisposable[] tokens = new IDisposable[] { this._primaryRepository.StartOperation(operation, mainPackageId, mainPackageVersion), this._secondaryRepository.StartOperation(operation, mainPackageId, mainPackageVersion) };
            return DisposableAction.All(tokens);
        }

        internal IPackageRepository PrimaryRepository =>
            this._primaryRepository;

        internal IPackageRepository SecondaryRepository =>
            this._secondaryRepository;

        public override string Source =>
            this._primaryRepository.Source;

        public override bool SupportsPrereleasePackages =>
            this._primaryRepository.SupportsPrereleasePackages;
    }
}

