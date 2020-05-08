namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class EventMapTable : MetadataTable<Row<uint, uint>>
    {
        public override void Write(TableHeapBuffer buffer)
        {
            for (int i = 0; i < base.length; i++)
            {
                buffer.WriteRID(base.rows[i].Col1, Table.TypeDef);
                buffer.WriteRID(base.rows[i].Col2, Table.Event);
            }
        }
    }
}

