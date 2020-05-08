namespace Mono.Cecil.Metadata
{
    using Mono.Cecil.PE;
    using System;

    internal sealed class DataBuffer : ByteBuffer
    {
        public DataBuffer() : base(0)
        {
        }

        public uint AddData(byte[] data)
        {
            uint position = (uint) base.position;
            base.WriteBytes(data);
            return position;
        }
    }
}

