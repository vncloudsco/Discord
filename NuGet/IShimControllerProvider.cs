namespace NuGet
{
    using System;

    internal interface IShimControllerProvider : IDisposable
    {
        IShimController Controller { get; }
    }
}

