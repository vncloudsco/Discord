namespace Mono.Cecil
{
    using System;

    internal abstract class OneRowTable<TRow> : MetadataTable where TRow: struct
    {
        internal TRow row;

        protected OneRowTable()
        {
        }

        public sealed override void Sort()
        {
        }

        public sealed override int Length =>
            1;
    }
}

