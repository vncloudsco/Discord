﻿namespace Mono.Cecil.PE
{
    using System;
    using System.IO;

    internal class BinaryStreamWriter : BinaryWriter
    {
        public BinaryStreamWriter(Stream stream) : base(stream)
        {
        }

        protected void Advance(int bytes)
        {
            this.BaseStream.Seek((long) bytes, SeekOrigin.Current);
        }

        public void WriteBuffer(ByteBuffer buffer)
        {
            this.Write(buffer.buffer, 0, buffer.length);
        }

        public void WriteByte(byte value)
        {
            this.Write(value);
        }

        public void WriteBytes(byte[] bytes)
        {
            this.Write(bytes);
        }

        public void WriteDataDirectory(DataDirectory directory)
        {
            this.Write(directory.VirtualAddress);
            this.Write(directory.Size);
        }

        public void WriteInt16(short value)
        {
            this.Write(value);
        }

        public void WriteInt32(int value)
        {
            this.Write(value);
        }

        public void WriteUInt16(ushort value)
        {
            this.Write(value);
        }

        public void WriteUInt32(uint value)
        {
            this.Write(value);
        }

        public void WriteUInt64(ulong value)
        {
            this.Write(value);
        }
    }
}

