namespace Mono.Cecil.Metadata
{
    using Mono;
    using Mono.Cecil;
    using Mono.Cecil.PE;
    using System;

    internal sealed class BlobHeap : Heap
    {
        public BlobHeap(Section section, uint start, uint size) : base(section, start, size)
        {
        }

        public byte[] Read(uint index)
        {
            if ((index == 0) || (index > (base.Size - 1)))
            {
                return Empty<byte>.Array;
            }
            byte[] data = base.Section.Data;
            int position = (int) (index + base.Offset);
            int count = (int) data.ReadCompressedUInt32(ref position);
            byte[] dst = new byte[count];
            Buffer.BlockCopy(data, position, dst, 0, count);
            return dst;
        }
    }
}

