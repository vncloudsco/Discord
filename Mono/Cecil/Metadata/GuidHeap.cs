namespace Mono.Cecil.Metadata
{
    using Mono.Cecil.PE;
    using System;

    internal sealed class GuidHeap : Heap
    {
        public GuidHeap(Section section, uint start, uint size) : base(section, start, size)
        {
        }

        public Guid Read(uint index)
        {
            if (index == 0)
            {
                return default(Guid);
            }
            byte[] dst = new byte[0x10];
            index--;
            Buffer.BlockCopy(base.Section.Data, (int) (base.Offset + index), dst, 0, 0x10);
            return new Guid(dst);
        }
    }
}

