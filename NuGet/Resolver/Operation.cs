namespace NuGet.Resolver
{
    using NuGet;
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    internal class Operation : PackageOperation
    {
        private string _projectName;

        public Operation(PackageOperation operation, IProjectManager projectManager, IPackageManager packageManager) : base(operation.Package, operation.Action)
        {
            if ((projectManager != null) && (packageManager != null))
            {
                throw new ArgumentException("Only one of packageManager and projectManager can be non-null");
            }
            if ((operation.Target == PackageOperationTarget.PackagesFolder) && (packageManager == null))
            {
                throw new ArgumentNullException("packageManager");
            }
            if ((operation.Target == PackageOperationTarget.Project) && (projectManager == null))
            {
                throw new ArgumentNullException("projectManager");
            }
            base.Target = operation.Target;
            this.PackageManager = packageManager;
            this.ProjectManager = projectManager;
            if (this.ProjectManager != null)
            {
                this._projectName = this.ProjectManager.Project.ProjectName;
            }
        }

        public override string ToString()
        {
            object[] args = new object[] { base.ToString(), string.IsNullOrEmpty(this._projectName) ? "" : (" -> " + this._projectName) };
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}", args);
        }

        public IProjectManager ProjectManager { get; private set; }

        public IPackageManager PackageManager { get; private set; }
    }
}

