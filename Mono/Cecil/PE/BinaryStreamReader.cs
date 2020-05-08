namespace Mono.Cecil.PE
{
    using System;
    using System.IO;

    internal class BinaryStreamReader : BinaryReader
    {
        public BinaryStreamReader(Stream stream) : base(stream)
        {
        }

        protected void Advance(int bytes)
        {
            this.BaseStream.Seek((long) bytes, SeekOrigin.Current);
        }

        protected DataDirectory ReadDataDirectory() => 
            new DataDirectory(this.ReadUInt32(), this.ReadUInt32());
    }
}

