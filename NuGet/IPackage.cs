namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal interface IPackage : IPackageMetadata, IPackageName, IServerPackageMetadata
    {
        IEnumerable<IPackageFile> GetFiles();
        Stream GetStream();
        IEnumerable<FrameworkName> GetSupportedFrameworks();

        bool IsAbsoluteLatestVersion { get; }

        bool IsLatestVersion { get; }

        bool Listed { get; }

        DateTimeOffset? Published { get; }

        IEnumerable<IPackageAssemblyReference> AssemblyReferences { get; }
    }
}

