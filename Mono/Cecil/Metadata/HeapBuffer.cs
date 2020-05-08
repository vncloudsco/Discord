namespace Mono.Cecil.Metadata
{
    using Mono.Cecil.PE;
    using System;

    internal abstract class HeapBuffer : ByteBuffer
    {
        protected HeapBuffer(int length) : base(length)
        {
        }

        public bool IsLarge =>
            (base.length > 0xffff);

        public abstract bool IsEmpty { get; }
    }
}

