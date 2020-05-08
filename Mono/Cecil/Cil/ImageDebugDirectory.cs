namespace Mono.Cecil.Cil
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ImageDebugDirectory
    {
        public int Characteristics;
        public int TimeDateStamp;
        public short MajorVersion;
        public short MinorVersion;
        public int Type;
        public int SizeOfData;
        public int AddressOfRawData;
        public int PointerToRawData;
    }
}

