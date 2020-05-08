namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class StringExtensions
    {
        public static string SafeTrim(this string value) => 
            value?.Trim();
    }
}

