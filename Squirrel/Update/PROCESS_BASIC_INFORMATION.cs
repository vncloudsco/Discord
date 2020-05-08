﻿namespace Squirrel.Update
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct PROCESS_BASIC_INFORMATION
    {
        public IntPtr ExitStatus;
        public IntPtr PebBaseAddress;
        public IntPtr AffinityMask;
        public IntPtr BasePriority;
        public UIntPtr UniqueProcessId;
        public IntPtr InheritedFromUniqueProcessId;
        public int Size =>
            Marshal.SizeOf(typeof(PROCESS_BASIC_INFORMATION));
    }
}

