namespace NuGet
{
    using System;

    internal interface IShimController : IDisposable
    {
        void Disable();
        void Enable(IPackageSourceProvider sourceProvider);
        void UpdateSources();
    }
}

