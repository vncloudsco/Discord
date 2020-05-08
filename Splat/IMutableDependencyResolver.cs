namespace Splat
{
    using System;
    using System.Runtime.InteropServices;

    internal interface IMutableDependencyResolver : IDependencyResolver, IDisposable
    {
        void Register(Func<object> factory, Type serviceType, string contract = null);
        IDisposable ServiceRegistrationCallback(Type serviceType, string contract, Action<IDisposable> callback);
    }
}

