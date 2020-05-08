namespace NuGet.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class AppDomainExtensions
    {
        public static T CreateInstance<T>(this AppDomain domain) => 
            ((T) domain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName));
    }
}

