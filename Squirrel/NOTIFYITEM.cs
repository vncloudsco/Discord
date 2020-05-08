namespace Squirrel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct NOTIFYITEM
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string exe_name;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string tip;
        public IntPtr icon;
        public IntPtr hwnd;
        public NOTIFYITEM_PREFERENCE preference;
        public uint id;
        public Guid guid;
    }
}

