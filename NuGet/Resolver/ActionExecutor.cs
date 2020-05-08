namespace NuGet.Resolver
{
    using NuGet;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal class ActionExecutor
    {
        public ActionExecutor()
        {
            this.Logger = NullLogger.Instance;
        }

        private static PackageAction CreateReverseAction(PackageAction action)
        {
            PackageProjectAction action2 = action as PackageProjectAction;
            if (action2 != null)
            {
                return new PackageProjectAction(GetReverseActionType(action2.ActionType), action2.Package, action2.ProjectManager);
            }
            PackageSolutionAction action3 = (PackageSolutionAction) action;
            return new PackageSolutionAction(GetReverseActionType(action3.ActionType), action3.Package, action3.PackageManager);
        }

        public void Execute(IEnumerable<PackageAction> actions)
        {
            List<PackageAction> executedOperations = new List<PackageAction>();
            try
            {
                foreach (PackageAction action in actions)
                {
                    executedOperations.Add(action);
                    PackageProjectAction action2 = action as PackageProjectAction;
                    if (action2 != null)
                    {
                        this.ExecuteProjectOperation(action2);
                        if ((action2.ActionType != PackageActionType.Install) || ((action2.ProjectManager.PackageManager == null) || (!action2.ProjectManager.PackageManager.BindingRedirectEnabled || !action2.ProjectManager.Project.IsBindingRedirectSupported)))
                        {
                            continue;
                        }
                        action2.ProjectManager.PackageManager.AddBindingRedirects(action2.ProjectManager);
                        continue;
                    }
                    PackageSolutionAction action3 = (PackageSolutionAction) action;
                    action3.PackageManager.Logger = this.Logger;
                    if (action3.ActionType == PackageActionType.AddToPackagesFolder)
                    {
                        action3.PackageManager.Execute(new PackageOperation(action.Package, PackageAction.Install));
                        continue;
                    }
                    if (action3.ActionType == PackageActionType.DeleteFromPackagesFolder)
                    {
                        action3.PackageManager.Execute(new PackageOperation(action.Package, PackageAction.Uninstall));
                    }
                }
            }
            catch
            {
                this.Rollback(executedOperations);
                throw;
            }
        }

        private void ExecuteProjectOperation(PackageProjectAction action)
        {
            try
            {
                if (this.PackageOperationEventListener != null)
                {
                    this.PackageOperationEventListener.OnBeforeAddPackageReference(action.ProjectManager);
                }
                action.ProjectManager.Execute(new PackageOperation(action.Package, (action.ActionType == PackageActionType.Install) ? PackageAction.Install : PackageAction.Uninstall));
            }
            catch (Exception exception)
            {
                if (!this.CatchProjectOperationException)
                {
                    throw;
                }
                this.Logger.Log(MessageLevel.Error, ExceptionUtility.Unwrap(exception).Message, new object[0]);
                if (this.PackageOperationEventListener != null)
                {
                    this.PackageOperationEventListener.OnAddPackageReferenceError(action.ProjectManager, exception);
                }
            }
            finally
            {
                if (this.PackageOperationEventListener != null)
                {
                    this.PackageOperationEventListener.OnAfterAddPackageReference(action.ProjectManager);
                }
            }
        }

        private static PackageActionType GetReverseActionType(PackageActionType actionType)
        {
            switch (actionType)
            {
                case PackageActionType.Install:
                    return PackageActionType.Uninstall;

                case PackageActionType.Uninstall:
                    return PackageActionType.Install;

                case PackageActionType.AddToPackagesFolder:
                    return PackageActionType.DeleteFromPackagesFolder;

                case PackageActionType.DeleteFromPackagesFolder:
                    return PackageActionType.AddToPackagesFolder;
            }
            throw new InvalidOperationException();
        }

        private void Rollback(List<PackageAction> executedOperations)
        {
            if (executedOperations.Count > 0)
            {
                this.Logger.Log(MessageLevel.Warning, "Rolling back", new object[0]);
            }
            executedOperations.Reverse();
            using (List<PackageAction>.Enumerator enumerator = executedOperations.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    PackageAction action = CreateReverseAction(enumerator.Current);
                    PackageProjectAction action2 = action as PackageProjectAction;
                    if (action2 != null)
                    {
                        action2.ProjectManager.Logger = NullLogger.Instance;
                        action2.ProjectManager.Execute(new PackageOperation(action2.Package, (action2.ActionType == PackageActionType.Install) ? PackageAction.Install : PackageAction.Uninstall));
                        continue;
                    }
                    PackageSolutionAction action3 = (PackageSolutionAction) action;
                    action3.PackageManager.Logger = NullLogger.Instance;
                    if (action3.ActionType == PackageActionType.AddToPackagesFolder)
                    {
                        action3.PackageManager.Execute(new PackageOperation(action3.Package, PackageAction.Install));
                        continue;
                    }
                    if (action3.ActionType == PackageActionType.DeleteFromPackagesFolder)
                    {
                        action3.PackageManager.Execute(new PackageOperation(action3.Package, PackageAction.Uninstall));
                    }
                }
            }
        }

        public ILogger Logger { get; set; }

        public IPackageOperationEventListener PackageOperationEventListener { get; set; }

        public bool CatchProjectOperationException { get; set; }
    }
}

