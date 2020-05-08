namespace Squirrel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct NOTIFYITEM_Writable
    {
        public IntPtr exe_name;
        public IntPtr tip;
        public IntPtr icon;
        public IntPtr hwnd;
        public NOTIFYITEM_PREFERENCE preference;
        public uint id;
        public Guid guid;
        public static NOTIFYITEM_Writable fromNotifyItem(NOTIFYITEM item) => 
            new NOTIFYITEM_Writable { 
                exe_name = Marshal.StringToCoTaskMemAuto(item.exe_name),
                tip = Marshal.StringToCoTaskMemAuto(item.tip),
                icon = item.icon,
                hwnd = item.hwnd,
                preference = item.preference,
                id = item.id,
                guid = item.guid
            };
    }
}

