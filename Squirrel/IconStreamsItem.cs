namespace Squirrel
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct IconStreamsItem
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x210)]
        public byte[] exe_path;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst=0x458)]
        public byte[] dontcare;
        public string ExePath
        {
            get
            {
                byte[] buffer2;
                byte[] buffer = new byte[this.exe_path.Length];
                for (int i = 0; i < this.exe_path.Length; i++)
                {
                    byte num2 = this.exe_path[i];
                    buffer[i] = ((num2 <= 0x40) || (num2 >= 0x5b)) ? (((num2 <= 0x60) || (num2 >= 0x7b)) ? num2 : ((byte) ((((num2 - 0x60) + 13) % 0x1a) + 0x60))) : ((byte) ((((num2 - 0x40) + 13) % 0x1a) + 0x40));
                }
                fixed (byte* numRef = ((((buffer2 = buffer) == null) || (buffer2.Length == 0)) ? null : ((byte*) buffer2[0])))
                {
                    return Marshal.PtrToStringUni((IntPtr) numRef);
                }
            }
        }
    }
}

