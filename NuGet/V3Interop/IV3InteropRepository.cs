namespace NuGet.V3Interop
{
    using System;
    using System.Collections.Generic;

    internal interface IV3InteropRepository
    {
        IEnumerable<IPackage> FindPackagesById(string packageId);
    }
}

