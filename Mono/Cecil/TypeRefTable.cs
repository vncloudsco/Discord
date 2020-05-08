namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class TypeRefTable : MetadataTable<Row<uint, uint, uint>>
    {
        public override void Write(TableHeapBuffer buffer)
        {
            for (int i = 0; i < base.length; i++)
            {
                buffer.WriteCodedRID(base.rows[i].Col1, CodedIndex.ResolutionScope);
                buffer.WriteString(base.rows[i].Col2);
                buffer.WriteString(base.rows[i].Col3);
            }
        }
    }
}

