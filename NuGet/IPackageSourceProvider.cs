namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal interface IPackageSourceProvider
    {
        event EventHandler PackageSourcesSaved;

        void DisablePackageSource(PackageSource source);
        bool IsPackageSourceEnabled(PackageSource source);
        IEnumerable<PackageSource> LoadPackageSources();
        void SavePackageSources(IEnumerable<PackageSource> sources);
    }
}

