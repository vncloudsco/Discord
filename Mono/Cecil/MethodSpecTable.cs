namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class MethodSpecTable : MetadataTable<Row<uint, uint>>
    {
        public override void Write(TableHeapBuffer buffer)
        {
            for (int i = 0; i < base.length; i++)
            {
                buffer.WriteCodedRID(base.rows[i].Col1, CodedIndex.MethodDefOrRef);
                buffer.WriteBlob(base.rows[i].Col2);
            }
        }
    }
}

