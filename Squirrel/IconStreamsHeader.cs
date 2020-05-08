namespace Squirrel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IconStreamsHeader
    {
        public uint cbSize;
        public uint unknown1;
        public uint unknown2;
        public uint count;
        public uint unknown3;
    }
}

