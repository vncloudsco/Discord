namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class EventTable : MetadataTable<Row<EventAttributes, uint, uint>>
    {
        public override void Write(TableHeapBuffer buffer)
        {
            for (int i = 0; i < base.length; i++)
            {
                buffer.WriteUInt16(base.rows[i].Col1);
                buffer.WriteString(base.rows[i].Col2);
                buffer.WriteCodedRID(base.rows[i].Col3, CodedIndex.TypeDefOrRef);
            }
        }
    }
}

