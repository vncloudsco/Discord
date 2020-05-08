namespace NuGet.Resolver
{
    using NuGet;
    using System;
    using System.Runtime.CompilerServices;

    internal abstract class PackageAction
    {
        protected PackageAction(PackageActionType actionType, IPackage package)
        {
            this.ActionType = actionType;
            this.Package = package;
        }

        public PackageActionType ActionType { get; private set; }

        public IPackage Package { get; private set; }
    }
}

