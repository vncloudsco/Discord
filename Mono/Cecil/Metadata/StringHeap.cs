namespace Mono.Cecil.Metadata
{
    using Mono.Cecil.PE;
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class StringHeap : Heap
    {
        private readonly Dictionary<uint, string> strings;

        public StringHeap(Section section, uint start, uint size) : base(section, start, size)
        {
            this.strings = new Dictionary<uint, string>();
        }

        public string Read(uint index)
        {
            string str;
            if (index == 0)
            {
                return string.Empty;
            }
            if (!this.strings.TryGetValue(index, out str))
            {
                if (index > (base.Size - 1))
                {
                    return string.Empty;
                }
                str = this.ReadStringAt(index);
                if (str.Length != 0)
                {
                    this.strings.Add(index, str);
                }
            }
            return str;
        }

        protected virtual string ReadStringAt(uint index)
        {
            int count = 0;
            byte[] data = base.Section.Data;
            int num2 = (int) (index + base.Offset);
            for (int i = num2; data[i] != 0; i++)
            {
                count++;
            }
            return Encoding.UTF8.GetString(data, num2, count);
        }
    }
}

