﻿namespace ICSharpCode.SharpZipLib.Zip.Compression
{
    using System;

    internal class PendingBuffer
    {
        private byte[] buffer_;
        private int start;
        private int end;
        private uint bits;
        private int bitCount;

        public PendingBuffer() : this(0x1000)
        {
        }

        public PendingBuffer(int bufferSize)
        {
            this.buffer_ = new byte[bufferSize];
        }

        public void AlignToByte()
        {
            if (this.bitCount > 0)
            {
                int end = this.end;
                this.end = end + 1;
                this.buffer_[end] = (byte) this.bits;
                if (this.bitCount > 8)
                {
                    end = this.end;
                    this.end = end + 1;
                    this.buffer_[end] = (byte) (this.bits >> 8);
                }
            }
            this.bits = 0;
            this.bitCount = 0;
        }

        public int Flush(byte[] output, int offset, int length)
        {
            if (this.bitCount >= 8)
            {
                int end = this.end;
                this.end = end + 1;
                this.buffer_[end] = (byte) this.bits;
                this.bits = this.bits >> 8;
                this.bitCount -= 8;
            }
            if (length <= (this.end - this.start))
            {
                Array.Copy(this.buffer_, this.start, output, offset, length);
                this.start += length;
            }
            else
            {
                length = this.end - this.start;
                Array.Copy(this.buffer_, this.start, output, offset, length);
                this.start = 0;
                this.end = 0;
            }
            return length;
        }

        public void Reset()
        {
            int num1 = this.bitCount = 0;
            this.start = this.end = num1;
        }

        public byte[] ToByteArray()
        {
            byte[] destinationArray = new byte[this.end - this.start];
            Array.Copy(this.buffer_, this.start, destinationArray, 0, destinationArray.Length);
            this.start = 0;
            this.end = 0;
            return destinationArray;
        }

        public void WriteBits(int b, int count)
        {
            this.bits |= (uint) (b << (this.bitCount & 0x1f));
            this.bitCount += count;
            if (this.bitCount >= 0x10)
            {
                int end = this.end;
                this.end = end + 1;
                this.buffer_[end] = (byte) this.bits;
                end = this.end;
                this.end = end + 1;
                this.buffer_[end] = (byte) (this.bits >> 8);
                this.bits = this.bits >> 0x10;
                this.bitCount -= 0x10;
            }
        }

        public void WriteBlock(byte[] block, int offset, int length)
        {
            Array.Copy(block, offset, this.buffer_, this.end, length);
            this.end += length;
        }

        public void WriteByte(int value)
        {
            int end = this.end;
            this.end = end + 1;
            this.buffer_[end] = (byte) value;
        }

        public void WriteInt(int value)
        {
            int end = this.end;
            this.end = end + 1;
            this.buffer_[end] = (byte) value;
            end = this.end;
            this.end = end + 1;
            this.buffer_[end] = (byte) (value >> 8);
            end = this.end;
            this.end = end + 1;
            this.buffer_[end] = (byte) (value >> 0x10);
            end = this.end;
            this.end = end + 1;
            this.buffer_[end] = (byte) (value >> 0x18);
        }

        public void WriteShort(int value)
        {
            int end = this.end;
            this.end = end + 1;
            this.buffer_[end] = (byte) value;
            end = this.end;
            this.end = end + 1;
            this.buffer_[end] = (byte) (value >> 8);
        }

        public void WriteShortMSB(int s)
        {
            int end = this.end;
            this.end = end + 1;
            this.buffer_[end] = (byte) (s >> 8);
            end = this.end;
            this.end = end + 1;
            this.buffer_[end] = (byte) s;
        }

        public int BitCount =>
            this.bitCount;

        public bool IsFlushed =>
            (this.end == 0);
    }
}

