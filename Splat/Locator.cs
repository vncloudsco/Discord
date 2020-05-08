namespace Splat
{
    using System;
    using System.Collections.Generic;

    internal static class Locator
    {
        [ThreadStatic]
        private static IDependencyResolver unitTestDependencyResolver;
        private static IDependencyResolver dependencyResolver = new ModernDependencyResolver();
        private static readonly List<Action> resolverChanged = new List<Action>();

        static Locator()
        {
            RegisterResolverCallbackChanged(delegate {
                if (CurrentMutable != null)
                {
                    CurrentMutable.InitializeSplat();
                }
            });
        }

        public static IDisposable RegisterResolverCallbackChanged(Action callback)
        {
            lock (resolverChanged)
            {
                resolverChanged.Add(callback);
            }
            callback();
            return new ActionDisposable(delegate {
                lock (resolverChanged)
                {
                    resolverChanged.Remove(callback);
                }
            });
        }

        public static IDependencyResolver Current
        {
            get => 
                (unitTestDependencyResolver ?? dependencyResolver);
            set
            {
                if (!ModeDetector.InUnitTestRunner())
                {
                    dependencyResolver = value;
                }
                else
                {
                    unitTestDependencyResolver = value;
                    dependencyResolver = dependencyResolver ?? value;
                }
                Action[] actionArray = null;
                lock (resolverChanged)
                {
                    actionArray = resolverChanged.ToArray();
                }
                foreach (Action action in actionArray)
                {
                    action();
                }
            }
        }

        public static IMutableDependencyResolver CurrentMutable
        {
            get => 
                (Current as IMutableDependencyResolver);
            set => 
                (Current = value);
        }
    }
}

