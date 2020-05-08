namespace NuGet
{
    using System;
    using System.IO;
    using System.Runtime.Versioning;

    internal interface IPackageFile : IFrameworkTargetable
    {
        Stream GetStream();

        string Path { get; }

        string EffectivePath { get; }

        FrameworkName TargetFramework { get; }
    }
}

