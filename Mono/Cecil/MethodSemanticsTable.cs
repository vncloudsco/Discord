namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class MethodSemanticsTable : SortedTable<Row<MethodSemanticsAttributes, uint, uint>>
    {
        public override int Compare(Row<MethodSemanticsAttributes, uint, uint> x, Row<MethodSemanticsAttributes, uint, uint> y) => 
            base.Compare(x.Col3, y.Col3);

        public override void Write(TableHeapBuffer buffer)
        {
            for (int i = 0; i < base.length; i++)
            {
                buffer.WriteUInt16(base.rows[i].Col1);
                buffer.WriteRID(base.rows[i].Col2, Table.Method);
                buffer.WriteCodedRID(base.rows[i].Col3, CodedIndex.HasSemantics);
            }
        }
    }
}

