namespace NuGet
{
    using System;

    internal interface IPackageOperationEventListener
    {
        void OnAddPackageReferenceError(IProjectManager projectManager, Exception exception);
        void OnAfterAddPackageReference(IProjectManager projectManager);
        void OnBeforeAddPackageReference(IProjectManager projectManager);
    }
}

