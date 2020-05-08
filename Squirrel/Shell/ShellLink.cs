namespace Squirrel.Shell
{
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class ShellLink : IDisposable
    {
        private IShellLinkW linkW;
        private IShellLinkA linkA;
        private string shortcutFile;

        public ShellLink()
        {
            this.shortcutFile = "";
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                this.linkW = (IShellLinkW) new CShellLink();
            }
            else
            {
                this.linkA = (IShellLinkA) new CShellLink();
            }
        }

        public ShellLink(string linkFile) : this()
        {
            this.Open(linkFile);
        }

        public void Dispose()
        {
            if (this.linkW != null)
            {
                Marshal.ReleaseComObject(this.linkW);
                this.linkW = null;
            }
            if (this.linkA != null)
            {
                Marshal.ReleaseComObject(this.linkA);
                this.linkA = null;
            }
        }

        ~ShellLink()
        {
            this.Dispose();
        }

        private Icon getIcon(bool large)
        {
            int piIcon = 0;
            StringBuilder pszIconPath = new StringBuilder(260, 260);
            if (this.linkA == null)
            {
                this.linkW.GetIconLocation(pszIconPath, pszIconPath.Capacity, out piIcon);
            }
            else
            {
                this.linkA.GetIconLocation(pszIconPath, pszIconPath.Capacity, out piIcon);
            }
            string lpszFile = pszIconPath.ToString();
            if (lpszFile.Length == 0)
            {
                FileIcon.SHGetFileInfoConstants flags = FileIcon.SHGetFileInfoConstants.SHGFI_ATTRIBUTES | FileIcon.SHGetFileInfoConstants.SHGFI_ICON;
                flags = !large ? (flags | FileIcon.SHGetFileInfoConstants.SHGFI_SMALLICON) : (flags | FileIcon.SHGetFileInfoConstants.SHGFI_LARGEICON);
                return new FileIcon(this.Target, flags).ShellIcon;
            }
            IntPtr[] phIconLarge = new IntPtr[] { IntPtr.Zero };
            if (large)
            {
                UnManagedMethods.ExtractIconEx(lpszFile, piIcon, phIconLarge, null, 1);
            }
            else
            {
                UnManagedMethods.ExtractIconEx(lpszFile, piIcon, null, phIconLarge, 1);
            }
            Icon icon = null;
            if (phIconLarge[0] != IntPtr.Zero)
            {
                icon = Icon.FromHandle(phIconLarge[0]);
            }
            return icon;
        }

        public void Open(string linkFile)
        {
            this.Open(linkFile, IntPtr.Zero, EShellLinkResolveFlags.SLR_ANY_MATCH | EShellLinkResolveFlags.SLR_NO_UI, 1);
        }

        public void Open(string linkFile, IntPtr hWnd, EShellLinkResolveFlags resolveFlags)
        {
            this.Open(linkFile, hWnd, resolveFlags, 1);
        }

        public void Open(string linkFile, IntPtr hWnd, EShellLinkResolveFlags resolveFlags, ushort timeOut)
        {
            uint fFlags = ((resolveFlags & EShellLinkResolveFlags.SLR_NO_UI) != EShellLinkResolveFlags.SLR_NO_UI) ? ((uint) resolveFlags) : ((uint) (resolveFlags | ((EShellLinkResolveFlags) (timeOut << 0x10))));
            if (this.linkA == null)
            {
                ((IPersistFile) this.linkW).Load(linkFile, 0);
                this.linkW.Resolve(hWnd, fFlags);
                this.shortcutFile = linkFile;
            }
            else
            {
                ((IPersistFile) this.linkA).Load(linkFile, 0);
                this.linkA.Resolve(hWnd, fFlags);
                this.shortcutFile = linkFile;
            }
        }

        public void Save()
        {
            this.Save(this.shortcutFile);
        }

        public void Save(string linkFile)
        {
            if (this.linkA == null)
            {
                ((IPersistFile) this.linkW).Save(linkFile, true);
                this.shortcutFile = linkFile;
            }
            else
            {
                ((IPersistFile) this.linkA).Save(linkFile, true);
                this.shortcutFile = linkFile;
            }
        }

        public void SetAppUserModelId(string appId)
        {
            ((IPropertyStore) this.linkW).SetValue(ref PROPERTYKEY.PKEY_AppUserModel_ID, ref PropVariant.FromString(appId));
        }

        public string ShortCutFile
        {
            get => 
                this.shortcutFile;
            set => 
                (this.shortcutFile = value);
        }

        public Icon LargeIcon =>
            this.getIcon(true);

        public Icon SmallIcon =>
            this.getIcon(false);

        public string IconPath
        {
            get
            {
                StringBuilder pszIconPath = new StringBuilder(260, 260);
                int piIcon = 0;
                if (this.linkA == null)
                {
                    this.linkW.GetIconLocation(pszIconPath, pszIconPath.Capacity, out piIcon);
                }
                else
                {
                    this.linkA.GetIconLocation(pszIconPath, pszIconPath.Capacity, out piIcon);
                }
                return pszIconPath.ToString();
            }
            set
            {
                StringBuilder pszIconPath = new StringBuilder(260, 260);
                int piIcon = 0;
                if (this.linkA == null)
                {
                    this.linkW.GetIconLocation(pszIconPath, pszIconPath.Capacity, out piIcon);
                }
                else
                {
                    this.linkA.GetIconLocation(pszIconPath, pszIconPath.Capacity, out piIcon);
                }
                if (this.linkA == null)
                {
                    this.linkW.SetIconLocation(value, piIcon);
                }
                else
                {
                    this.linkA.SetIconLocation(value, piIcon);
                }
            }
        }

        public int IconIndex
        {
            get
            {
                StringBuilder pszIconPath = new StringBuilder(260, 260);
                int piIcon = 0;
                if (this.linkA == null)
                {
                    this.linkW.GetIconLocation(pszIconPath, pszIconPath.Capacity, out piIcon);
                }
                else
                {
                    this.linkA.GetIconLocation(pszIconPath, pszIconPath.Capacity, out piIcon);
                }
                return piIcon;
            }
            set
            {
                StringBuilder pszIconPath = new StringBuilder(260, 260);
                int piIcon = 0;
                if (this.linkA == null)
                {
                    this.linkW.GetIconLocation(pszIconPath, pszIconPath.Capacity, out piIcon);
                }
                else
                {
                    this.linkA.GetIconLocation(pszIconPath, pszIconPath.Capacity, out piIcon);
                }
                if (this.linkA == null)
                {
                    this.linkW.SetIconLocation(pszIconPath.ToString(), value);
                }
                else
                {
                    this.linkA.SetIconLocation(pszIconPath.ToString(), value);
                }
            }
        }

        public string Target
        {
            get
            {
                StringBuilder pszFile = new StringBuilder(260, 260);
                if (this.linkA == null)
                {
                    _WIN32_FIND_DATAW pfd = new _WIN32_FIND_DATAW();
                    this.linkW.GetPath(pszFile, pszFile.Capacity, ref pfd, 2);
                }
                else
                {
                    _WIN32_FIND_DATAA pfd = new _WIN32_FIND_DATAA();
                    this.linkA.GetPath(pszFile, pszFile.Capacity, ref pfd, 2);
                }
                return pszFile.ToString();
            }
            set
            {
                if (this.linkA == null)
                {
                    this.linkW.SetPath(value);
                }
                else
                {
                    this.linkA.SetPath(value);
                }
            }
        }

        public string WorkingDirectory
        {
            get
            {
                StringBuilder pszDir = new StringBuilder(260, 260);
                if (this.linkA == null)
                {
                    this.linkW.GetWorkingDirectory(pszDir, pszDir.Capacity);
                }
                else
                {
                    this.linkA.GetWorkingDirectory(pszDir, pszDir.Capacity);
                }
                return pszDir.ToString();
            }
            set
            {
                if (this.linkA == null)
                {
                    this.linkW.SetWorkingDirectory(value);
                }
                else
                {
                    this.linkA.SetWorkingDirectory(value);
                }
            }
        }

        public string Description
        {
            get
            {
                StringBuilder pszFile = new StringBuilder(0x400, 0x400);
                if (this.linkA == null)
                {
                    this.linkW.GetDescription(pszFile, pszFile.Capacity);
                }
                else
                {
                    this.linkA.GetDescription(pszFile, pszFile.Capacity);
                }
                return pszFile.ToString();
            }
            set
            {
                if (this.linkA == null)
                {
                    this.linkW.SetDescription(value);
                }
                else
                {
                    this.linkA.SetDescription(value);
                }
            }
        }

        public string Arguments
        {
            get
            {
                StringBuilder pszArgs = new StringBuilder(260, 260);
                if (this.linkA == null)
                {
                    this.linkW.GetArguments(pszArgs, pszArgs.Capacity);
                }
                else
                {
                    this.linkA.GetArguments(pszArgs, pszArgs.Capacity);
                }
                return pszArgs.ToString();
            }
            set
            {
                if (this.linkA == null)
                {
                    this.linkW.SetArguments(value);
                }
                else
                {
                    this.linkA.SetArguments(value);
                }
            }
        }

        public LinkDisplayMode DisplayMode
        {
            get
            {
                uint piShowCmd = 0;
                if (this.linkA == null)
                {
                    this.linkW.GetShowCmd(out piShowCmd);
                }
                else
                {
                    this.linkA.GetShowCmd(out piShowCmd);
                }
                return (LinkDisplayMode) piShowCmd;
            }
            set
            {
                if (this.linkA == null)
                {
                    this.linkW.SetShowCmd((uint) value);
                }
                else
                {
                    this.linkA.SetShowCmd((uint) value);
                }
            }
        }

        public short HotKey
        {
            get
            {
                short pwHotkey = 0;
                if (this.linkA == null)
                {
                    this.linkW.GetHotkey(out pwHotkey);
                }
                else
                {
                    this.linkA.GetHotkey(out pwHotkey);
                }
                return pwHotkey;
            }
            set
            {
                if (this.linkA == null)
                {
                    this.linkW.SetHotkey(value);
                }
                else
                {
                    this.linkA.SetHotkey(value);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack=4)]
        private struct _FILETIME
        {
            public uint dwLowDateTime;
            public uint dwHighDateTime;
        }

        [StructLayout(LayoutKind.Sequential, Pack=4)]
        private struct _WIN32_FIND_DATAA
        {
            public uint dwFileAttributes;
            public ShellLink._FILETIME ftCreationTime;
            public ShellLink._FILETIME ftLastAccessTime;
            public ShellLink._FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=14)]
            public string cAlternateFileName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode, Pack=4)]
        private struct _WIN32_FIND_DATAW
        {
            public uint dwFileAttributes;
            public ShellLink._FILETIME ftCreationTime;
            public ShellLink._FILETIME ftLastAccessTime;
            public ShellLink._FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst=14)]
            public string cAlternateFileName;
        }

        [ComImport, ClassInterface(ClassInterfaceType.None), Guid("00021401-0000-0000-C000-000000000046")]
        private class CShellLink
        {
        }

        private enum EShellLinkGP : uint
        {
            SLGP_SHORTPATH = 1,
            SLGP_UNCPRIORITY = 2
        }

        [Flags]
        public enum EShellLinkResolveFlags : uint
        {
            SLR_ANY_MATCH = 2,
            SLR_INVOKE_MSI = 0x80,
            SLR_NOLINKINFO = 0x40,
            SLR_NO_UI = 1,
            SLR_NO_UI_WITH_MSG_PUMP = 0x101,
            SLR_NOUPDATE = 8,
            SLR_NOSEARCH = 0x10,
            SLR_NOTRACK = 0x20,
            SLR_UPDATE = 4
        }

        [Flags]
        private enum EShowWindowFlags : uint
        {
            SW_HIDE = 0,
            SW_SHOWNORMAL = 1,
            SW_NORMAL = 1,
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_SHOWNOACTIVATE = 4,
            SW_SHOW = 5,
            SW_MINIMIZE = 6,
            SW_SHOWMINNOACTIVE = 7,
            SW_SHOWNA = 8,
            SW_RESTORE = 9,
            SW_SHOWDEFAULT = 10,
            SW_MAX = 10
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000010C-0000-0000-C000-000000000046")]
        private interface IPersist
        {
            [PreserveSig]
            void GetClassID(out Guid pClassID);
        }

        [ComImport, Guid("0000010B-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPersistFile
        {
            [PreserveSig]
            void GetClassID(out Guid pClassID);
            void IsDirty();
            void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
            void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);
            void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
            void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
        }

        [ComImport, Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IPropertyStore
        {
            [PreserveSig]
            int GetCount(out uint cProps);
            [PreserveSig]
            int GetAt([In] uint iProp, out ShellLink.PROPERTYKEY pkey);
            [PreserveSig]
            int GetValue([In] ref ShellLink.PROPERTYKEY key, out ShellLink.PropVariant pv);
            [PreserveSig]
            int SetValue([In] ref ShellLink.PROPERTYKEY key, [In] ref ShellLink.PropVariant pv);
            [PreserveSig]
            int Commit();
        }

        [ComImport, Guid("000214EE-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellLinkA
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile, int cchMaxPath, ref ShellLink._WIN32_FIND_DATAA pfd, uint fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short pwHotkey);
            void GetShowCmd(out uint piShowCmd);
            void SetShowCmd(uint piShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPStr)] string pszPathRel, uint dwReserved);
            void Resolve(IntPtr hWnd, uint fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPStr)] string pszFile);
        }

        [ComImport, Guid("000214F9-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellLinkW
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, ref ShellLink._WIN32_FIND_DATAW pfd, uint fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short pwHotkey);
            void GetShowCmd(out uint piShowCmd);
            void SetShowCmd(uint piShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
            void Resolve(IntPtr hWnd, uint fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        public enum LinkDisplayMode : uint
        {
            edmNormal = 1,
            edmMinimized = 7,
            edmMaximized = 3
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROPERTYKEY
        {
            public Guid fmtid;
            public UIntPtr pid;
            public static ShellLink.PROPERTYKEY PKEY_AppUserModel_ID =>
                new ShellLink.PROPERTYKEY { 
                    fmtid=Guid.ParseExact("{9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3}", "B"),
                    pid=new UIntPtr(5)
                };
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PropVariant
        {
            public short variantType;
            public short Reserved1;
            public short Reserved2;
            public short Reserved3;
            public IntPtr pointerValue;
            public static ShellLink.PropVariant FromString(string str) => 
                new ShellLink.PropVariant { 
                    variantType = 0x1f,
                    pointerValue = Marshal.StringToCoTaskMemUni(str)
                };
        }

        private class UnManagedMethods
        {
            [DllImport("user32")]
            internal static extern int DestroyIcon(IntPtr hIcon);
            [DllImport("Shell32", CharSet=CharSet.Auto)]
            internal static extern int ExtractIconEx([MarshalAs(UnmanagedType.LPTStr)] string lpszFile, int nIconIndex, IntPtr[] phIconLarge, IntPtr[] phIconSmall, int nIcons);
        }
    }
}

