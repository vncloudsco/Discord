namespace Mono.Cecil
{
    using System;

    internal enum MetadataType : byte
    {
        Void = 1,
        Boolean = 2,
        Char = 3,
        SByte = 4,
        Byte = 5,
        Int16 = 6,
        UInt16 = 7,
        Int32 = 8,
        UInt32 = 9,
        Int64 = 10,
        UInt64 = 11,
        Single = 12,
        Double = 13,
        String = 14,
        Pointer = 15,
        ByReference = 0x10,
        ValueType = 0x11,
        Class = 0x12,
        Var = 0x13,
        Array = 20,
        GenericInstance = 0x15,
        TypedByReference = 0x16,
        IntPtr = 0x18,
        UIntPtr = 0x19,
        FunctionPointer = 0x1b,
        Object = 0x1c,
        MVar = 30,
        RequiredModifier = 0x1f,
        OptionalModifier = 0x20,
        Sentinel = 0x41,
        Pinned = 0x45
    }
}

