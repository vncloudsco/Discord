namespace Mono.Cecil
{
    using System;

    internal enum NativeType
    {
        None = 0x66,
        Boolean = 2,
        I1 = 3,
        U1 = 4,
        I2 = 5,
        U2 = 6,
        I4 = 7,
        U4 = 8,
        I8 = 9,
        U8 = 10,
        R4 = 11,
        R8 = 12,
        LPStr = 20,
        Int = 0x1f,
        UInt = 0x20,
        Func = 0x26,
        Array = 0x2a,
        Currency = 15,
        BStr = 0x13,
        LPWStr = 0x15,
        LPTStr = 0x16,
        FixedSysString = 0x17,
        IUnknown = 0x19,
        IDispatch = 0x1a,
        Struct = 0x1b,
        IntF = 0x1c,
        SafeArray = 0x1d,
        FixedArray = 30,
        ByValStr = 0x22,
        ANSIBStr = 0x23,
        TBStr = 0x24,
        VariantBool = 0x25,
        ASAny = 40,
        LPStruct = 0x2b,
        CustomMarshaler = 0x2c,
        Error = 0x2d,
        Max = 80
    }
}

