﻿namespace Mono.Cecil.PE
{
    using System;

    internal sealed class Section
    {
        public string Name;
        public uint VirtualAddress;
        public uint VirtualSize;
        public uint SizeOfRawData;
        public uint PointerToRawData;
        public byte[] Data;
    }
}

