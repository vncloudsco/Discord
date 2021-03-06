﻿namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class CustomAttributeTable : SortedTable<Row<uint, uint, uint>>
    {
        public override int Compare(Row<uint, uint, uint> x, Row<uint, uint, uint> y) => 
            base.Compare(x.Col1, y.Col1);

        public override void Write(TableHeapBuffer buffer)
        {
            for (int i = 0; i < base.length; i++)
            {
                buffer.WriteCodedRID(base.rows[i].Col1, CodedIndex.HasCustomAttribute);
                buffer.WriteCodedRID(base.rows[i].Col2, CodedIndex.CustomAttributeType);
                buffer.WriteBlob(base.rows[i].Col3);
            }
        }
    }
}

