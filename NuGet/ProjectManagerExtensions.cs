namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;

    internal static class ProjectManagerExtensions
    {
        public static FrameworkName GetTargetFrameworkForPackage(this IProjectManager projectManager, string packageId)
        {
            if (projectManager == null)
            {
                return null;
            }
            FrameworkName packageTargetFramework = null;
            IPackageReferenceRepository localRepository = projectManager.LocalRepository as IPackageReferenceRepository;
            if (localRepository != null)
            {
                packageTargetFramework = localRepository.GetPackageTargetFramework(packageId);
            }
            if ((packageTargetFramework == null) && (projectManager.Project != null))
            {
                packageTargetFramework = projectManager.Project.TargetFramework;
            }
            return packageTargetFramework;
        }
    }
}

