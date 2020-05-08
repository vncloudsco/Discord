namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class ObjectExtensions
    {
        public static string ToStringSafe(this object obj) => 
            obj?.ToString();
    }
}

