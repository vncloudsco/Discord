namespace Mono.Cecil
{
    using System;

    internal abstract class MetadataTable<TRow> : MetadataTable where TRow: struct
    {
        internal TRow[] rows;
        internal int length;

        protected MetadataTable()
        {
            this.rows = new TRow[2];
        }

        public int AddRow(TRow row)
        {
            int num;
            if (this.rows.Length == this.length)
            {
                this.Grow();
            }
            this.length = (num = this.length) + 1;
            this.rows[num] = row;
            return this.length;
        }

        private void Grow()
        {
            TRow[] destinationArray = new TRow[this.rows.Length * 2];
            Array.Copy(this.rows, destinationArray, this.rows.Length);
            this.rows = destinationArray;
        }

        public override void Sort()
        {
        }

        public sealed override int Length =>
            this.length;
    }
}

