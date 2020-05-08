namespace Mono.Options
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void OptionAction<TKey, TValue>(TKey key, TValue value);
}

