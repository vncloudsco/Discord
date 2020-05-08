namespace Mono.Cecil
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Range
    {
        public uint Start;
        public uint Length;
        public Range(uint index, uint length)
        {
            this.Start = index;
            this.Length = length;
        }
    }
}

