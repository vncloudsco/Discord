namespace NuGet
{
    using System;
    using System.Collections.Generic;

    internal interface IPackageReferenceRepository2 : IPackageRepository
    {
        PackageReference GetPackageReference(string packageId);
        IEnumerable<PackageReference> GetPackageReferences(string packageId);
    }
}

