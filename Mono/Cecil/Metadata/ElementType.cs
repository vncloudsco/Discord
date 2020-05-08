namespace Mono.Cecil.Metadata
{
    using System;

    internal enum ElementType : byte
    {
        None = 0,
        Void = 1,
        Boolean = 2,
        Char = 3,
        I1 = 4,
        U1 = 5,
        I2 = 6,
        U2 = 7,
        I4 = 8,
        U4 = 9,
        I8 = 10,
        U8 = 11,
        R4 = 12,
        R8 = 13,
        String = 14,
        Ptr = 15,
        ByRef = 0x10,
        ValueType = 0x11,
        Class = 0x12,
        Var = 0x13,
        Array = 20,
        GenericInst = 0x15,
        TypedByRef = 0x16,
        I = 0x18,
        U = 0x19,
        FnPtr = 0x1b,
        Object = 0x1c,
        SzArray = 0x1d,
        MVar = 30,
        CModReqD = 0x1f,
        CModOpt = 0x20,
        Internal = 0x21,
        Modifier = 0x40,
        Sentinel = 0x41,
        Pinned = 0x45,
        Type = 80,
        Boxed = 0x51,
        Enum = 0x55
    }
}

