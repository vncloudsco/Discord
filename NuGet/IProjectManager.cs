namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;

    internal interface IProjectManager
    {
        event EventHandler<PackageOperationEventArgs> PackageReferenceAdded;

        event EventHandler<PackageOperationEventArgs> PackageReferenceAdding;

        event EventHandler<PackageOperationEventArgs> PackageReferenceRemoved;

        event EventHandler<PackageOperationEventArgs> PackageReferenceRemoving;

        void Execute(PackageOperation operation);

        IPackageRepository LocalRepository { get; }

        IPackageManager PackageManager { get; }

        ILogger Logger { get; set; }

        IProjectSystem Project { get; }

        IPackageConstraintProvider ConstraintProvider { get; set; }
    }
}

