namespace Mono.Cecil
{
    using System;

    internal enum AssemblyHashAlgorithm : uint
    {
        None = 0,
        Reserved = 0x8003,
        SHA1 = 0x8004
    }
}

