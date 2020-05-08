namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class ExportedTypeTable : MetadataTable<Row<TypeAttributes, uint, uint, uint, uint>>
    {
        public override void Write(TableHeapBuffer buffer)
        {
            for (int i = 0; i < base.length; i++)
            {
                buffer.WriteUInt32(base.rows[i].Col1);
                buffer.WriteUInt32(base.rows[i].Col2);
                buffer.WriteString(base.rows[i].Col3);
                buffer.WriteString(base.rows[i].Col4);
                buffer.WriteCodedRID(base.rows[i].Col5, CodedIndex.Implementation);
            }
        }
    }
}

