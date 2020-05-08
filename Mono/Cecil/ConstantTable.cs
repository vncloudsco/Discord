namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class ConstantTable : SortedTable<Row<ElementType, uint, uint>>
    {
        public override int Compare(Row<ElementType, uint, uint> x, Row<ElementType, uint, uint> y) => 
            base.Compare(x.Col2, y.Col2);

        public override void Write(TableHeapBuffer buffer)
        {
            for (int i = 0; i < base.length; i++)
            {
                buffer.WriteUInt16(base.rows[i].Col1);
                buffer.WriteCodedRID(base.rows[i].Col2, CodedIndex.HasConstant);
                buffer.WriteBlob(base.rows[i].Col3);
            }
        }
    }
}

