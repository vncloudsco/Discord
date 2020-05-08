namespace ICSharpCode.SharpZipLib.Zip
{
    using System;
    using System.IO;

    internal class NTTaggedData : ITaggedData
    {
        private DateTime _lastAccessTime = DateTime.FromFileTime(0L);
        private DateTime _lastModificationTime = DateTime.FromFileTime(0L);
        private DateTime _createTime = DateTime.FromFileTime(0L);

        public byte[] GetData()
        {
            byte[] buffer;
            using (MemoryStream stream = new MemoryStream())
            {
                using (ZipHelperStream stream2 = new ZipHelperStream(stream))
                {
                    stream2.IsStreamOwner = false;
                    stream2.WriteLEInt(0);
                    stream2.WriteLEShort(1);
                    stream2.WriteLEShort(0x18);
                    stream2.WriteLELong(this._lastModificationTime.ToFileTime());
                    stream2.WriteLELong(this._lastAccessTime.ToFileTime());
                    stream2.WriteLELong(this._createTime.ToFileTime());
                    buffer = stream.ToArray();
                }
            }
            return buffer;
        }

        public static bool IsValidValue(DateTime value)
        {
            bool flag = true;
            try
            {
                value.ToFileTimeUtc();
            }
            catch
            {
                flag = false;
            }
            return flag;
        }

        public void SetData(byte[] data, int index, int count)
        {
            using (MemoryStream stream = new MemoryStream(data, index, count, false))
            {
                using (ZipHelperStream stream2 = new ZipHelperStream(stream))
                {
                    stream2.ReadLEInt();
                    while (stream2.Position < stream2.Length)
                    {
                        int num = stream2.ReadLEShort();
                        if (stream2.ReadLEShort() == 1)
                        {
                            if (num >= 0x18)
                            {
                                long fileTime = stream2.ReadLELong();
                                this._lastModificationTime = DateTime.FromFileTime(fileTime);
                                long num3 = stream2.ReadLELong();
                                this._lastAccessTime = DateTime.FromFileTime(num3);
                                long num4 = stream2.ReadLELong();
                                this._createTime = DateTime.FromFileTime(num4);
                            }
                            break;
                        }
                        stream2.Seek((long) num, SeekOrigin.Current);
                    }
                }
            }
        }

        public short TagID =>
            10;

        public DateTime LastModificationTime
        {
            get => 
                this._lastModificationTime;
            set
            {
                if (!IsValidValue(value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._lastModificationTime = value;
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
                this._createTime = value;
            }
        }

        public DateTime LastAccessTime
        {
            get => 
                this._lastAccessTime;
            set
            {
                if (!IsValidValue(value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this._lastAccessTime = value;
            }
        }
    }
}

