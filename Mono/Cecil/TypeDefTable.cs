namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class TypeDefTable : MetadataTable<Row<TypeAttributes, uint, uint, uint, uint, uint>>
    {
        public override void Write(TableHeapBuffer buffer)
        {
            for (int i = 0; i < base.length; i++)
            {
                buffer.WriteUInt32(base.rows[i].Col1);
                buffer.WriteString(base.rows[i].Col2);
                buffer.WriteString(base.rows[i].Col3);
                buffer.WriteCodedRID(base.rows[i].Col4, CodedIndex.TypeDefOrRef);
                buffer.WriteRID(base.rows[i].Col5, Table.Field);
                buffer.WriteRID(base.rows[i].Col6, Table.Method);
            }
        }
    }
}

