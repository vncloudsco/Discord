﻿namespace Mono.Cecil
{
    using System;

    [Flags]
    internal enum ParameterAttributes : ushort
    {
        None = 0,
        In = 1,
        Out = 2,
        Lcid = 4,
        Retval = 8,
        Optional = 0x10,
        HasDefault = 0x1000,
        HasFieldMarshal = 0x2000,
        Unused = 0xcfe0
    }
}

