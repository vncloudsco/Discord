namespace NuGet.Resolver
{
    using Microsoft.VisualStudio.ProjectSystem.Interop;
    using NuGet;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;

    internal class ActionResolver
    {
        private List<Operation> _operations;
        private Dictionary<IProjectManager, VirtualRepository> _virtualProjectRepos;
        private Dictionary<IPackageManager, VirtualRepository> _virtualPackageRepos;
        private Dictionary<IPackageManager, Dictionary<IPackage, int>> _packageRefCounts;

        public ActionResolver()
        {
            this.Logger = NullLogger.Instance;
            this.DependencyVersion = NuGet.DependencyVersion.Lowest;
            this._operations = new List<Operation>();
        }

        public void AddOperation(PackageAction operationType, IPackage package, IProjectManager projectManager)
        {
            Operation item = new Operation();
            item.OperationType = operationType;
            item.Package = package;
            item.ProjectManager = projectManager;
            this._operations.Add(item);
        }

        private void InitializeRefCount()
        {
            this._packageRefCounts = new Dictionary<IPackageManager, Dictionary<IPackage, int>>();
            foreach (IPackageManager manager in from op in this._operations select op.ProjectManager.PackageManager)
            {
                Dictionary<IPackage, int> dictionary = new Dictionary<IPackage, int>((IEqualityComparer<IPackage>) PackageEqualityComparer.IdAndVersion);
                foreach (IPackage package in manager.LocalRepository.GetPackages())
                {
                    dictionary[package] = 0;
                }
                using (IEnumerator<IPackageRepository> enumerator3 = manager.LocalRepository.LoadProjectRepositories().GetEnumerator())
                {
                    while (enumerator3.MoveNext())
                    {
                        foreach (IPackage package2 in enumerator3.Current.GetPackages())
                        {
                            if (!dictionary.ContainsKey(package2))
                            {
                                dictionary[package2] = 1;
                                continue;
                            }
                            IPackage package3 = package2;
                            dictionary[package3] += 1;
                        }
                    }
                }
                foreach (IPackage package4 in manager.LocalRepository.GetPackages())
                {
                    if (dictionary[package4] == 0)
                    {
                        dictionary[package4] = 1;
                    }
                }
                this._packageRefCounts[manager] = dictionary;
            }
        }

        private void InitilizeVirtualRepos()
        {
            this._virtualProjectRepos = new Dictionary<IProjectManager, VirtualRepository>();
            this._virtualPackageRepos = new Dictionary<IPackageManager, VirtualRepository>();
            foreach (Operation operation in this._operations)
            {
                if (!this._virtualProjectRepos.ContainsKey(operation.ProjectManager))
                {
                    this._virtualProjectRepos.Add(operation.ProjectManager, new VirtualRepository(operation.ProjectManager.LocalRepository));
                }
                IPackageManager packageManager = operation.ProjectManager.PackageManager;
                if (!this._virtualPackageRepos.ContainsKey(packageManager))
                {
                    this._virtualPackageRepos.Add(packageManager, new VirtualRepository(packageManager.LocalRepository));
                }
            }
        }

        public IEnumerable<PackageAction> ResolveActions()
        {
            this.InitilizeVirtualRepos();
            this.InitializeRefCount();
            List<PackageAction> list = new List<PackageAction>();
            foreach (Operation operation in this._operations)
            {
                list.AddRange(this.ResolveActionsForOperation(operation));
            }
            return list;
        }

        private IEnumerable<PackageAction> ResolveActionsForOperation(Operation operation)
        {
            IEnumerable<PackageOperation> enumerable = Enumerable.Empty<PackageOperation>();
            if (operation.ProjectManager.Project is INuGetPackageManager)
            {
                PackageActionType actionType = (operation.OperationType == PackageAction.Install) ? PackageActionType.Install : PackageActionType.Uninstall;
                return new PackageProjectAction[] { new PackageProjectAction(actionType, operation.Package, operation.ProjectManager) };
            }
            bool flag = operation.ProjectManager.PackageManager.IsProjectLevel(operation.Package);
            enumerable = (operation.OperationType != PackageAction.Install) ? ((operation.OperationType != PackageAction.Update) ? (!flag ? this.ResolveOperationsToUninstallSolutionLevelPackage(operation) : this.ResolveOperationsToUninstallProjectLevelPackage(operation)) : (!flag ? this.ResolveOperationsToUpdateSolutionLevelPackage(operation) : this.ResolveOperationsToInstallProjectLevelPackage(operation))) : (!flag ? this.ResolveOperationsToInstallSolutionLevelPackage(operation) : this.ResolveOperationsToInstallProjectLevelPackage(operation));
            List<PackageAction> projectActions = new List<PackageAction>();
            foreach (PackageOperation operation2 in enumerable)
            {
                PackageActionType actionType = (operation2.Action == PackageAction.Install) ? PackageActionType.Install : PackageActionType.Uninstall;
                if (operation2.Target == PackageOperationTarget.Project)
                {
                    projectActions.Add(new PackageProjectAction(actionType, operation2.Package, operation.ProjectManager));
                    continue;
                }
                projectActions.Add(new PackageSolutionAction(actionType, operation2.Package, operation.ProjectManager.PackageManager));
            }
            IList<PackageAction> actions = this.ResolveFinalActions(operation.ProjectManager.PackageManager, projectActions);
            this.UpdateVirtualRepos(actions);
            return actions;
        }

        private IList<PackageAction> ResolveFinalActions(IPackageManager packageManager, IEnumerable<PackageAction> projectActions)
        {
            Dictionary<IPackage, int> dictionary = this._packageRefCounts[packageManager];
            List<PackageSolutionAction> collection = new List<PackageSolutionAction>();
            List<PackageSolutionAction> list2 = new List<PackageSolutionAction>();
            foreach (PackageAction action in projectActions)
            {
                int num2;
                if (action.ActionType == PackageActionType.Uninstall)
                {
                    if (!dictionary.ContainsKey(action.Package))
                    {
                        continue;
                    }
                    IPackage package = action.Package;
                    dictionary[package] -= 1;
                    if (dictionary[action.Package] > 0)
                    {
                        continue;
                    }
                    list2.Add(new PackageSolutionAction(PackageActionType.DeleteFromPackagesFolder, action.Package, packageManager));
                    continue;
                }
                bool flag = false;
                if (!dictionary.TryGetValue(action.Package, out num2))
                {
                    dictionary.Add(action.Package, 1);
                }
                else
                {
                    if (num2 > 0)
                    {
                        flag = true;
                    }
                    dictionary[action.Package] = num2 + 1;
                }
                if (!flag)
                {
                    collection.Add(new PackageSolutionAction(PackageActionType.AddToPackagesFolder, action.Package, packageManager));
                }
            }
            List<PackageAction> list1 = new List<PackageAction>();
            list1.AddRange(collection);
            list1.AddRange(projectActions);
            list1.AddRange(list2);
            return list1;
        }

        private IEnumerable<PackageOperation> ResolveOperationsToInstallProjectLevelPackage(Operation operation)
        {
            DependentsWalker walker1 = new DependentsWalker(operation.ProjectManager.PackageManager.LocalRepository, operation.ProjectManager.GetTargetFrameworkForPackage(operation.Package.Id));
            walker1.DependencyVersion = this.DependencyVersion;
            DependentsWalker dependentsResolver = walker1;
            UpdateWalker walker2 = new UpdateWalker(this._virtualProjectRepos[operation.ProjectManager], operation.ProjectManager.PackageManager.DependencyResolver, dependentsResolver, operation.ProjectManager.ConstraintProvider, operation.ProjectManager.Project.TargetFramework, this.Logger ?? NullLogger.Instance, !this.IgnoreDependencies, this.AllowPrereleaseVersions);
            walker2.AcceptedTargets = PackageTargets.All;
            walker2.DependencyVersion = this.DependencyVersion;
            return walker2.ResolveOperations(operation.Package);
        }

        private IEnumerable<PackageOperation> ResolveOperationsToInstallSolutionLevelPackage(Operation operation)
        {
            IEnumerable<PackageOperation> enumerable = new InstallWalker(this._virtualPackageRepos[operation.ProjectManager.PackageManager], operation.ProjectManager.PackageManager.DependencyResolver, null, this.Logger, this.IgnoreDependencies, this.AllowPrereleaseVersions, this.DependencyVersion).ResolveOperations(operation.Package);
            using (IEnumerator<PackageOperation> enumerator = enumerable.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Target = PackageOperationTarget.PackagesFolder;
                }
            }
            return enumerable;
        }

        private IEnumerable<PackageOperation> ResolveOperationsToUninstallProjectLevelPackage(Operation operation)
        {
            VirtualRepository repository = this._virtualProjectRepos[operation.ProjectManager];
            FrameworkName targetFrameworkForPackage = operation.ProjectManager.GetTargetFrameworkForPackage(operation.Package.Id);
            return new UninstallWalker(repository, new DependentsWalker(repository, targetFrameworkForPackage), targetFrameworkForPackage, NullLogger.Instance, this.RemoveDependencies, this.ForceRemove).ResolveOperations(operation.Package);
        }

        private IEnumerable<PackageOperation> ResolveOperationsToUninstallSolutionLevelPackage(Operation operation)
        {
            IEnumerable<PackageOperation> enumerable = new UninstallWalker(this._virtualPackageRepos[operation.ProjectManager.PackageManager], new DependentsWalker(operation.ProjectManager.PackageManager.LocalRepository, null), null, NullLogger.Instance, this.RemoveDependencies, this.ForceRemove).ResolveOperations(operation.Package);
            using (IEnumerator<PackageOperation> enumerator = enumerable.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Target = PackageOperationTarget.PackagesFolder;
                }
            }
            return enumerable;
        }

        private IEnumerable<PackageOperation> ResolveOperationsToUpdateSolutionLevelPackage(Operation operation)
        {
            VirtualRepository repository = this._virtualPackageRepos[operation.ProjectManager.PackageManager];
            DependentsWalker dependentsResolver = new DependentsWalker(repository, null);
            dependentsResolver.DependencyVersion = this.DependencyVersion;
            UpdateWalker walker2 = new UpdateWalker(repository, operation.ProjectManager.PackageManager.DependencyResolver, dependentsResolver, NullConstraintProvider.Instance, null, this.Logger ?? NullLogger.Instance, !this.IgnoreDependencies, this.AllowPrereleaseVersions);
            walker2.AcceptedTargets = PackageTargets.All;
            walker2.DependencyVersion = this.DependencyVersion;
            IEnumerable<PackageOperation> enumerable = walker2.ResolveOperations(operation.Package);
            using (IEnumerator<PackageOperation> enumerator = enumerable.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    enumerator.Current.Target = PackageOperationTarget.PackagesFolder;
                }
            }
            return enumerable;
        }

        private void UpdateVirtualRepos(IList<PackageAction> actions)
        {
            foreach (PackageAction action in actions)
            {
                PackageProjectAction action2 = action as PackageProjectAction;
                if (action2 != null)
                {
                    VirtualRepository repository2 = this._virtualProjectRepos[action2.ProjectManager];
                    if (action2.ActionType == PackageActionType.Install)
                    {
                        repository2.AddPackage(action.Package);
                        continue;
                    }
                    repository2.RemovePackage(action.Package);
                    continue;
                }
                PackageSolutionAction action3 = (PackageSolutionAction) action;
                VirtualRepository repository = this._virtualPackageRepos[action3.PackageManager];
                if (action3.ActionType == PackageActionType.AddToPackagesFolder)
                {
                    repository.AddPackage(action3.Package);
                    continue;
                }
                if (action3.ActionType == PackageActionType.DeleteFromPackagesFolder)
                {
                    repository.RemovePackage(action3.Package);
                }
            }
        }

        public NuGet.DependencyVersion DependencyVersion { get; set; }

        public bool IgnoreDependencies { get; set; }

        public bool AllowPrereleaseVersions { get; set; }

        public bool ForceRemove { get; set; }

        public bool RemoveDependencies { get; set; }

        public ILogger Logger { get; set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ActionResolver.<>c <>9 = new ActionResolver.<>c();
            public static Func<ActionResolver.Operation, IPackageManager> <>9__33_0;

            internal IPackageManager <InitializeRefCount>b__33_0(ActionResolver.Operation op) => 
                op.ProjectManager.PackageManager;
        }

        private class Operation
        {
            public PackageAction OperationType { get; set; }

            public IPackage Package { get; set; }

            public IProjectManager ProjectManager { get; set; }
        }
    }
}

