namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal abstract class MetadataTable
    {
        protected MetadataTable()
        {
        }

        public abstract void Sort();
        public abstract void Write(TableHeapBuffer buffer);

        public abstract int Length { get; }

        public bool IsLarge =>
            (this.Length > 0xffff);
    }
}

