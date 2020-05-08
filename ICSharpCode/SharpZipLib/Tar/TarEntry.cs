namespace ICSharpCode.SharpZipLib.Tar
{
    using System;
    using System.IO;

    internal class TarEntry : ICloneable
    {
        private string file;
        private ICSharpCode.SharpZipLib.Tar.TarHeader header;

        private TarEntry()
        {
            this.header = new ICSharpCode.SharpZipLib.Tar.TarHeader();
        }

        public TarEntry(byte[] headerBuffer)
        {
            this.header = new ICSharpCode.SharpZipLib.Tar.TarHeader();
            this.header.ParseBuffer(headerBuffer);
        }

        public TarEntry(ICSharpCode.SharpZipLib.Tar.TarHeader header)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }
            this.header = (ICSharpCode.SharpZipLib.Tar.TarHeader) header.Clone();
        }

        public static void AdjustEntryName(byte[] buffer, string newName)
        {
            ICSharpCode.SharpZipLib.Tar.TarHeader.GetNameBytes(newName, buffer, 0, 100);
        }

        public object Clone()
        {
            TarEntry entry1 = new TarEntry();
            entry1.file = this.file;
            entry1.header = (ICSharpCode.SharpZipLib.Tar.TarHeader) this.header.Clone();
            entry1.Name = this.Name;
            return entry1;
        }

        public static TarEntry CreateEntryFromFile(string fileName)
        {
            TarEntry entry1 = new TarEntry();
            entry1.GetFileTarHeader(entry1.header, fileName);
            return entry1;
        }

        public static TarEntry CreateTarEntry(string name)
        {
            TarEntry entry1 = new TarEntry();
            NameTarHeader(entry1.header, name);
            return entry1;
        }

        public override bool Equals(object obj)
        {
            TarEntry entry = obj as TarEntry;
            return ((entry != null) && this.Name.Equals(entry.Name));
        }

        public TarEntry[] GetDirectoryEntries()
        {
            if ((this.file == null) || !Directory.Exists(this.file))
            {
                return new TarEntry[0];
            }
            string[] fileSystemEntries = Directory.GetFileSystemEntries(this.file);
            TarEntry[] entryArray = new TarEntry[fileSystemEntries.Length];
            for (int i = 0; i < fileSystemEntries.Length; i++)
            {
                entryArray[i] = CreateEntryFromFile(fileSystemEntries[i]);
            }
            return entryArray;
        }

        public void GetFileTarHeader(ICSharpCode.SharpZipLib.Tar.TarHeader header, string file)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }
            this.file = file;
            string str = file;
            if (str.IndexOf(Environment.CurrentDirectory) == 0)
            {
                str = str.Substring(Environment.CurrentDirectory.Length);
            }
            str = str.Replace(Path.DirectorySeparatorChar, '/');
            while (str.StartsWith("/"))
            {
                str = str.Substring(1);
            }
            header.LinkName = string.Empty;
            header.Name = str;
            if (!Directory.Exists(file))
            {
                header.Mode = 0x81c0;
                header.TypeFlag = 0x30;
                header.Size = new FileInfo(file.Replace('/', Path.DirectorySeparatorChar)).Length;
            }
            else
            {
                header.Mode = 0x3eb;
                header.TypeFlag = 0x35;
                if ((header.Name.Length == 0) || (header.Name[header.Name.Length - 1] != '/'))
                {
                    header.Name = header.Name + "/";
                }
                header.Size = 0L;
            }
            header.ModTime = System.IO.File.GetLastWriteTime(file.Replace('/', Path.DirectorySeparatorChar)).ToUniversalTime();
            header.DevMajor = 0;
            header.DevMinor = 0;
        }

        public override int GetHashCode() => 
            this.Name.GetHashCode();

        public bool IsDescendent(TarEntry toTest)
        {
            if (toTest == null)
            {
                throw new ArgumentNullException("toTest");
            }
            return toTest.Name.StartsWith(this.Name);
        }

        public static void NameTarHeader(ICSharpCode.SharpZipLib.Tar.TarHeader header, string name)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            bool flag = name.EndsWith("/");
            header.Name = name;
            header.Mode = flag ? 0x3eb : 0x81c0;
            header.UserId = 0;
            header.GroupId = 0;
            header.Size = 0L;
            header.ModTime = DateTime.UtcNow;
            header.TypeFlag = flag ? ((byte) 0x35) : ((byte) 0x30);
            header.LinkName = string.Empty;
            header.UserName = string.Empty;
            header.GroupName = string.Empty;
            header.DevMajor = 0;
            header.DevMinor = 0;
        }

        public void SetIds(int userId, int groupId)
        {
            this.UserId = userId;
            this.GroupId = groupId;
        }

        public void SetNames(string userName, string groupName)
        {
            this.UserName = userName;
            this.GroupName = groupName;
        }

        public void WriteEntryHeader(byte[] outBuffer)
        {
            this.header.WriteHeader(outBuffer);
        }

        public ICSharpCode.SharpZipLib.Tar.TarHeader TarHeader =>
            this.header;

        public string Name
        {
            get => 
                this.header.Name;
            set => 
                (this.header.Name = value);
        }

        public int UserId
        {
            get => 
                this.header.UserId;
            set => 
                (this.header.UserId = value);
        }

        public int GroupId
        {
            get => 
                this.header.GroupId;
            set => 
                (this.header.GroupId = value);
        }

        public string UserName
        {
            get => 
                this.header.UserName;
            set => 
                (this.header.UserName = value);
        }

        public string GroupName
        {
            get => 
                this.header.GroupName;
            set => 
                (this.header.GroupName = value);
        }

        public DateTime ModTime
        {
            get => 
                this.header.ModTime;
            set => 
                (this.header.ModTime = value);
        }

        public string File =>
            this.file;

        public long Size
        {
            get => 
                this.header.Size;
            set => 
                (this.header.Size = value);
        }

        public bool IsDirectory =>
            ((this.file == null) ? ((this.header != null) && ((this.header.TypeFlag == 0x35) || this.Name.EndsWith("/"))) : Directory.Exists(this.file));
    }
}

