namespace NuGet
{
    using System;

    internal interface IPackageAssemblyReference : IPackageFile, IFrameworkTargetable
    {
        string Name { get; }
    }
}

