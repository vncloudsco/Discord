namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal static class QueryableExtensions
    {
        public static IEnumerable<T> AsBufferedEnumerable<T>(this IQueryable<T> source, int bufferSize) => 
            new BufferedEnumerable<T>(source, bufferSize);
    }
}

