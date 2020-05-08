namespace Mono.Cecil
{
    using System;
    using System.Collections.Generic;

    internal abstract class SortedTable<TRow> : MetadataTable<TRow>, IComparer<TRow> where TRow: struct
    {
        protected SortedTable()
        {
        }

        protected int Compare(uint x, uint y) => 
            ((x == y) ? 0 : ((x > y) ? 1 : -1));

        public abstract int Compare(TRow x, TRow y);
        public sealed override void Sort()
        {
            Array.Sort<TRow>(base.rows, 0, base.length, this);
        }
    }
}

