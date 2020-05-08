namespace ICSharpCode.SharpZipLib.Zip
{
    using System;
    using System.IO;

    internal class ExtendedUnixData : ITaggedData
    {
        private Flags _flags;
        private DateTime _modificationTime = new DateTime(0x7b2, 1, 1);
        private DateTime _lastAccessTime = new DateTime(0x7b2, 1, 1);
        private DateTime _createTime = new DateTime(0x7b2, 1, 1);

        public byte[] GetData()
        {
            byte[] buffer;
            using (MemoryStream stream = new MemoryStream())
            {
                using (ZipHelperStream stream2 = new ZipHelperStream(stream))
                {
                    stream2.IsStreamOwner = false;
                    stream2.WriteByte((byte) this._flags);
                    if (((int) (this._flags & Flags.ModificationTime)) != 0)
                    {
                        stream2.WriteLEInt((int) (this._modificationTime.ToUniversalTime() - new DateTime(0x7b2, 1, 1, 0, 0, 0).ToUniversalTime()).TotalSeconds);
                    }
                    if (((int) (this._flags & Flags.AccessTime)) != 0)
                    {
                        stream2.WriteLEInt((int) (this._lastAccessTime.ToUniversalTime() - new DateTime(0x7b2, 1, 1, 0, 0, 0).ToUniversalTime()).TotalSeconds);
                    }
                    if (((int) (this._flags & Flags.CreateTime)) != 0)
                    {
                        stream2.WriteLEInt((int) (this._createTime.ToUniversalTime() - new DateTime(0x7b2, 1, 1, 0, 0, 0).ToUniversalTime()).TotalSeconds);
                    }
                    buffer = stream.ToArray();
                }
            }
            return buffer;
        }

        public static bool IsValidValue(DateTime value) => 
            ((value >= new DateTime(0x76d, 12, 13, 20, 0x2d, 0x34)) || (value <= new DateTime(0x7f6, 1, 0x13, 3, 14, 7)));

        public void SetData(byte[] data, int index, int count)
        {
            using (MemoryStream stream = new MemoryStream(data, index, count, false))
            {
                using (ZipHelperStream stream2 = new ZipHelperStream(stream))
                {
                    this._flags = (Flags) ((byte) stream2.ReadByte());
                    if ((((int) (this._flags & Flags.ModificationTime)) != 0) && (count >= 5))
                    {
                        int seconds = stream2.ReadLEInt();
                        this._modificationTime = (new DateTime(0x7b2, 1, 1, 0, 0, 0).ToUniversalTime() + new TimeSpan(0, 0, 0, seconds, 0)).ToLocalTime();
                    }
                    if (((int) (this._flags & Flags.AccessTime)) != 0)
                    {
                        int seconds = stream2.ReadLEInt();
                        this._lastAccessTime = (new DateTime(0x7b2, 1, 1, 0, 0, 0).ToUniversalTime() + new TimeSpan(0, 0, 0, seconds, 0)).ToLocalTime();
                    }
                    if (((int) (this._flags & Flags.CreateTime)) != 0)
                    {
                        int seconds = stream2.ReadLEInt();
                        this._createTime = (new DateTime(0x7b2, 1, 1, 0, 0, 0).ToUniversalTime() + new TimeSpan(0, 0, 0, seconds, 0)).ToLocalTime();
                    }
                }
            }
        }

        public short TagID =>
            0x5455;

        public DateTime ModificationTime
        {
            get => 
                this._modificationTime;
            set
            {
                if (!IsValidValue(value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._flags |= Flags.ModificationTime;
                this._modificationTime = value;
            }
        }

        public DateTime AccessTime
        {
            get => 
                this._lastAccessTime;
            set
            {
                if (!IsValidValue(value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._flags |= Flags.AccessTime;
                this._lastAccessTime = value;
            }
        }

        public DateTime CreateTime
        {
            get => 
                this._createTime;
            set
            {
                if (!IsValidValue(value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._flags |= Flags.CreateTime;
                this._createTime = value;
            }
        }

        private Flags Include
        {
            get => 
                this._flags;
            set => 
                (this._flags = value);
        }

        [Flags]
        public enum Flags : byte
        {
            ModificationTime = 1,
            AccessTime = 2,
            CreateTime = 4
        }
    }
}

