namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class ClassLayoutTable : SortedTable<Row<ushort, uint, uint>>
    {
        public override int Compare(Row<ushort, uint, uint> x, Row<ushort, uint, uint> y) => 
            base.Compare(x.Col3, y.Col3);

        public override void Write(TableHeapBuffer buffer)
        {
            for (int i = 0; i < base.length; i++)
            {
                buffer.WriteUInt16(base.rows[i].Col1);
                buffer.WriteUInt32(base.rows[i].Col2);
                buffer.WriteRID(base.rows[i].Col3, Table.TypeDef);
            }
        }
    }
}

