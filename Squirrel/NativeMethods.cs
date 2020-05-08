namespace Squirrel
{
    using System;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("version.dll", SetLastError=true)]
        public static extern bool GetFileVersionInfo(string lpszFileName, IntPtr dwHandleIgnored, int dwLen, [MarshalAs(UnmanagedType.LPArray)] byte[] lpData);
        [DllImport("version.dll", SetLastError=true)]
        public static extern int GetFileVersionInfoSize(string lpszFileName, IntPtr dwHandleIgnored);
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("version.dll")]
        public static extern bool VerQueryValue(byte[] pBlock, string pSubBlock, out IntPtr pValue, out int len);
    }
}

