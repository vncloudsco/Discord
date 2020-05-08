namespace Mono.Cecil.Metadata
{
    using Mono.Cecil;
    using Mono.Cecil.PE;
    using System;

    internal sealed class UserStringHeap : StringHeap
    {
        public UserStringHeap(Section section, uint start, uint size) : base(section, start, size)
        {
        }

        protected override string ReadStringAt(uint index)
        {
            byte[] data = base.Section.Data;
            int position = (int) (index + base.Offset);
            uint num2 = data.ReadCompressedUInt32(ref position) & ((uint) (-2));
            if (num2 < 1)
            {
                return string.Empty;
            }
            char[] chArray = new char[num2 / 2];
            int num3 = position;
            int num4 = 0;
            while (num3 < (position + num2))
            {
                chArray[num4++] = (char) (data[num3] | (data[num3 + 1] << 8));
                num3 += 2;
            }
            return new string(chArray);
        }
    }
}

