namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class ManifestResourceTable : MetadataTable<Row<uint, ManifestResourceAttributes, uint, uint>>
    {
        public override void Write(TableHeapBuffer buffer)
        {
            for (int i = 0; i < base.length; i++)
            {
                buffer.WriteUInt32(base.rows[i].Col1);
                buffer.WriteUInt32(base.rows[i].Col2);
                buffer.WriteString(base.rows[i].Col3);
                buffer.WriteCodedRID(base.rows[i].Col4, CodedIndex.Implementation);
            }
        }
    }
}

