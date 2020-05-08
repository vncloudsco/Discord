namespace NuGet.V3Interop
{
    using NuGet;

    internal interface IV3PackageMetadata : IPackageMetadata, IPackageName
    {
        PackageTargets PackageTarget { get; }
    }
}

