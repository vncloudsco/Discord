namespace ICSharpCode.SharpZipLib.Zip
{
    using ICSharpCode.SharpZipLib.Checksums;
    using ICSharpCode.SharpZipLib.Encryption;
    using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal class ZipInputStream : InflaterInputStream
    {
        private ReadDataHandler internalReader;
        private Crc32 crc;
        private ZipEntry entry;
        private long size;
        private int method;
        private int flags;
        private string password;

        public ZipInputStream(Stream baseInputStream) : base(baseInputStream, new Inflater(true))
        {
            this.crc = new Crc32();
            this.internalReader = new ReadDataHandler(this.ReadingNotAvailable);
        }

        public ZipInputStream(Stream baseInputStream, int bufferSize) : base(baseInputStream, new Inflater(true), bufferSize)
        {
            this.crc = new Crc32();
            this.internalReader = new ReadDataHandler(this.ReadingNotAvailable);
        }

        private int BodyRead(byte[] buffer, int offset, int count)
        {
            if (this.crc == null)
            {
                throw new InvalidOperationException("Closed");
            }
            if ((this.entry == null) || (count <= 0))
            {
                return 0;
            }
            if ((offset + count) > buffer.Length)
            {
                throw new ArgumentException("Offset + count exceeds buffer size");
            }
            bool flag = false;
            int method = this.method;
            if (method == 0)
            {
                if ((count > base.csize) && (base.csize >= 0L))
                {
                    count = (int) base.csize;
                }
                if (count > 0)
                {
                    count = base.inputBuffer.ReadClearTextBuffer(buffer, offset, count);
                    if (count > 0)
                    {
                        base.csize -= count;
                        this.size -= count;
                    }
                }
                if (base.csize == 0)
                {
                    flag = true;
                }
                else if (count < 0)
                {
                    throw new ZipException("EOF in stored block");
                }
            }
            else if (method == 8)
            {
                count = base.Read(buffer, offset, count);
                if (count <= 0)
                {
                    if (!base.inf.IsFinished)
                    {
                        throw new ZipException("Inflater not finished!");
                    }
                    base.inputBuffer.Available = base.inf.RemainingInput;
                    if (((this.flags & 8) == 0) && (((base.inf.TotalIn != base.csize) && ((base.csize != 0xffffffffUL) && (base.csize != -1L))) || (base.inf.TotalOut != this.size)))
                    {
                        object[] objArray1 = new object[] { "Size mismatch: ", base.csize, ";", this.size, " <-> ", base.inf.TotalIn, ";", base.inf.TotalOut };
                        throw new ZipException(string.Concat(objArray1));
                    }
                    base.inf.Reset();
                    flag = true;
                }
            }
            if (count > 0)
            {
                this.crc.Update(buffer, offset, count);
            }
            if (flag)
            {
                this.CompleteCloseEntry(true);
            }
            return count;
        }

        public override void Close()
        {
            this.internalReader = new ReadDataHandler(this.ReadingNotAvailable);
            this.crc = null;
            this.entry = null;
            base.Close();
        }

        public void CloseEntry()
        {
            if (this.crc == null)
            {
                throw new InvalidOperationException("Closed");
            }
            if (this.entry != null)
            {
                if (this.method == 8)
                {
                    if ((this.flags & 8) != 0)
                    {
                        byte[] buffer = new byte[0x1000];
                        while (this.Read(buffer, 0, buffer.Length) > 0)
                        {
                        }
                        return;
                    }
                    base.csize -= base.inf.TotalIn;
                    base.inputBuffer.Available += base.inf.RemainingInput;
                }
                if ((base.inputBuffer.Available > base.csize) && (base.csize >= 0L))
                {
                    base.inputBuffer.Available -= (int) base.csize;
                }
                else
                {
                    base.csize -= base.inputBuffer.Available;
                    base.inputBuffer.Available = 0;
                    while (base.csize != 0)
                    {
                        long num = base.Skip(base.csize);
                        if (num <= 0L)
                        {
                            throw new ZipException("Zip archive ends early.");
                        }
                        base.csize -= num;
                    }
                }
                this.CompleteCloseEntry(false);
            }
        }

        private void CompleteCloseEntry(bool testCrc)
        {
            base.StopDecrypting();
            if ((this.flags & 8) != 0)
            {
                this.ReadDataDescriptor();
            }
            this.size = 0L;
            if (testCrc && (((((ulong) this.crc.Value) & 0xffffffffUL) != this.entry.Crc) && (this.entry.Crc != -1L)))
            {
                throw new ZipException("CRC mismatch");
            }
            this.crc.Reset();
            if (this.method == 8)
            {
                base.inf.Reset();
            }
            this.entry = null;
        }

        public ZipEntry GetNextEntry()
        {
            if (this.crc == null)
            {
                throw new InvalidOperationException("Closed.");
            }
            if (this.entry != null)
            {
                this.CloseEntry();
            }
            int num = base.inputBuffer.ReadLeInt();
            if ((num == 0x2014b50) || ((num == 0x6054b50) || ((num == 0x5054b50) || ((num == 0x7064b50) || (num == 0x6064b50)))))
            {
                this.Close();
                return null;
            }
            if ((num == 0x30304b50) || (num == 0x8074b50))
            {
                num = base.inputBuffer.ReadLeInt();
            }
            if (num != 0x4034b50)
            {
                throw new ZipException("Wrong Local header signature: 0x" + $"{num:X}");
            }
            this.flags = base.inputBuffer.ReadLeShort();
            this.method = base.inputBuffer.ReadLeShort();
            uint num3 = (uint) base.inputBuffer.ReadLeInt();
            int num4 = base.inputBuffer.ReadLeInt();
            base.csize = base.inputBuffer.ReadLeInt();
            this.size = base.inputBuffer.ReadLeInt();
            int num5 = base.inputBuffer.ReadLeShort();
            bool flag = (this.flags & 1) == 1;
            byte[] buffer = new byte[base.inputBuffer.ReadLeShort()];
            base.inputBuffer.ReadRawBuffer(buffer);
            string name = ZipConstants.ConvertToStringExt(this.flags, buffer);
            this.entry = new ZipEntry(name, (short) base.inputBuffer.ReadLeShort());
            this.entry.Flags = this.flags;
            this.entry.CompressionMethod = (CompressionMethod) this.method;
            if ((this.flags & 8) == 0)
            {
                this.entry.Crc = num4 & 0xffffffffUL;
                this.entry.Size = (long) (((ulong) this.size) & 0xffffffffUL);
                this.entry.CompressedSize = (long) (((ulong) base.csize) & 0xffffffffUL);
                this.entry.CryptoCheckValue = (byte) ((num4 >> 0x18) & 0xff);
            }
            else
            {
                if (num4 != 0)
                {
                    this.entry.Crc = num4 & 0xffffffffUL;
                }
                if (this.size != 0)
                {
                    this.entry.Size = (long) (((ulong) this.size) & 0xffffffffUL);
                }
                if (base.csize != 0)
                {
                    this.entry.CompressedSize = (long) (((ulong) base.csize) & 0xffffffffUL);
                }
                this.entry.CryptoCheckValue = (byte) ((num3 >> 8) & 0xff);
            }
            this.entry.DosTime = num3;
            if (num5 > 0)
            {
                byte[] buffer2 = new byte[num5];
                base.inputBuffer.ReadRawBuffer(buffer2);
                this.entry.ExtraData = buffer2;
            }
            this.entry.ProcessExtraData(true);
            if (this.entry.CompressedSize >= 0L)
            {
                base.csize = this.entry.CompressedSize;
            }
            if (this.entry.Size >= 0L)
            {
                this.size = this.entry.Size;
            }
            if ((this.method == 0) && ((!flag && (base.csize != this.size)) || (flag && ((base.csize - 12) != this.size))))
            {
                throw new ZipException("Stored, but compressed != uncompressed");
            }
            this.internalReader = !this.entry.IsCompressionMethodSupported() ? new ReadDataHandler(this.ReadingNotSupported) : new ReadDataHandler(this.InitialRead);
            return this.entry;
        }

        private int InitialRead(byte[] destination, int offset, int count)
        {
            if (!this.CanDecompressEntry)
            {
                throw new ZipException("Library cannot extract this entry. Version required is (" + this.entry.Version.ToString() + ")");
            }
            if (!this.entry.IsCrypted)
            {
                base.inputBuffer.CryptoTransform = null;
            }
            else
            {
                if (this.password == null)
                {
                    throw new ZipException("No password set.");
                }
                base.inputBuffer.CryptoTransform = new PkzipClassicManaged().CreateDecryptor(PkzipClassic.GenerateKeys(ZipConstants.ConvertToArray(this.password)), null);
                byte[] outBuffer = new byte[12];
                base.inputBuffer.ReadClearTextBuffer(outBuffer, 0, 12);
                if (outBuffer[11] != this.entry.CryptoCheckValue)
                {
                    throw new ZipException("Invalid password");
                }
                if (base.csize >= 12)
                {
                    base.csize -= 12;
                }
                else if ((this.entry.Flags & 8) == 0)
                {
                    throw new ZipException($"Entry compressed size {base.csize} too small for encryption");
                }
            }
            if ((base.csize <= 0L) && ((this.flags & 8) == 0))
            {
                this.internalReader = new ReadDataHandler(this.ReadingNotAvailable);
                return 0;
            }
            if ((this.method == 8) && (base.inputBuffer.Available > 0))
            {
                base.inputBuffer.SetInflaterInput(base.inf);
            }
            this.internalReader = new ReadDataHandler(this.BodyRead);
            return this.BodyRead(destination, offset, count);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "Cannot be negative");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Cannot be negative");
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException("Invalid offset/count combination");
            }
            return this.internalReader(buffer, offset, count);
        }

        public override int ReadByte()
        {
            byte[] buffer = new byte[1];
            return ((this.Read(buffer, 0, 1) > 0) ? (buffer[0] & 0xff) : -1);
        }

        private void ReadDataDescriptor()
        {
            if (base.inputBuffer.ReadLeInt() != 0x8074b50)
            {
                throw new ZipException("Data descriptor signature not found");
            }
            this.entry.Crc = base.inputBuffer.ReadLeInt() & 0xffffffffUL;
            if (this.entry.LocalHeaderRequiresZip64)
            {
                base.csize = base.inputBuffer.ReadLeLong();
                this.size = base.inputBuffer.ReadLeLong();
            }
            else
            {
                base.csize = base.inputBuffer.ReadLeInt();
                this.size = base.inputBuffer.ReadLeInt();
            }
            this.entry.CompressedSize = base.csize;
            this.entry.Size = this.size;
        }

        private int ReadingNotAvailable(byte[] destination, int offset, int count)
        {
            throw new InvalidOperationException("Unable to read from this stream");
        }

        private int ReadingNotSupported(byte[] destination, int offset, int count)
        {
            throw new ZipException("The compression method for this entry is not supported");
        }

        public string Password
        {
            get => 
                this.password;
            set => 
                (this.password = value);
        }

        public bool CanDecompressEntry =>
            ((this.entry != null) && this.entry.CanDecompress);

        public override int Available =>
            ((this.entry != null) ? 1 : 0);

        public override long Length
        {
            get
            {
                if (this.entry == null)
                {
                    throw new InvalidOperationException("No current entry");
                }
                if (this.entry.Size < 0L)
                {
                    throw new ZipException("Length not available for the current entry");
                }
                return this.entry.Size;
            }
        }

        private delegate int ReadDataHandler(byte[] b, int offset, int length);
    }
}

