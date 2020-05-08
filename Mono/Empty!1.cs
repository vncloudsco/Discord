namespace Mono
{
    using System;

    internal static class Empty<T>
    {
        public static readonly T[] Array;

        static Empty()
        {
            Empty<T>.Array = new T[0];
        }
    }
}

