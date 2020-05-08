namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal static class CollectionExtensions
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (T local in items)
            {
                collection.Add(local);
            }
        }

        public static int RemoveAll<T>(this ICollection<T> collection, Func<T, bool> match)
        {
            IList<T> list = Enumerable.Where<T>(collection, match).ToList<T>();
            foreach (T local in list)
            {
                collection.Remove(local);
            }
            return list.Count;
        }
    }
}

