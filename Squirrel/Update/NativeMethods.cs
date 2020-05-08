namespace Squirrel.Update
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    internal class NativeMethods
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll")]
        public static extern bool AllocConsole();
        [DllImport("kernel32.dll")]
        public static extern bool AttachConsole(int pid);
        [DllImport("Kernel32.dll", SetLastError=true)]
        public static extern IntPtr BeginUpdateResource(string pFileName, bool bDeleteExistingResources);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern bool CloseHandle(IntPtr hHandle);
        [DllImport("Kernel32.dll", SetLastError=true)]
        public static extern bool EndUpdateResource(IntPtr handle, bool discard);
        public static int GetParentProcessId()
        {
            PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();
            IntPtr hProcess = OpenProcess(0x1f0fff, false, Process.GetCurrentProcess().Id);
            try
            {
                int num;
                NtQueryInformationProcess(hProcess, PROCESSINFOCLASS.ProcessBasicInformation, ref pbi, pbi.Size, out num);
            }
            finally
            {
                if (!hProcess.Equals(IntPtr.Zero))
                {
                    CloseHandle(hProcess);
                    hProcess = IntPtr.Zero;
                }
            }
            return (int) pbi.InheritedFromUniqueProcessId;
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetStdHandle(StandardHandles nStdHandle);
        [DllImport("NTDLL.DLL", SetLastError=true)]
        public static extern int NtQueryInformationProcess(IntPtr hProcess, PROCESSINFOCLASS pic, ref PROCESS_BASIC_INFORMATION pbi, int cb, out int pSize);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern IntPtr OpenProcess(ProcessAccess dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);
        [DllImport("Kernel32.dll", SetLastError=true)]
        public static extern bool UpdateResource(IntPtr handle, IntPtr pType, IntPtr pName, short language, [MarshalAs(UnmanagedType.LPArray)] byte[] pData, int dwSize);
        [DllImport("Kernel32.dll", SetLastError=true)]
        public static extern bool UpdateResource(IntPtr handle, string pType, IntPtr pName, short language, [MarshalAs(UnmanagedType.LPArray)] byte[] pData, int dwSize);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);
    }
}

