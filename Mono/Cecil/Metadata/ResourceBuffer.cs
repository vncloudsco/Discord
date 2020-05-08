namespace Mono.Cecil.Metadata
{
    using Mono.Cecil.PE;
    using System;

    internal sealed class ResourceBuffer : ByteBuffer
    {
        public ResourceBuffer() : base(0)
        {
        }

        public uint AddResource(byte[] resource)
        {
            uint position = (uint) base.position;
            base.WriteInt32(resource.Length);
            base.WriteBytes(resource);
            return position;
        }
    }
}

