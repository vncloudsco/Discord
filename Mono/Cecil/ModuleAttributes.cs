namespace Mono.Cecil
{
    using System;

    [Flags]
    internal enum ModuleAttributes
    {
        ILOnly = 1,
        Required32Bit = 2,
        StrongNameSigned = 8,
        Preferred32Bit = 0x20000
    }
}

