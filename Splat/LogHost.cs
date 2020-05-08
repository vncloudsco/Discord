namespace Splat
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class LogHost
    {
        internal static bool suppressLogging = false;
        private static readonly IFullLogger nullLogger = new WrappingFullLogger(new NullLogger(), typeof(string));

        public static IFullLogger Log<T>(this T This) where T: IEnableLogger
        {
            if (suppressLogging)
            {
                return nullLogger;
            }
            ILogManager service = Locator.Current.GetService<ILogManager>(null);
            if (service == null)
            {
                throw new Exception("ILogManager is null. This should never happen, your dependency resolver is broken");
            }
            return service.GetLogger<T>();
        }

        public static IFullLogger Default
        {
            get
            {
                if (suppressLogging)
                {
                    return nullLogger;
                }
                ILogManager service = Locator.Current.GetService<ILogManager>(null);
                if (service == null)
                {
                    throw new Exception("ILogManager is null. This should never happen, your dependency resolver is broken");
                }
                return service.GetLogger(typeof(LogHost));
            }
        }
    }
}

