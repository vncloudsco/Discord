namespace ICSharpCode.SharpZipLib.Zip
{
    using System;
    using System.IO;

    internal class ZipEntry : ICloneable
    {
        private Known known;
        private int externalFileAttributes;
        private ushort versionMadeBy;
        private string name;
        private ulong size;
        private ulong compressedSize;
        private ushort versionToExtract;
        private uint crc;
        private uint dosTime;
        private ICSharpCode.SharpZipLib.Zip.CompressionMethod method;
        private byte[] extra;
        private string comment;
        private int flags;
        private long zipFileIndex;
        private long offset;
        private bool forceZip64_;
        private byte cryptoCheckValue_;
        private int _aesVer;
        private int _aesEncryptionStrength;

        [Obsolete("Use Clone instead")]
        public ZipEntry(ZipEntry entry)
        {
            this.externalFileAttributes = -1;
            this.method = ICSharpCode.SharpZipLib.Zip.CompressionMethod.Deflated;
            this.zipFileIndex = -1L;
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }
            this.known = entry.known;
            this.name = entry.name;
            this.size = entry.size;
            this.compressedSize = entry.compressedSize;
            this.crc = entry.crc;
            this.dosTime = entry.dosTime;
            this.method = entry.method;
            this.comment = entry.comment;
            this.versionToExtract = entry.versionToExtract;
            this.versionMadeBy = entry.versionMadeBy;
            this.externalFileAttributes = entry.externalFileAttributes;
            this.flags = entry.flags;
            this.zipFileIndex = entry.zipFileIndex;
            this.offset = entry.offset;
            this.forceZip64_ = entry.forceZip64_;
            if (entry.extra != null)
            {
                this.extra = new byte[entry.extra.Length];
                Array.Copy(entry.extra, 0, this.extra, 0, entry.extra.Length);
            }
        }

        public ZipEntry(string name) : this(name, 0, 0x33, ICSharpCode.SharpZipLib.Zip.CompressionMethod.Deflated)
        {
        }

        internal ZipEntry(string name, int versionRequiredToExtract) : this(name, versionRequiredToExtract, 0x33, ICSharpCode.SharpZipLib.Zip.CompressionMethod.Deflated)
        {
        }

        internal ZipEntry(string name, int versionRequiredToExtract, int madeByInfo, ICSharpCode.SharpZipLib.Zip.CompressionMethod method)
        {
            this.externalFileAttributes = -1;
            this.method = ICSharpCode.SharpZipLib.Zip.CompressionMethod.Deflated;
            this.zipFileIndex = -1L;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length > 0xffff)
            {
                throw new ArgumentException("Name is too long", "name");
            }
            if ((versionRequiredToExtract != 0) && (versionRequiredToExtract < 10))
            {
                throw new ArgumentOutOfRangeException("versionRequiredToExtract");
            }
            this.DateTime = System.DateTime.Now;
            this.name = CleanName(name);
            this.versionMadeBy = (ushort) madeByInfo;
            this.versionToExtract = (ushort) versionRequiredToExtract;
            this.method = method;
        }

        public static string CleanName(string name)
        {
            if (name == null)
            {
                return string.Empty;
            }
            if (Path.IsPathRooted(name))
            {
                name = name.Substring(Path.GetPathRoot(name).Length);
            }
            name = name.Replace(@"\", "/");
            while ((name.Length > 0) && (name[0] == '/'))
            {
                name = name.Remove(0, 1);
            }
            return name;
        }

        public object Clone()
        {
            ZipEntry entry = (ZipEntry) base.MemberwiseClone();
            if (this.extra != null)
            {
                entry.extra = new byte[this.extra.Length];
                Array.Copy(this.extra, 0, entry.extra, 0, this.extra.Length);
            }
            return entry;
        }

        public void ForceZip64()
        {
            this.forceZip64_ = true;
        }

        private bool HasDosAttributes(int attributes)
        {
            bool flag = false;
            if ((((this.known & Known.ExternalAttributes) != Known.None) && ((this.HostSystem == 0) || (this.HostSystem == 10))) && ((this.ExternalFileAttributes & attributes) == attributes))
            {
                flag = true;
            }
            return flag;
        }

        public bool IsCompressionMethodSupported() => 
            IsCompressionMethodSupported(this.CompressionMethod);

        public static bool IsCompressionMethodSupported(ICSharpCode.SharpZipLib.Zip.CompressionMethod method) => 
            ((method == ICSharpCode.SharpZipLib.Zip.CompressionMethod.Deflated) || (method == ICSharpCode.SharpZipLib.Zip.CompressionMethod.Stored));

        public bool IsZip64Forced() => 
            this.forceZip64_;

        private void ProcessAESExtraData(ZipExtraData extraData)
        {
            if (!extraData.Find(0x9901))
            {
                throw new ZipException("AES Extra Data missing");
            }
            this.versionToExtract = 0x33;
            this.Flags |= 0x40;
            int valueLength = extraData.ValueLength;
            if (valueLength < 7)
            {
                throw new ZipException("AES Extra Data Length " + valueLength + " invalid.");
            }
            int num2 = extraData.ReadShort();
            extraData.ReadShort();
            int num3 = extraData.ReadByte();
            int num4 = extraData.ReadShort();
            this._aesVer = num2;
            this._aesEncryptionStrength = num3;
            this.method = (ICSharpCode.SharpZipLib.Zip.CompressionMethod) num4;
        }

        internal void ProcessExtraData(bool localHeader)
        {
            ZipExtraData extraData = new ZipExtraData(this.extra);
            if (!extraData.Find(1))
            {
                if (((this.versionToExtract & 0xff) >= 0x2d) && ((this.size == 0xffffffffUL) || (this.compressedSize == 0xffffffffUL)))
                {
                    throw new ZipException("Zip64 Extended information required but is missing.");
                }
            }
            else
            {
                this.forceZip64_ = true;
                if (extraData.ValueLength < 4)
                {
                    throw new ZipException("Extra data extended Zip64 information length is invalid");
                }
                if (localHeader || (this.size == 0xffffffffUL))
                {
                    this.size = (ulong) extraData.ReadLong();
                }
                if (localHeader || (this.compressedSize == 0xffffffffUL))
                {
                    this.compressedSize = (ulong) extraData.ReadLong();
                }
                if (!localHeader && (this.offset == 0xffffffffUL))
                {
                    this.offset = extraData.ReadLong();
                }
            }
            if (!extraData.Find(10))
            {
                if (extraData.Find(0x5455))
                {
                    int valueLength = extraData.ValueLength;
                    if (((extraData.ReadByte() & 1) != 0) && (valueLength >= 5))
                    {
                        int seconds = extraData.ReadInt();
                        this.DateTime = (new System.DateTime(0x7b2, 1, 1, 0, 0, 0).ToUniversalTime() + new TimeSpan(0, 0, 0, seconds, 0)).ToLocalTime();
                    }
                }
            }
            else
            {
                if (extraData.ValueLength < 4)
                {
                    throw new ZipException("NTFS Extra data invalid");
                }
                extraData.ReadInt();
                while (extraData.UnreadCount >= 4)
                {
                    int amount = extraData.ReadShort();
                    if (extraData.ReadShort() == 1)
                    {
                        if (amount >= 0x18)
                        {
                            long fileTime = extraData.ReadLong();
                            extraData.ReadLong();
                            extraData.ReadLong();
                            this.DateTime = System.DateTime.FromFileTime(fileTime);
                        }
                        break;
                    }
                    extraData.Skip(amount);
                }
            }
            if (this.method == ICSharpCode.SharpZipLib.Zip.CompressionMethod.WinZipAES)
            {
                this.ProcessAESExtraData(extraData);
            }
        }

        public override string ToString() => 
            this.name;

        public bool HasCrc =>
            ((this.known & Known.Crc) != Known.None);

        public bool IsCrypted
        {
            get => 
                ((this.flags & 1) != 0);
            set
            {
                if (value)
                {
                    this.flags |= 1;
                }
                else
                {
                    this.flags &= -2;
                }
            }
        }

        public bool IsUnicodeText
        {
            get => 
                ((this.flags & 0x800) != 0);
            set
            {
                if (value)
                {
                    this.flags |= 0x800;
                }
                else
                {
                    this.flags &= -2049;
                }
            }
        }

        internal byte CryptoCheckValue
        {
            get => 
                this.cryptoCheckValue_;
            set => 
                (this.cryptoCheckValue_ = value);
        }

        public int Flags
        {
            get => 
                this.flags;
            set => 
                (this.flags = value);
        }

        public long ZipFileIndex
        {
            get => 
                this.zipFileIndex;
            set => 
                (this.zipFileIndex = value);
        }

        public long Offset
        {
            get => 
                this.offset;
            set => 
                (this.offset = value);
        }

        public int ExternalFileAttributes
        {
            get => 
                (((this.known & Known.ExternalAttributes) != Known.None) ? this.externalFileAttributes : -1);
            set
            {
                this.externalFileAttributes = value;
                this.known |= Known.ExternalAttributes;
            }
        }

        public int VersionMadeBy =>
            (this.versionMadeBy & 0xff);

        public bool IsDOSEntry =>
            ((this.HostSystem == 0) || (this.HostSystem == 10));

        public int HostSystem
        {
            get => 
                ((this.versionMadeBy >> 8) & 0xff);
            set
            {
                this.versionMadeBy = (ushort) (this.versionMadeBy & 0xff);
                this.versionMadeBy = (ushort) (this.versionMadeBy | ((ushort) ((value & 0xff) << 8)));
            }
        }

        public int Version
        {
            get
            {
                if (this.versionToExtract != 0)
                {
                    return (this.versionToExtract & 0xff);
                }
                int num = 10;
                if (this.AESKeySize > 0)
                {
                    num = 0x33;
                }
                else if (this.CentralHeaderRequiresZip64)
                {
                    num = 0x2d;
                }
                else if (ICSharpCode.SharpZipLib.Zip.CompressionMethod.Deflated == this.method)
                {
                    num = 20;
                }
                else if (this.IsDirectory)
                {
                    num = 20;
                }
                else if (this.IsCrypted)
                {
                    num = 20;
                }
                else if (this.HasDosAttributes(8))
                {
                    num = 11;
                }
                return num;
            }
        }

        public bool CanDecompress =>
            ((this.Version <= 0x33) && (((this.Version == 10) || ((this.Version == 11) || ((this.Version == 20) || ((this.Version == 0x2d) || (this.Version == 0x33))))) && this.IsCompressionMethodSupported()));

        public bool LocalHeaderRequiresZip64
        {
            get
            {
                bool flag = this.forceZip64_;
                if (!flag)
                {
                    ulong compressedSize = this.compressedSize;
                    if ((this.versionToExtract == 0) && this.IsCrypted)
                    {
                        compressedSize += 12;
                    }
                    flag = ((this.size >= 0xffffffffUL) || (compressedSize >= 0xffffffffUL)) ? ((this.versionToExtract == 0) || (this.versionToExtract >= 0x2d)) : false;
                }
                return flag;
            }
        }

        public bool CentralHeaderRequiresZip64 =>
            (this.LocalHeaderRequiresZip64 || (this.offset >= 0xffffffffUL));

        public long DosTime
        {
            get => 
                (((this.known & (Known.None | Known.Time)) != Known.None) ? ((long) this.dosTime) : 0L);
            set
            {
                this.dosTime = (uint) value;
                this.known |= Known.None | Known.Time;
            }
        }

        public System.DateTime DateTime
        {
            get
            {
                uint num4 = Math.Max(1, Math.Min((uint) 12, (uint) ((this.dosTime >> 0x15) & ((uint) 15))));
                uint num5 = (uint) (((this.dosTime >> 0x19) & 0x7f) + 0x7bc);
                return new System.DateTime((int) num5, (int) num4, Math.Max(1, Math.Min(System.DateTime.DaysInMonth((int) num5, (int) num4), (int) ((this.dosTime >> 0x10) & 0x1f))), (int) Math.Min((uint) 0x17, (uint) ((this.dosTime >> 11) & ((uint) 0x1f))), (int) Math.Min((uint) 0x3b, (uint) ((this.dosTime >> 5) & ((uint) 0x3f))), (int) Math.Min(0x3b, (uint) (2 * (this.dosTime & 0x1f))));
            }
            set
            {
                uint year = (uint) value.Year;
                uint month = (uint) value.Month;
                uint day = (uint) value.Day;
                uint hour = (uint) value.Hour;
                uint minute = (uint) value.Minute;
                uint second = (uint) value.Second;
                if (year < 0x7bc)
                {
                    year = 0x7bc;
                    month = 1;
                    day = 1;
                    hour = 0;
                    minute = 0;
                    second = 0;
                }
                else if (year > 0x83b)
                {
                    year = 0x83b;
                    month = 12;
                    day = 0x1f;
                    hour = 0x17;
                    minute = 0x3b;
                    second = 0x3b;
                }
                this.DosTime = (long) ((ulong) ((((((((year - 0x7bc) & 0x7f) << 0x19) | (month << 0x15)) | (day << 0x10)) | (hour << 11)) | (minute << 5)) | (second >> 1)));
            }
        }

        public string Name =>
            this.name;

        public long Size
        {
            get => 
                (((this.known & (Known.None | Known.Size)) != Known.None) ? ((long) this.size) : -1L);
            set
            {
                this.size = (ulong) value;
                this.known |= Known.None | Known.Size;
            }
        }

        public long CompressedSize
        {
            get => 
                (((this.known & Known.CompressedSize) != Known.None) ? ((long) this.compressedSize) : -1L);
            set
            {
                this.compressedSize = (ulong) value;
                this.known |= Known.CompressedSize;
            }
        }

        public long Crc
        {
            get => 
                (((this.known & Known.Crc) != Known.None) ? ((long) (this.crc & 0xffffffffUL)) : -1L);
            set
            {
                if ((this.crc & 18446744069414584320UL) != 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.crc = (uint) value;
                this.known |= Known.Crc;
            }
        }

        public ICSharpCode.SharpZipLib.Zip.CompressionMethod CompressionMethod
        {
            get => 
                this.method;
            set
            {
                if (!IsCompressionMethodSupported(value))
                {
                    throw new NotSupportedException("Compression method not supported");
                }
                this.method = value;
            }
        }

        internal ICSharpCode.SharpZipLib.Zip.CompressionMethod CompressionMethodForHeader =>
            ((this.AESKeySize > 0) ? ICSharpCode.SharpZipLib.Zip.CompressionMethod.WinZipAES : this.method);

        public byte[] ExtraData
        {
            get => 
                this.extra;
            set
            {
                if (value == null)
                {
                    this.extra = null;
                }
                else
                {
                    if (value.Length > 0xffff)
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }
                    this.extra = new byte[value.Length];
                    Array.Copy(value, 0, this.extra, 0, value.Length);
                }
            }
        }

        public int AESKeySize
        {
            get
            {
                switch (this._aesEncryptionStrength)
                {
                    case 0:
                        return 0;

                    case 1:
                        return 0x80;

                    case 2:
                        return 0xc0;

                    case 3:
                        return 0x100;
                }
                throw new ZipException("Invalid AESEncryptionStrength " + this._aesEncryptionStrength);
            }
            set
            {
                if (value == 0)
                {
                    this._aesEncryptionStrength = 0;
                }
                else if (value == 0x80)
                {
                    this._aesEncryptionStrength = 1;
                }
                else
                {
                    if (value != 0x100)
                    {
                        throw new ZipException("AESKeySize must be 0, 128 or 256: " + value);
                    }
                    this._aesEncryptionStrength = 3;
                }
            }
        }

        internal byte AESEncryptionStrength =>
            ((byte) this._aesEncryptionStrength);

        internal int AESSaltLen =>
            (this.AESKeySize / 0x10);

        internal int AESOverheadSize =>
            (12 + this.AESSaltLen);

        public string Comment
        {
            get => 
                this.comment;
            set
            {
                if ((value != null) && (value.Length > 0xffff))
                {
                    throw new ArgumentOutOfRangeException("value", "cannot exceed 65535");
                }
                this.comment = value;
            }
        }

        public bool IsDirectory
        {
            get
            {
                int length = this.name.Length;
                return (((length <= 0) || ((this.name[length - 1] != '/') && (this.name[length - 1] != '\\'))) ? this.HasDosAttributes(0x10) : true);
            }
        }

        public bool IsFile =>
            (!this.IsDirectory && !this.HasDosAttributes(8));

        [Flags]
        private enum Known : byte
        {
            None = 0,
            Size = 1,
            CompressedSize = 2,
            Crc = 4,
            Time = 8,
            ExternalAttributes = 0x10
        }
    }
}

