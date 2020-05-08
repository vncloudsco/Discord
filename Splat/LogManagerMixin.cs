namespace Splat
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class LogManagerMixin
    {
        public static IFullLogger GetLogger<T>(this ILogManager This) => 
            This.GetLogger(typeof(T));
    }
}

