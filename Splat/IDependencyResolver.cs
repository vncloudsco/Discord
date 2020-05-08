namespace Splat
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal interface IDependencyResolver : IDisposable
    {
        object GetService(Type serviceType, string contract = null);
        IEnumerable<object> GetServices(Type serviceType, string contract = null);
    }
}

