namespace ICSharpCode.SharpZipLib.Tar
{
    using System;
    using System.IO;

    internal class TarOutputStream : Stream
    {
        private long currBytes;
        private int assemblyBufferLength;
        private bool isClosed;
        protected long currSize;
        protected byte[] blockBuffer;
        protected byte[] assemblyBuffer;
        protected TarBuffer buffer;
        protected Stream outputStream;

        public TarOutputStream(Stream outputStream) : this(outputStream, 20)
        {
        }

        public TarOutputStream(Stream outputStream, int blockFactor)
        {
            if (outputStream == null)
            {
                throw new ArgumentNullException("outputStream");
            }
            this.outputStream = outputStream;
            this.buffer = TarBuffer.CreateOutputTarBuffer(outputStream, blockFactor);
            this.assemblyBuffer = new byte[0x200];
            this.blockBuffer = new byte[0x200];
        }

        public override void Close()
        {
            if (!this.isClosed)
            {
                this.isClosed = true;
                this.Finish();
                this.buffer.Close();
            }
        }

        public void CloseEntry()
        {
            if (this.assemblyBufferLength > 0)
            {
                Array.Clear(this.assemblyBuffer, this.assemblyBufferLength, this.assemblyBuffer.Length - this.assemblyBufferLength);
                this.buffer.WriteBlock(this.assemblyBuffer);
                this.currBytes += this.assemblyBufferLength;
                this.assemblyBufferLength = 0;
            }
            if (this.currBytes < this.currSize)
            {
                throw new TarException($"Entry closed at '{this.currBytes}' before the '{this.currSize}' bytes specified in the header were written");
            }
        }

        public void Finish()
        {
            if (this.IsEntryOpen)
            {
                this.CloseEntry();
            }
            this.WriteEofBlock();
        }

        public override void Flush()
        {
            this.outputStream.Flush();
        }

        [Obsolete("Use RecordSize property instead")]
        public int GetRecordSize() => 
            this.buffer.RecordSize;

        public void PutNextEntry(TarEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException("entry");
            }
            if (entry.TarHeader.Name.Length >= 100)
            {
                TarHeader header = new TarHeader {
                    TypeFlag = 0x4c
                };
                header.Name = header.Name + "././@LongLink";
                header.UserId = 0;
                header.GroupId = 0;
                header.GroupName = "";
                header.UserName = "";
                header.LinkName = "";
                header.Size = entry.TarHeader.Name.Length + 1;
                header.WriteHeader(this.blockBuffer);
                this.buffer.WriteBlock(this.blockBuffer);
                int nameOffset = 0;
                while (nameOffset < entry.TarHeader.Name.Length)
                {
                    Array.Clear(this.blockBuffer, 0, this.blockBuffer.Length);
                    TarHeader.GetAsciiBytes(entry.TarHeader.Name, nameOffset, this.blockBuffer, 0, 0x200);
                    nameOffset += 0x200;
                    this.buffer.WriteBlock(this.blockBuffer);
                }
            }
            entry.WriteEntryHeader(this.blockBuffer);
            this.buffer.WriteBlock(this.blockBuffer);
            this.currBytes = 0L;
            this.currSize = entry.IsDirectory ? 0L : entry.Size;
        }

        public override int Read(byte[] buffer, int offset, int count) => 
            this.outputStream.Read(buffer, offset, count);

        public override int ReadByte() => 
            this.outputStream.ReadByte();

        public override long Seek(long offset, SeekOrigin origin) => 
            this.outputStream.Seek(offset, origin);

        public override void SetLength(long value)
        {
            this.outputStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "Cannot be negative");
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException("offset and count combination is invalid");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Cannot be negative");
            }
            if ((this.currBytes + count) > this.currSize)
            {
                string message = $"request to write '{count}' bytes exceeds size in header of '{this.currSize}' bytes";
                throw new ArgumentOutOfRangeException("count", message);
            }
            if (this.assemblyBufferLength > 0)
            {
                if ((this.assemblyBufferLength + count) < this.blockBuffer.Length)
                {
                    Array.Copy(buffer, offset, this.assemblyBuffer, this.assemblyBufferLength, count);
                    offset += count;
                    this.assemblyBufferLength += count;
                    count -= count;
                }
                else
                {
                    int length = this.blockBuffer.Length - this.assemblyBufferLength;
                    Array.Copy(this.assemblyBuffer, 0, this.blockBuffer, 0, this.assemblyBufferLength);
                    Array.Copy(buffer, offset, this.blockBuffer, this.assemblyBufferLength, length);
                    this.buffer.WriteBlock(this.blockBuffer);
                    this.currBytes += this.blockBuffer.Length;
                    offset += length;
                    count -= length;
                    this.assemblyBufferLength = 0;
                }
            }
            while (count > 0)
            {
                if (count < this.blockBuffer.Length)
                {
                    Array.Copy(buffer, offset, this.assemblyBuffer, this.assemblyBufferLength, count);
                    this.assemblyBufferLength += count;
                    return;
                }
                this.buffer.WriteBlock(buffer, offset);
                int length = this.blockBuffer.Length;
                this.currBytes += length;
                count -= length;
                offset += length;
            }
        }

        public override void WriteByte(byte value)
        {
            byte[] buffer = new byte[] { value };
            this.Write(buffer, 0, 1);
        }

        private void WriteEofBlock()
        {
            Array.Clear(this.blockBuffer, 0, this.blockBuffer.Length);
            this.buffer.WriteBlock(this.blockBuffer);
        }

        public bool IsStreamOwner
        {
            get => 
                this.buffer.IsStreamOwner;
            set => 
                (this.buffer.IsStreamOwner = value);
        }

        public override bool CanRead =>
            this.outputStream.CanRead;

        public override bool CanSeek =>
            this.outputStream.CanSeek;

        public override bool CanWrite =>
            this.outputStream.CanWrite;

        public override long Length =>
            this.outputStream.Length;

        public override long Position
        {
            get => 
                this.outputStream.Position;
            set => 
                (this.outputStream.Position = value);
        }

        public int RecordSize =>
            this.buffer.RecordSize;

        private bool IsEntryOpen =>
            (this.currBytes < this.currSize);
    }
}

