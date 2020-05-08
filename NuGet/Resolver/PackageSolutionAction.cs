namespace NuGet.Resolver
{
    using NuGet;
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    internal class PackageSolutionAction : PackageAction
    {
        public PackageSolutionAction(PackageActionType actionType, IPackage package, IPackageManager packageManager) : base(actionType, package)
        {
            this.PackageManager = packageManager;
        }

        public override string ToString()
        {
            switch (base.ActionType)
            {
                case PackageActionType.Install:
                {
                    object[] objArray1 = new object[] { base.Package.ToString() };
                    return string.Format(CultureInfo.InvariantCulture, "Install {0} into solution", objArray1);
                }
                case PackageActionType.Uninstall:
                {
                    object[] objArray2 = new object[] { base.Package.ToString() };
                    return string.Format(CultureInfo.InvariantCulture, "Uninstall {0} from solution", objArray2);
                }
                case PackageActionType.AddToPackagesFolder:
                {
                    object[] objArray3 = new object[] { base.Package.ToString() };
                    return string.Format(CultureInfo.InvariantCulture, "Add {0} into packages folder", objArray3);
                }
            }
            object[] args = new object[] { base.Package.ToString() };
            return string.Format(CultureInfo.InvariantCulture, "Delete {0} from packages folder", args);
        }

        public IPackageManager PackageManager { get; private set; }
    }
}

