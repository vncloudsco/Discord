namespace NuGet
{
    using System;

    internal interface IPackageRepositoryFactory
    {
        IPackageRepository CreateRepository(string packageSource);
    }
}

