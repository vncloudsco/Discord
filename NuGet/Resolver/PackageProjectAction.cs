namespace NuGet.Resolver
{
    using NuGet;
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    internal class PackageProjectAction : PackageAction
    {
        private string _projectName;

        public PackageProjectAction(PackageActionType actionType, IPackage package, IProjectManager projectManager) : base(actionType, package)
        {
            this.ProjectManager = projectManager;
            this._projectName = this.ProjectManager.Project.ProjectName;
        }

        public override string ToString()
        {
            if (base.ActionType == PackageActionType.Install)
            {
                object[] objArray1 = new object[] { base.Package.ToString(), this._projectName };
                return string.Format(CultureInfo.InvariantCulture, "Install {0} into project '{1}'", objArray1);
            }
            object[] args = new object[] { base.Package.ToString(), this._projectName };
            return string.Format(CultureInfo.InvariantCulture, "Uninstall {0} from project '{1}'", args);
        }

        public IProjectManager ProjectManager { get; private set; }
    }
}

