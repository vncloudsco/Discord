namespace Mono.Cecil.PE
{
    using Mono;
    using System;

    internal class ByteBuffer
    {
        internal byte[] buffer;
        internal int length;
        internal int position;

        public ByteBuffer()
        {
            this.buffer = Empty<byte>.Array;
        }

        public ByteBuffer(int length)
        {
            this.buffer = new byte[length];
        }

        public ByteBuffer(byte[] buffer)
        {
            this.buffer = buffer ?? Empty<byte>.Array;
            this.length = this.buffer.Length;
        }

        public void Advance(int length)
        {
            this.position += length;
        }

        private void Grow(int desired)
        {
            byte[] src = this.buffer;
            int length = src.Length;
            byte[] dst = new byte[Math.Max((int) (length + desired), (int) (length * 2))];
            Buffer.BlockCopy(src, 0, dst, 0, length);
            this.buffer = dst;
        }

        public byte ReadByte()
        {
            int num;
            this.position = (num = this.position) + 1;
            return this.buffer[num];
        }

        public byte[] ReadBytes(int length)
        {
            byte[] dst = new byte[length];
            Buffer.BlockCopy(this.buffer, this.position, dst, 0, length);
            this.position += length;
            return dst;
        }

        public int ReadCompressedInt32()
        {
            int num = (int) (this.ReadCompressedUInt32() >> 1);
            return (((num & 1) != 0) ? ((num >= 0x40) ? ((num >= 0x2000) ? ((num >= 0x10000000) ? (num - 0x20000000) : (num - 0x10000000)) : (num - 0x2000)) : (num - 0x40)) : num);
        }

        public uint ReadCompressedUInt32()
        {
            byte num = this.ReadByte();
            return (((num & 0x80) != 0) ? (((num & 0x40) != 0) ? ((uint) (((((num & -193) << 0x18) | (this.ReadByte() << 0x10)) | (this.ReadByte() << 8)) | this.ReadByte())) : ((uint) (((num & -129) << 8) | this.ReadByte()))) : ((uint) num));
        }

        public double ReadDouble()
        {
            if (!BitConverter.IsLittleEndian)
            {
                byte[] array = this.ReadBytes(8);
                Array.Reverse(array);
                return BitConverter.ToDouble(array, 0);
            }
            double num = BitConverter.ToDouble(this.buffer, this.position);
            this.position += 8;
            return num;
        }

        public short ReadInt16() => 
            ((short) this.ReadUInt16());

        public int ReadInt32() => 
            ((int) this.ReadUInt32());

        public long ReadInt64() => 
            ((long) this.ReadUInt64());

        public sbyte ReadSByte() => 
            ((sbyte) this.ReadByte());

        public float ReadSingle()
        {
            if (!BitConverter.IsLittleEndian)
            {
                byte[] array = this.ReadBytes(4);
                Array.Reverse(array);
                return BitConverter.ToSingle(array, 0);
            }
            float num = BitConverter.ToSingle(this.buffer, this.position);
            this.position += 4;
            return num;
        }

        public ushort ReadUInt16()
        {
            ushort num = (ushort) (this.buffer[this.position] | (this.buffer[this.position + 1] << 8));
            this.position += 2;
            return num;
        }

        public uint ReadUInt32()
        {
            uint num = (uint) (((this.buffer[this.position] | (this.buffer[this.position + 1] << 8)) | (this.buffer[this.position + 2] << 0x10)) | (this.buffer[this.position + 3] << 0x18));
            this.position += 4;
            return num;
        }

        public ulong ReadUInt64()
        {
            uint num = this.ReadUInt32();
            return ((this.ReadUInt32() << 0x20) | num);
        }

        public void Reset(byte[] buffer)
        {
            this.buffer = buffer ?? Empty<byte>.Array;
            this.length = this.buffer.Length;
        }

        public void WriteByte(byte value)
        {
            int num;
            if (this.position == this.buffer.Length)
            {
                this.Grow(1);
            }
            this.position = (num = this.position) + 1;
            this.buffer[num] = value;
            if (this.position > this.length)
            {
                this.length = this.position;
            }
        }

        public void WriteBytes(byte[] bytes)
        {
            int length = bytes.Length;
            if ((this.position + length) > this.buffer.Length)
            {
                this.Grow(length);
            }
            Buffer.BlockCopy(bytes, 0, this.buffer, this.position, length);
            this.position += length;
            if (this.position > this.length)
            {
                this.length = this.position;
            }
        }

        public void WriteBytes(ByteBuffer buffer)
        {
            if ((this.position + buffer.length) > this.buffer.Length)
            {
                this.Grow(buffer.length);
            }
            Buffer.BlockCopy(buffer.buffer, 0, this.buffer, this.position, buffer.length);
            this.position += buffer.length;
            if (this.position > this.length)
            {
                this.length = this.position;
            }
        }

        public void WriteBytes(int length)
        {
            if ((this.position + length) > this.buffer.Length)
            {
                this.Grow(length);
            }
            this.position += length;
            if (this.position > this.length)
            {
                this.length = this.position;
            }
        }

        public void WriteCompressedInt32(int value)
        {
            if (value >= 0)
            {
                this.WriteCompressedUInt32((uint) (value << 1));
            }
            else
            {
                if (value > -64)
                {
                    value = 0x40 + value;
                }
                else if (value >= -8192)
                {
                    value = 0x2000 + value;
                }
                else if (value >= -536870912)
                {
                    value = 0x20000000 + value;
                }
                this.WriteCompressedUInt32((uint) ((value << 1) | 1));
            }
        }

        public void WriteCompressedUInt32(uint value)
        {
            if (value < 0x80)
            {
                this.WriteByte((byte) value);
            }
            else if (value < 0x4000)
            {
                this.WriteByte((byte) (0x80 | (value >> 8)));
                this.WriteByte((byte) (value & 0xff));
            }
            else
            {
                this.WriteByte((byte) ((value >> 0x18) | 0xc0));
                this.WriteByte((byte) ((value >> 0x10) & 0xff));
                this.WriteByte((byte) ((value >> 8) & 0xff));
                this.WriteByte((byte) (value & 0xff));
            }
        }

        public void WriteDouble(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            this.WriteBytes(bytes);
        }

        public void WriteInt16(short value)
        {
            this.WriteUInt16((ushort) value);
        }

        public void WriteInt32(int value)
        {
            this.WriteUInt32((uint) value);
        }

        public void WriteInt64(long value)
        {
            this.WriteUInt64((ulong) value);
        }

        public void WriteSByte(sbyte value)
        {
            this.WriteByte((byte) value);
        }

        public void WriteSingle(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            this.WriteBytes(bytes);
        }

        public void WriteUInt16(ushort value)
        {
            int num;
            int num2;
            if ((this.position + 2) > this.buffer.Length)
            {
                this.Grow(2);
            }
            this.position = (num = this.position) + 1;
            this.buffer[num] = (byte) value;
            this.position = (num2 = this.position) + 1;
            this.buffer[num2] = (byte) (value >> 8);
            if (this.position > this.length)
            {
                this.length = this.position;
            }
        }

        public void WriteUInt32(uint value)
        {
            int num;
            int num2;
            int num3;
            int num4;
            if ((this.position + 4) > this.buffer.Length)
            {
                this.Grow(4);
            }
            this.position = (num = this.position) + 1;
            this.buffer[num] = (byte) value;
            this.position = (num2 = this.position) + 1;
            this.buffer[num2] = (byte) (value >> 8);
            this.position = (num3 = this.position) + 1;
            this.buffer[num3] = (byte) (value >> 0x10);
            this.position = (num4 = this.position) + 1;
            this.buffer[num4] = (byte) (value >> 0x18);
            if (this.position > this.length)
            {
                this.length = this.position;
            }
        }

        public void WriteUInt64(ulong value)
        {
            int num;
            int num2;
            int num3;
            int num4;
            int num5;
            int num6;
            int num7;
            int num8;
            if ((this.position + 8) > this.buffer.Length)
            {
                this.Grow(8);
            }
            this.position = (num = this.position) + 1;
            this.buffer[num] = (byte) value;
            this.position = (num2 = this.position) + 1;
            this.buffer[num2] = (byte) (value >> 8);
            this.position = (num3 = this.position) + 1;
            this.buffer[num3] = (byte) (value >> 0x10);
            this.position = (num4 = this.position) + 1;
            this.buffer[num4] = (byte) (value >> 0x18);
            this.position = (num5 = this.position) + 1;
            this.buffer[num5] = (byte) (value >> 0x20);
            this.position = (num6 = this.position) + 1;
            this.buffer[num6] = (byte) (value >> 40);
            this.position = (num7 = this.position) + 1;
            this.buffer[num7] = (byte) (value >> 0x30);
            this.position = (num8 = this.position) + 1;
            this.buffer[num8] = (byte) (value >> 0x38);
            if (this.position > this.length)
            {
                this.length = this.position;
            }
        }
    }
}

