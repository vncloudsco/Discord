namespace ICSharpCode.SharpZipLib.Zip
{
    using System;
    using System.IO;

    internal class ZipHelperStream : Stream
    {
        private bool isOwner_;
        private Stream stream_;

        public ZipHelperStream(Stream stream)
        {
            this.stream_ = stream;
        }

        public ZipHelperStream(string name)
        {
            this.stream_ = new FileStream(name, FileMode.Open, FileAccess.ReadWrite);
            this.isOwner_ = true;
        }

        public override void Close()
        {
            Stream stream = this.stream_;
            this.stream_ = null;
            if (this.isOwner_ && (stream != null))
            {
                this.isOwner_ = false;
                stream.Close();
            }
        }

        public override void Flush()
        {
            this.stream_.Flush();
        }

        public long LocateBlockWithSignature(int signature, long endLocation, int minimumBlockSize, int maximumVariableData)
        {
            long num = endLocation - minimumBlockSize;
            if (num >= 0L)
            {
                long num2 = Math.Max((long) (num - maximumVariableData), (long) 0L);
                while (num >= num2)
                {
                    long offset = num;
                    num = offset - 1L;
                    this.Seek(offset, SeekOrigin.Begin);
                    if (this.ReadLEInt() == signature)
                    {
                        return this.Position;
                    }
                }
            }
            return -1L;
        }

        public override int Read(byte[] buffer, int offset, int count) => 
            this.stream_.Read(buffer, offset, count);

        public void ReadDataDescriptor(bool zip64, DescriptorData data)
        {
            if (this.ReadLEInt() != 0x8074b50)
            {
                throw new ZipException("Data descriptor signature not found");
            }
            data.Crc = this.ReadLEInt();
            if (zip64)
            {
                data.CompressedSize = this.ReadLELong();
                data.Size = this.ReadLELong();
            }
            else
            {
                data.CompressedSize = this.ReadLEInt();
                data.Size = this.ReadLEInt();
            }
        }

        public int ReadLEInt() => 
            (this.ReadLEShort() | (this.ReadLEShort() << 0x10));

        public long ReadLELong() => 
            (((long) ((ulong) this.ReadLEInt())) | (this.ReadLEInt() << 0x20));

        public int ReadLEShort()
        {
            int num1 = this.stream_.ReadByte();
            if (num1 < 0)
            {
                throw new EndOfStreamException();
            }
            int num = this.stream_.ReadByte();
            if (num < 0)
            {
                throw new EndOfStreamException();
            }
            return (num1 | (num << 8));
        }

        public override long Seek(long offset, SeekOrigin origin) => 
            this.stream_.Seek(offset, origin);

        public override void SetLength(long value)
        {
            this.stream_.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.stream_.Write(buffer, offset, count);
        }

        public int WriteDataDescriptor(ZipEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }
            int num = 0;
            if ((entry.Flags & 8) != 0)
            {
                this.WriteLEInt(0x8074b50);
                this.WriteLEInt((int) entry.Crc);
                num += 8;
                if (entry.LocalHeaderRequiresZip64)
                {
                    this.WriteLELong(entry.CompressedSize);
                    this.WriteLELong(entry.Size);
                    num += 0x10;
                }
                else
                {
                    this.WriteLEInt((int) entry.CompressedSize);
                    this.WriteLEInt((int) entry.Size);
                    num += 8;
                }
            }
            return num;
        }

        public void WriteEndOfCentralDirectory(long noOfEntries, long sizeEntries, long startOfCentralDirectory, byte[] comment)
        {
            if ((noOfEntries >= 0xffffL) || ((startOfCentralDirectory >= 0xffffffffUL) || (sizeEntries >= 0xffffffffUL)))
            {
                this.WriteZip64EndOfCentralDirectory(noOfEntries, sizeEntries, startOfCentralDirectory);
            }
            this.WriteLEInt(0x6054b50);
            this.WriteLEShort(0);
            this.WriteLEShort(0);
            if (noOfEntries >= 0xffffL)
            {
                this.WriteLEUshort(0xffff);
                this.WriteLEUshort(0xffff);
            }
            else
            {
                this.WriteLEShort((short) noOfEntries);
                this.WriteLEShort((short) noOfEntries);
            }
            if (sizeEntries >= 0xffffffffUL)
            {
                this.WriteLEUint(uint.MaxValue);
            }
            else
            {
                this.WriteLEInt((int) sizeEntries);
            }
            if (startOfCentralDirectory >= 0xffffffffUL)
            {
                this.WriteLEUint(uint.MaxValue);
            }
            else
            {
                this.WriteLEInt((int) startOfCentralDirectory);
            }
            int num = (comment != null) ? comment.Length : 0;
            if (num > 0xffff)
            {
                throw new ZipException($"Comment length({num}) is too long can only be 64K");
            }
            this.WriteLEShort(num);
            if (num > 0)
            {
                this.Write(comment, 0, comment.Length);
            }
        }

        public void WriteLEInt(int value)
        {
            this.WriteLEShort(value);
            this.WriteLEShort(value >> 0x10);
        }

        public void WriteLELong(long value)
        {
            this.WriteLEInt((int) value);
            this.WriteLEInt((int) (value >> 0x20));
        }

        public void WriteLEShort(int value)
        {
            this.stream_.WriteByte((byte) (value & 0xff));
            this.stream_.WriteByte((byte) ((value >> 8) & 0xff));
        }

        public void WriteLEUint(uint value)
        {
            this.WriteLEUshort((ushort) (value & 0xffff));
            this.WriteLEUshort((ushort) (value >> 0x10));
        }

        public void WriteLEUlong(ulong value)
        {
            this.WriteLEUint((uint) (value & 0xffffffffUL));
            this.WriteLEUint((uint) (value >> 0x20));
        }

        public void WriteLEUshort(ushort value)
        {
            this.stream_.WriteByte((byte) (value & 0xff));
            this.stream_.WriteByte((byte) (value >> 8));
        }

        private void WriteLocalHeader(ZipEntry entry, EntryPatchData patchData)
        {
            CompressionMethod compressionMethod = entry.CompressionMethod;
            bool flag = true;
            bool flag2 = false;
            this.WriteLEInt(0x4034b50);
            this.WriteLEShort(entry.Version);
            this.WriteLEShort(entry.Flags);
            this.WriteLEShort((byte) compressionMethod);
            this.WriteLEInt((int) entry.DosTime);
            if (flag)
            {
                this.WriteLEInt((int) entry.Crc);
                if (entry.LocalHeaderRequiresZip64)
                {
                    this.WriteLEInt(-1);
                    this.WriteLEInt(-1);
                }
                else
                {
                    this.WriteLEInt(entry.IsCrypted ? (((int) entry.CompressedSize) + 12) : ((int) entry.CompressedSize));
                    this.WriteLEInt((int) entry.Size);
                }
            }
            else
            {
                if (patchData != null)
                {
                    patchData.CrcPatchOffset = this.stream_.Position;
                }
                this.WriteLEInt(0);
                if (patchData != null)
                {
                    patchData.SizePatchOffset = this.stream_.Position;
                }
                if (entry.LocalHeaderRequiresZip64 & flag2)
                {
                    this.WriteLEInt(-1);
                    this.WriteLEInt(-1);
                }
                else
                {
                    this.WriteLEInt(0);
                    this.WriteLEInt(0);
                }
            }
            byte[] buffer = ZipConstants.ConvertToArray(entry.Flags, entry.Name);
            if (buffer.Length > 0xffff)
            {
                throw new ZipException("Entry name too long.");
            }
            ZipExtraData data = new ZipExtraData(entry.ExtraData);
            if (!entry.LocalHeaderRequiresZip64 || !(flag | flag2))
            {
                data.Delete(1);
            }
            else
            {
                data.StartNewEntry();
                if (flag)
                {
                    data.AddLeLong(entry.Size);
                    data.AddLeLong(entry.CompressedSize);
                }
                else
                {
                    data.AddLeLong(-1L);
                    data.AddLeLong(-1L);
                }
                data.AddNewEntry(1);
                if (!data.Find(1))
                {
                    throw new ZipException("Internal error cant find extra data");
                }
                if (patchData != null)
                {
                    patchData.SizePatchOffset = data.CurrentReadIndex;
                }
            }
            byte[] entryData = data.GetEntryData();
            this.WriteLEShort(buffer.Length);
            this.WriteLEShort(entryData.Length);
            if (buffer.Length != 0)
            {
                this.stream_.Write(buffer, 0, buffer.Length);
            }
            if (entry.LocalHeaderRequiresZip64 & flag2)
            {
                patchData.SizePatchOffset += this.stream_.Position;
            }
            if (entryData.Length != 0)
            {
                this.stream_.Write(entryData, 0, entryData.Length);
            }
        }

        public void WriteZip64EndOfCentralDirectory(long noOfEntries, long sizeEntries, long centralDirOffset)
        {
            long position = this.stream_.Position;
            this.WriteLEInt(0x6064b50);
            this.WriteLELong((long) 0x2c);
            this.WriteLEShort(0x33);
            this.WriteLEShort(0x2d);
            this.WriteLEInt(0);
            this.WriteLEInt(0);
            this.WriteLELong(noOfEntries);
            this.WriteLELong(noOfEntries);
            this.WriteLELong(sizeEntries);
            this.WriteLELong(centralDirOffset);
            this.WriteLEInt(0x7064b50);
            this.WriteLEInt(0);
            this.WriteLELong(position);
            this.WriteLEInt(1);
        }

        public bool IsStreamOwner
        {
            get => 
                this.isOwner_;
            set => 
                (this.isOwner_ = value);
        }

        public override bool CanRead =>
            this.stream_.CanRead;

        public override bool CanSeek =>
            this.stream_.CanSeek;

        public override bool CanTimeout =>
            this.stream_.CanTimeout;

        public override long Length =>
            this.stream_.Length;

        public override long Position
        {
            get => 
                this.stream_.Position;
            set => 
                (this.stream_.Position = value);
        }

        public override bool CanWrite =>
            this.stream_.CanWrite;
    }
}

