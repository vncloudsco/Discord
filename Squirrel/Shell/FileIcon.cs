namespace Squirrel.Shell
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;

    internal class FileIcon
    {
        private const int MAX_PATH = 260;
        private const int FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x100;
        private const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 0x2000;
        private const int FORMAT_MESSAGE_FROM_HMODULE = 0x800;
        private const int FORMAT_MESSAGE_FROM_STRING = 0x400;
        private const int FORMAT_MESSAGE_FROM_SYSTEM = 0x1000;
        private const int FORMAT_MESSAGE_IGNORE_INSERTS = 0x200;
        private const int FORMAT_MESSAGE_MAX_WIDTH_MASK = 0xff;
        private string fileName;
        private string displayName;
        private string typeName;
        private SHGetFileInfoConstants flags;
        private Icon fileIcon;

        public FileIcon()
        {
            this.flags = SHGetFileInfoConstants.SHGFI_EXETYPE | SHGetFileInfoConstants.SHGFI_ATTRIBUTES | SHGetFileInfoConstants.SHGFI_TYPENAME | SHGetFileInfoConstants.SHGFI_DISPLAYNAME | SHGetFileInfoConstants.SHGFI_ICON;
        }

        public FileIcon(string fileName) : this()
        {
            this.fileName = fileName;
            this.GetInfo();
        }

        public FileIcon(string fileName, SHGetFileInfoConstants flags)
        {
            this.fileName = fileName;
            this.flags = flags;
            this.GetInfo();
        }

        [DllImport("user32.dll")]
        private static extern int DestroyIcon(IntPtr hIcon);
        [DllImport("kernel32")]
        private static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, string lpBuffer, uint nSize, int argumentsLong);
        public void GetInfo()
        {
            this.fileIcon = null;
            this.typeName = "";
            this.displayName = "";
            SHFILEINFO psfi = new SHFILEINFO();
            if (SHGetFileInfo(this.fileName, 0, ref psfi, (uint) Marshal.SizeOf(psfi.GetType()), (uint) this.flags) == 0)
            {
                int lastError = GetLastError();
                Console.WriteLine("Error {0}", lastError);
                string lpBuffer = new string('\0', 0x100);
                int num3 = FormatMessage(0x1200, IntPtr.Zero, lastError, 0, lpBuffer, 0x100, 0);
                Console.WriteLine("Len {0} text {1}", num3, lpBuffer);
            }
            else
            {
                if (psfi.hIcon != IntPtr.Zero)
                {
                    this.fileIcon = Icon.FromHandle(psfi.hIcon);
                }
                this.typeName = psfi.szTypeName;
                this.displayName = psfi.szDisplayName;
            }
        }

        [DllImport("kernel32")]
        private static extern int GetLastError();
        [DllImport("shell32")]
        private static extern int SHGetFileInfo(string pszPath, int dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        public SHGetFileInfoConstants Flags
        {
            get => 
                this.flags;
            set => 
                (this.flags = value);
        }

        public string FileName
        {
            get => 
                this.fileName;
            set => 
                (this.fileName = value);
        }

        public Icon ShellIcon =>
            this.fileIcon;

        public string DisplayName =>
            this.displayName;

        public string TypeName =>
            this.typeName;

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public int dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=80)]
            public string szTypeName;
        }

        [Flags]
        public enum SHGetFileInfoConstants
        {
            SHGFI_ICON = 0x100,
            SHGFI_DISPLAYNAME = 0x200,
            SHGFI_TYPENAME = 0x400,
            SHGFI_ATTRIBUTES = 0x800,
            SHGFI_ICONLOCATION = 0x1000,
            SHGFI_EXETYPE = 0x2000,
            SHGFI_SYSICONINDEX = 0x4000,
            SHGFI_LINKOVERLAY = 0x8000,
            SHGFI_SELECTED = 0x10000,
            SHGFI_ATTR_SPECIFIED = 0x20000,
            SHGFI_LARGEICON = 0,
            SHGFI_SMALLICON = 1,
            SHGFI_OPENICON = 2,
            SHGFI_SHELLICONSIZE = 4,
            SHGFI_USEFILEATTRIBUTES = 0x10,
            SHGFI_ADDOVERLAYS = 0x20,
            SHGFI_OVERLAYINDEX = 0x40
        }
    }
}

