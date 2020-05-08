namespace Mono.Cecil.PE
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct DataDirectory
    {
        public readonly uint VirtualAddress;
        public readonly uint Size;
        public bool IsZero =>
            ((this.VirtualAddress == 0) && (this.Size == 0));
        public DataDirectory(uint rva, uint size)
        {
            this.VirtualAddress = rva;
            this.Size = size;
        }
    }
}

