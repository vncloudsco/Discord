namespace Mono.Cecil.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct TableInformation
    {
        public uint Offset;
        public uint Length;
        public uint RowSize;
    }
}

