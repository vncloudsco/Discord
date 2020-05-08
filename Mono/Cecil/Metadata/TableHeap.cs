namespace Mono.Cecil.Metadata
{
    using Mono.Cecil.PE;
    using System;
    using System.Reflection;

    internal sealed class TableHeap : Heap
    {
        public const int TableCount = 0x2d;
        public long Valid;
        public long Sorted;
        public readonly TableInformation[] Tables;

        public TableHeap(Section section, uint start, uint size) : base(section, start, size)
        {
            this.Tables = new TableInformation[0x2d];
        }

        public bool HasTable(Table table) => 
            ((this.Valid & (1L << (table & 0x3f))) != 0L);

        public TableInformation this[Table table] =>
            this.Tables[(int) table];
    }
}

