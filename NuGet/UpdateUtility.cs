namespace NuGet
{
    using NuGet.Resolver;
    using NuGet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal class UpdateUtility
    {
        public UpdateUtility(ActionResolver resolver)
        {
            this.Resolver = resolver;
            this.Logger = NullLogger.Instance;
        }

        private void AddUnsafeUpdateOperation(string id, SemanticVersion version, bool targetVersionSetExplicitly, IProjectManager projectManager)
        {
            IPackage packageMetadata = projectManager.LocalRepository.FindPackage(id);
            if (packageMetadata != null)
            {
                object[] args = new object[] { id };
                this.Logger.Log(MessageLevel.Debug, NuGetResources.Debug_LookingForUpdates, args);
                IPackage package2 = projectManager.PackageManager.SourceRepository.FindPackage(id, version, projectManager.ConstraintProvider, this.AllowPrereleaseVersions, false);
                if ((package2 != null) && ((packageMetadata.Version != package2.Version) && ((this.AllowPrereleaseVersions | targetVersionSetExplicitly) || (packageMetadata.IsReleaseVersion() || (!package2.IsReleaseVersion() || (packageMetadata.Version < package2.Version))))))
                {
                    object[] objArray2 = new object[] { package2.Id, packageMetadata.Version, package2.Version, projectManager.Project.ProjectName };
                    this.Logger.Log(MessageLevel.Info, NuGetResources.Log_UpdatingPackages, objArray2);
                    this.Resolver.AddOperation(PackageAction.Install, package2, projectManager);
                }
                IVersionSpec constraint = projectManager.ConstraintProvider.GetConstraint(package2.Id);
                if (constraint != null)
                {
                    object[] objArray3 = new object[] { package2.Id, VersionUtility.PrettyPrint(constraint), projectManager.ConstraintProvider.Source };
                    this.Logger.Log(MessageLevel.Info, NuGetResources.Log_ApplyingConstraints, objArray3);
                }
                object[] objArray4 = new object[] { package2.Id, projectManager.Project.ProjectName };
                this.Logger.Log(MessageLevel.Info, NuGetResources.Log_NoUpdatesAvailableForProject, objArray4);
            }
        }

        private void AddUpdateOperations(string id, SemanticVersion version, IEnumerable<IProjectManager> projectManagers)
        {
            if (!this.Safe)
            {
                foreach (IProjectManager manager in projectManagers)
                {
                    this.AddUnsafeUpdateOperation(id, version, version != null, manager);
                }
            }
            else
            {
                foreach (IProjectManager manager2 in projectManagers)
                {
                    IPackage package = manager2.LocalRepository.FindPackage(id);
                    if (package != null)
                    {
                        IVersionSpec safeRange = VersionUtility.GetSafeRange(package.Version);
                        IPackage package2 = manager2.PackageManager.SourceRepository.FindPackage(id, safeRange, manager2.ConstraintProvider, this.AllowPrereleaseVersions, false);
                        this.Resolver.AddOperation(PackageAction.Install, package2, manager2);
                    }
                }
            }
        }

        public static Tuple<IPackage, IProjectManager> FindPackageToUpdate(string id, SemanticVersion version, IPackageManager packageManager, IProjectManager projectManager)
        {
            IPackage package = null;
            package = projectManager.LocalRepository.FindPackage(id, null);
            if (package != null)
            {
                return Tuple.Create<IPackage, IProjectManager>(package, projectManager);
            }
            if (version != null)
            {
                package = packageManager.LocalRepository.FindPackage(id, version);
            }
            else
            {
                List<IPackage> source = packageManager.LocalRepository.FindPackagesById(id).ToList<IPackage>();
                if (source.Count > 1)
                {
                    if (!Enumerable.Any<IPackage>(source, p => packageManager.IsProjectLevel(p)))
                    {
                        object[] objArray2 = new object[] { source[0].Id };
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Ambiguous update: {0}", objArray2));
                    }
                    object[] objArray1 = new object[] { source[0].Id, projectManager.Project.ProjectName };
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unknown package in Project {0}: {1}", objArray1));
                }
                package = source.SingleOrDefault<IPackage>();
            }
            if (package == null)
            {
                object[] objArray3 = new object[] { id };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unknown Package: {0}", objArray3));
            }
            if (!packageManager.IsProjectLevel(package))
            {
                return Tuple.Create<IPackage, IProjectManager>(package, null);
            }
            if (version == null)
            {
                object[] objArray4 = new object[] { package.Id, projectManager.Project.ProjectName };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unknown package {0} in project {1}", objArray4));
            }
            object[] args = new object[] { package.GetFullName(), projectManager.Project.ProjectName };
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unknown package {0} in project {1}", args));
        }

        public static Tuple<IPackage, IProjectManager> FindPackageToUpdate(string id, SemanticVersion version, IPackageManager packageManager, IEnumerable<IProjectManager> projectManagers, ILogger logger)
        {
            IPackage package = null;
            Tuple<IPackage, IProjectManager> tuple;
            using (IEnumerator<IProjectManager> enumerator = projectManagers.GetEnumerator())
            {
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        IProjectManager current = enumerator.Current;
                        package = current.LocalRepository.FindPackage(id, null);
                        if (package == null)
                        {
                            continue;
                        }
                        tuple = Tuple.Create<IPackage, IProjectManager>(package, current);
                    }
                    else
                    {
                        if (version != null)
                        {
                            package = packageManager.LocalRepository.FindPackage(id, version);
                        }
                        else
                        {
                            List<IPackage> list = packageManager.LocalRepository.FindPackagesById(id).ToList<IPackage>();
                            foreach (IPackage package2 in list)
                            {
                                if (!packageManager.IsProjectLevel(package2))
                                {
                                    if (list.Count > 1)
                                    {
                                        object[] objArray1 = new object[] { id };
                                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Ambiguous update: {0}", objArray1));
                                    }
                                    package = package2;
                                }
                                else
                                {
                                    if (!packageManager.LocalRepository.IsReferenced(package2.Id, package2.Version))
                                    {
                                        object[] objArray2 = new object[] { package2.Id, package2.Version };
                                        logger.Log(MessageLevel.Warning, string.Format(CultureInfo.CurrentCulture, "Package not referenced by any project {0}, {1}", objArray2), new object[0]);
                                        continue;
                                    }
                                    package = package2;
                                }
                                break;
                            }
                            if (package == null)
                            {
                                object[] objArray3 = new object[] { id };
                                throw new PackageNotInstalledException(string.Format(CultureInfo.CurrentCulture, "Package not installed in any project: {0}", objArray3));
                            }
                        }
                        if (package != null)
                        {
                            return Tuple.Create<IPackage, IProjectManager>(package, null);
                        }
                        object[] args = new object[] { id };
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unknown Package: {0}", args));
                    }
                    break;
                }
            }
            return tuple;
        }

        public IEnumerable<PackageAction> ResolveActionsForUpdate(string id, SemanticVersion version, IEnumerable<IProjectManager> projectManagers, bool projectNameSpecified) => 
            (!string.IsNullOrEmpty(id) ? this.ResolveActionsToUpdateOnePackage(id, version, projectManagers, projectNameSpecified) : this.ResolveActionsToUpdateAllPackages(projectManagers));

        private IEnumerable<PackageAction> ResolveActionsToUpdateAllPackages(IEnumerable<IProjectManager> projectManagers)
        {
            IEnumerable<IPackage> enumerable = new PackageSorter(null).GetPackagesByDependencyOrder(projectManagers.First<IProjectManager>().PackageManager.LocalRepository).Reverse<IPackage>();
            foreach (IProjectManager manager in projectManagers)
            {
                foreach (IPackage package in enumerable)
                {
                    IProjectManager[] managerArray1 = new IProjectManager[] { manager };
                    this.AddUpdateOperations(package.Id, null, managerArray1);
                }
            }
            return this.Resolver.ResolveActions();
        }

        private IEnumerable<PackageAction> ResolveActionsToUpdateOnePackage(string id, SemanticVersion version, IEnumerable<IProjectManager> projectManagers, bool projectNameSpecified)
        {
            IPackageManager packageManager = projectManagers.First<IProjectManager>().PackageManager;
            if ((projectNameSpecified ? FindPackageToUpdate(id, version, packageManager, projectManagers.First<IProjectManager>()) : FindPackageToUpdate(id, version, packageManager, projectManagers, this.Logger)).Item2 != null)
            {
                this.AddUpdateOperations(id, version, projectManagers);
            }
            else
            {
                IPackage package = packageManager.SourceRepository.FindPackage(id, version, this.AllowPrereleaseVersions, false);
                if (package == null)
                {
                    object[] args = new object[] { id };
                    this.Logger.Log(MessageLevel.Info, "No updates available for {0}", args);
                    return Enumerable.Empty<PackageAction>();
                }
                this.Resolver.AddOperation(PackageAction.Update, package, new NullProjectManager(packageManager));
            }
            return this.Resolver.ResolveActions();
        }

        public ActionResolver Resolver { get; private set; }

        public bool Safe { get; set; }

        public ILogger Logger { get; set; }

        public bool AllowPrereleaseVersions { get; set; }
    }
}

