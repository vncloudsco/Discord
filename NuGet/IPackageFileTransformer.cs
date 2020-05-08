namespace NuGet
{
    using System;
    using System.Collections.Generic;

    internal interface IPackageFileTransformer
    {
        void RevertFile(IPackageFile file, string targetPath, IEnumerable<IPackageFile> matchingFiles, IProjectSystem projectSystem);
        void TransformFile(IPackageFile file, string targetPath, IProjectSystem projectSystem);
    }
}

