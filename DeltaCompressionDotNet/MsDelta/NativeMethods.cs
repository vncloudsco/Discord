namespace DeltaCompressionDotNet.MsDelta
{
    using System;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("msdelta.dll", CharSet=CharSet.Unicode)]
        public static extern bool ApplyDelta([MarshalAs(UnmanagedType.I8)] ApplyFlags applyFlags, string sourceName, string deltaName, string targetName);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("msdelta.dll", CharSet=CharSet.Unicode)]
        public static extern bool CreateDelta([MarshalAs(UnmanagedType.I8)] FileTypeSet fileTypeSet, long setFlags, long resetFlags, string sourceName, string targetName, string sourceOptionsName, string targetOptionsName, DeltaInput globalOptions, IntPtr targetFileTime, [MarshalAs(UnmanagedType.U4)] HashAlgId hashAlgId, string deltaName);
    }
}

