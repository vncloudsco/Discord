namespace NuGet
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;

    internal interface IPackageBuilder : IPackageMetadata, IPackageName
    {
        void Save(Stream stream);

        Collection<IPackageFile> Files { get; }
    }
}

