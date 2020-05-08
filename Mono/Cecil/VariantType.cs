namespace Mono.Cecil
{
    using System;

    internal enum VariantType
    {
        None = 0,
        I2 = 2,
        I4 = 3,
        R4 = 4,
        R8 = 5,
        CY = 6,
        Date = 7,
        BStr = 8,
        Dispatch = 9,
        Error = 10,
        Bool = 11,
        Variant = 12,
        Unknown = 13,
        Decimal = 14,
        I1 = 0x10,
        UI1 = 0x11,
        UI2 = 0x12,
        UI4 = 0x13,
        Int = 0x16,
        UInt = 0x17
    }
}

