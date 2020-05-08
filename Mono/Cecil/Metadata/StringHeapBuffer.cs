namespace Mono.Cecil.Metadata
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class StringHeapBuffer : HeapBuffer
    {
        private readonly Dictionary<string, uint> strings;

        public StringHeapBuffer() : base(1)
        {
            this.strings = new Dictionary<string, uint>(StringComparer.Ordinal);
            base.WriteByte(0);
        }

        public uint GetStringIndex(string @string)
        {
            uint position;
            if (!this.strings.TryGetValue(@string, out position))
            {
                position = (uint) base.position;
                this.WriteString(@string);
                this.strings.Add(@string, position);
            }
            return position;
        }

        protected virtual void WriteString(string @string)
        {
            base.WriteBytes(Encoding.UTF8.GetBytes(@string));
            base.WriteByte(0);
        }

        public sealed override bool IsEmpty =>
            (base.length <= 1);
    }
}

