namespace Splat
{
    using System;
    using System.Runtime.InteropServices;

    internal class DefaultLogManager : ILogManager
    {
        private readonly MemoizingMRUCache<Type, IFullLogger> loggerCache;
        private static readonly IFullLogger nullLogger = new WrappingFullLogger(new NullLogger(), typeof(MemoizingMRUCache<Type, IFullLogger>));

        public DefaultLogManager(IDependencyResolver dependencyResolver = null)
        {
            Func<Type, object, IFullLogger> calculationFunc = null;
            dependencyResolver = dependencyResolver ?? Locator.Current;
            if (calculationFunc == null)
            {
                calculationFunc = delegate (Type type, object _) {
                    ILogger service = dependencyResolver.GetService<ILogger>(null);
                    if (service == null)
                    {
                        throw new Exception("Couldn't find an ILogger. This should never happen, your dependency resolver is probably broken.");
                    }
                    return new WrappingFullLogger(service, type);
                };
            }
            this.loggerCache = new MemoizingMRUCache<Type, IFullLogger>(calculationFunc, 0x40, null);
        }

        public IFullLogger GetLogger(Type type)
        {
            if (LogHost.suppressLogging)
            {
                return nullLogger;
            }
            if (type == typeof(MemoizingMRUCache<Type, IFullLogger>))
            {
                return nullLogger;
            }
            lock (this.loggerCache)
            {
                return this.loggerCache.Get(type);
            }
        }
    }
}

