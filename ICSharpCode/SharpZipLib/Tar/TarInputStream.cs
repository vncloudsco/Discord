namespace ICSharpCode.SharpZipLib.Tar
{
    using System;
    using System.IO;
    using System.Text;

    internal class TarInputStream : Stream
    {
        protected bool hasHitEOF;
        protected long entrySize;
        protected long entryOffset;
        protected byte[] readBuffer;
        protected TarBuffer tarBuffer;
        private TarEntry currentEntry;
        protected IEntryFactory entryFactory;
        private readonly Stream inputStream;

        public TarInputStream(Stream inputStream) : this(inputStream, 20)
        {
        }

        public TarInputStream(Stream inputStream, int blockFactor)
        {
            this.inputStream = inputStream;
            this.tarBuffer = TarBuffer.CreateInputTarBuffer(inputStream, blockFactor);
        }

        public override void Close()
        {
            this.tarBuffer.Close();
        }

        public void CopyEntryContents(Stream outputStream)
        {
            byte[] buffer = new byte[0x8000];
            while (true)
            {
                int count = this.Read(buffer, 0, buffer.Length);
                if (count <= 0)
                {
                    return;
                }
                outputStream.Write(buffer, 0, count);
            }
        }

        public override void Flush()
        {
            this.inputStream.Flush();
        }

        public TarEntry GetNextEntry()
        {
            if (this.hasHitEOF)
            {
                return null;
            }
            if (this.currentEntry != null)
            {
                this.SkipToNextEntry();
            }
            byte[] block = this.tarBuffer.ReadBlock();
            if (block == null)
            {
                this.hasHitEOF = true;
            }
            else if (TarBuffer.IsEndOfArchiveBlock(block))
            {
                this.hasHitEOF = true;
            }
            if (this.hasHitEOF)
            {
                this.currentEntry = null;
            }
            else
            {
                try
                {
                    TarHeader header = new TarHeader();
                    header.ParseBuffer(block);
                    if (!header.IsChecksumValid)
                    {
                        throw new TarException("Header checksum is invalid");
                    }
                    this.entryOffset = 0L;
                    this.entrySize = header.Size;
                    StringBuilder builder = null;
                    if (header.TypeFlag == 0x4c)
                    {
                        byte[] buffer = new byte[0x200];
                        long entrySize = this.entrySize;
                        builder = new StringBuilder();
                        while (true)
                        {
                            if (entrySize <= 0L)
                            {
                                this.SkipToNextEntry();
                                block = this.tarBuffer.ReadBlock();
                                break;
                            }
                            int length = this.Read(buffer, 0, (entrySize > buffer.Length) ? buffer.Length : ((int) entrySize));
                            if (length == -1)
                            {
                                throw new InvalidHeaderException("Failed to read long name entry");
                            }
                            builder.Append(TarHeader.ParseName(buffer, 0, length).ToString());
                            entrySize -= length;
                        }
                    }
                    else if (header.TypeFlag == 0x67)
                    {
                        this.SkipToNextEntry();
                        block = this.tarBuffer.ReadBlock();
                    }
                    else if (header.TypeFlag == 120)
                    {
                        this.SkipToNextEntry();
                        block = this.tarBuffer.ReadBlock();
                    }
                    else if (header.TypeFlag == 0x56)
                    {
                        this.SkipToNextEntry();
                        block = this.tarBuffer.ReadBlock();
                    }
                    else if ((header.TypeFlag != 0x30) && ((header.TypeFlag != 0) && (header.TypeFlag != 0x35)))
                    {
                        this.SkipToNextEntry();
                        block = this.tarBuffer.ReadBlock();
                    }
                    if (this.entryFactory != null)
                    {
                        this.currentEntry = this.entryFactory.CreateEntry(block);
                    }
                    else
                    {
                        this.currentEntry = new TarEntry(block);
                        if (builder != null)
                        {
                            this.currentEntry.Name = builder.ToString();
                        }
                    }
                    this.entryOffset = 0L;
                    this.entrySize = this.currentEntry.Size;
                }
                catch (InvalidHeaderException exception)
                {
                    this.entrySize = 0L;
                    this.entryOffset = 0L;
                    this.currentEntry = null;
                    throw new InvalidHeaderException($"Bad header in record {this.tarBuffer.CurrentRecord} block {this.tarBuffer.CurrentBlock} {exception.Message}");
                }
            }
            return this.currentEntry;
        }

        [Obsolete("Use RecordSize property instead")]
        public int GetRecordSize() => 
            this.tarBuffer.RecordSize;

        public void Mark(int markLimit)
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            int num = 0;
            if (this.entryOffset >= this.entrySize)
            {
                return 0;
            }
            long num2 = count;
            if ((num2 + this.entryOffset) > this.entrySize)
            {
                num2 = this.entrySize - this.entryOffset;
            }
            if (this.readBuffer != null)
            {
                int length = (num2 > this.readBuffer.Length) ? this.readBuffer.Length : ((int) num2);
                Array.Copy(this.readBuffer, 0, buffer, offset, length);
                if (length >= this.readBuffer.Length)
                {
                    this.readBuffer = null;
                }
                else
                {
                    int num4 = this.readBuffer.Length - length;
                    byte[] destinationArray = new byte[num4];
                    Array.Copy(this.readBuffer, length, destinationArray, 0, num4);
                    this.readBuffer = destinationArray;
                }
                num += length;
                num2 -= length;
                offset += length;
            }
            while (num2 > 0L)
            {
                byte[] sourceArray = this.tarBuffer.ReadBlock();
                if (sourceArray == null)
                {
                    throw new TarException("unexpected EOF with " + num2 + " bytes unread");
                }
                int length = (int) num2;
                int num6 = sourceArray.Length;
                if (num6 <= length)
                {
                    length = num6;
                    Array.Copy(sourceArray, 0, buffer, offset, num6);
                }
                else
                {
                    Array.Copy(sourceArray, 0, buffer, offset, length);
                    this.readBuffer = new byte[num6 - length];
                    Array.Copy(sourceArray, length, this.readBuffer, 0, num6 - length);
                }
                num += length;
                num2 -= length;
                offset += length;
            }
            this.entryOffset += num;
            return num;
        }

        public override int ReadByte()
        {
            byte[] buffer = new byte[1];
            return ((this.Read(buffer, 0, 1) > 0) ? buffer[0] : -1);
        }

        public void Reset()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("TarInputStream Seek not supported");
        }

        public void SetEntryFactory(IEntryFactory factory)
        {
            this.entryFactory = factory;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("TarInputStream SetLength not supported");
        }

        public void Skip(long skipCount)
        {
            byte[] buffer = new byte[0x2000];
            long num = skipCount;
            while (true)
            {
                if (num > 0L)
                {
                    int num3 = this.Read(buffer, 0, (num > buffer.Length) ? buffer.Length : ((int) num));
                    if (num3 != -1)
                    {
                        num -= num3;
                        continue;
                    }
                }
                return;
            }
        }

        private void SkipToNextEntry()
        {
            long skipCount = this.entrySize - this.entryOffset;
            if (skipCount > 0L)
            {
                this.Skip(skipCount);
            }
            this.readBuffer = null;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("TarInputStream Write not supported");
        }

        public override void WriteByte(byte value)
        {
            throw new NotSupportedException("TarInputStream WriteByte not supported");
        }

        public bool IsStreamOwner
        {
            get => 
                this.tarBuffer.IsStreamOwner;
            set => 
                (this.tarBuffer.IsStreamOwner = value);
        }

        public override bool CanRead =>
            this.inputStream.CanRead;

        public override bool CanSeek =>
            false;

        public override bool CanWrite =>
            false;

        public override long Length =>
            this.inputStream.Length;

        public override long Position
        {
            get => 
                this.inputStream.Position;
            set
            {
                throw new NotSupportedException("TarInputStream Seek not supported");
            }
        }

        public int RecordSize =>
            this.tarBuffer.RecordSize;

        public long Available =>
            (this.entrySize - this.entryOffset);

        public bool IsMarkSupported =>
            false;

        public class EntryFactoryAdapter : TarInputStream.IEntryFactory
        {
            public TarEntry CreateEntry(string name) => 
                TarEntry.CreateTarEntry(name);

            public TarEntry CreateEntry(byte[] headerBuffer) => 
                new TarEntry(headerBuffer);

            public TarEntry CreateEntryFromFile(string fileName) => 
                TarEntry.CreateEntryFromFile(fileName);
        }

        public interface IEntryFactory
        {
            TarEntry CreateEntry(string name);
            TarEntry CreateEntry(byte[] headerBuffer);
            TarEntry CreateEntryFromFile(string fileName);
        }
    }
}

