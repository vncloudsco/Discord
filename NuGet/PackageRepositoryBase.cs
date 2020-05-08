namespace NuGet
{
    using System;
    using System.Linq;

    internal abstract class PackageRepositoryBase : IPackageRepository
    {
        private PackageSaveModes _packageSave = PackageSaveModes.Nupkg;

        protected PackageRepositoryBase()
        {
        }

        public virtual void AddPackage(IPackage package)
        {
            throw new NotSupportedException();
        }

        public abstract IQueryable<IPackage> GetPackages();
        public virtual void RemovePackage(IPackage package)
        {
            throw new NotSupportedException();
        }

        public abstract string Source { get; }

        public PackageSaveModes PackageSaveMode
        {
            get => 
                this._packageSave;
            set
            {
                if (value == PackageSaveModes.None)
                {
                    throw new ArgumentException("PackageSave cannot be set to None");
                }
                this._packageSave = value;
            }
        }

        public abstract bool SupportsPrereleasePackages { get; }
    }
}

