namespace Splat
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal static class DependencyResolverMixins
    {
        public static T GetService<T>(this IDependencyResolver This, string contract = null) => 
            ((T) This.GetService(typeof(T), contract));

        public static IEnumerable<T> GetServices<T>(this IDependencyResolver This, string contract = null) => 
            This.GetServices(typeof(T), contract).Cast<T>();

        public static void InitializeSplat(this IMutableDependencyResolver This)
        {
            This.Register(() => new DefaultLogManager(null), typeof(ILogManager), null);
            This.Register(() => new DebugLogger(), typeof(ILogger), null);
        }

        public static void RegisterConstant(this IMutableDependencyResolver This, object value, Type serviceType, string contract = null)
        {
            This.Register(() => value, serviceType, contract);
        }

        public static void RegisterLazySingleton(this IMutableDependencyResolver This, Func<object> valueFactory, Type serviceType, string contract = null)
        {
            Lazy<object> val = new Lazy<object>(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication);
            This.Register(() => val.Value, serviceType, contract);
        }

        public static IDisposable ServiceRegistrationCallback(this IMutableDependencyResolver This, Type serviceType, Action<IDisposable> callback) => 
            This.ServiceRegistrationCallback(serviceType, null, callback);

        public static IDisposable WithResolver(this IDependencyResolver resolver)
        {
            IDependencyResolver origResolver = Locator.Current;
            Locator.Current = resolver;
            return new ActionDisposable(delegate {
                Locator.Current = origResolver;
            });
        }
    }
}

