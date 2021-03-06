﻿namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class InterfaceImplTable : MetadataTable<Row<uint, uint>>
    {
        public override void Write(TableHeapBuffer buffer)
        {
            for (int i = 0; i < base.length; i++)
            {
                buffer.WriteRID(base.rows[i].Col1, Table.TypeDef);
                buffer.WriteCodedRID(base.rows[i].Col2, CodedIndex.TypeDefOrRef);
            }
        }
    }
}

