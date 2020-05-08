﻿namespace Mono.Cecil
{
    using System;

    [Flags]
    internal enum PropertyAttributes : ushort
    {
        None = 0,
        SpecialName = 0x200,
        RTSpecialName = 0x400,
        HasDefault = 0x1000,
        Unused = 0xe9ff
    }
}

