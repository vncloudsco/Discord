namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;

    internal class ConflictResult
    {
        public ConflictResult(IPackage conflictingPackage, IPackageRepository repository, IDependentsResolver resolver)
        {
            this.Package = conflictingPackage;
            this.Repository = repository;
            this.DependentsResolver = resolver;
        }

        public IPackage Package { get; private set; }

        public IPackageRepository Repository { get; private set; }

        public IDependentsResolver DependentsResolver { get; private set; }
    }
}

