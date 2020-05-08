namespace NuGet
{
    using System;
    using System.Collections.Generic;

    internal interface IPackageMetadata : IPackageName
    {
        string Title { get; }

        IEnumerable<string> Authors { get; }

        IEnumerable<string> Owners { get; }

        Uri IconUrl { get; }

        Uri LicenseUrl { get; }

        Uri ProjectUrl { get; }

        bool RequireLicenseAcceptance { get; }

        bool DevelopmentDependency { get; }

        string Description { get; }

        string Summary { get; }

        string ReleaseNotes { get; }

        string Language { get; }

        string Tags { get; }

        string Copyright { get; }

        IEnumerable<FrameworkAssemblyReference> FrameworkAssemblies { get; }

        ICollection<PackageReferenceSet> PackageAssemblyReferences { get; }

        IEnumerable<PackageDependencySet> DependencySets { get; }

        Version MinClientVersion { get; }
    }
}

