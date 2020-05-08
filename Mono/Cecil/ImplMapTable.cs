namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class ImplMapTable : SortedTable<Row<PInvokeAttributes, uint, uint, uint>>
    {
        public override int Compare(Row<PInvokeAttributes, uint, uint, uint> x, Row<PInvokeAttributes, uint, uint, uint> y) => 
            base.Compare(x.Col2, y.Col2);

        public override void Write(TableHeapBuffer buffer)
        {
            for (int i = 0; i < base.length; i++)
            {
                buffer.WriteUInt16(base.rows[i].Col1);
                buffer.WriteCodedRID(base.rows[i].Col2, CodedIndex.MemberForwarded);
                buffer.WriteString(base.rows[i].Col3);
                buffer.WriteRID(base.rows[i].Col4, Table.ModuleRef);
            }
        }
    }
}

