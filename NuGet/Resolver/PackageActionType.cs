namespace NuGet.Resolver
{
    using System;

    internal enum PackageActionType
    {
        Install,
        Uninstall,
        AddToPackagesFolder,
        DeleteFromPackagesFolder
    }
}

