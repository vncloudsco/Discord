namespace NuGet
{
    using System.Collections.Generic;

    internal interface IPackageOperationResolver
    {
        IEnumerable<PackageOperation> ResolveOperations(IPackage package);
    }
}

