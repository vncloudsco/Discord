﻿namespace Mono.Cecil
{
    using System;

    [Flags]
    internal enum FieldAttributes : ushort
    {
        FieldAccessMask = 7,
        CompilerControlled = 0,
        Private = 1,
        FamANDAssem = 2,
        Assembly = 3,
        Family = 4,
        FamORAssem = 5,
        Public = 6,
        Static = 0x10,
        InitOnly = 0x20,
        Literal = 0x40,
        NotSerialized = 0x80,
        SpecialName = 0x200,
        PInvokeImpl = 0x2000,
        RTSpecialName = 0x400,
        HasFieldMarshal = 0x1000,
        HasDefault = 0x8000,
        HasFieldRVA = 0x100
    }
}

