namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class MethodTable : MetadataTable<Row<uint, MethodImplAttributes, MethodAttributes, uint, uint, uint>>
    {
        public override void Write(TableHeapBuffer buffer)
        {
            for (int i = 0; i < base.length; i++)
            {
                buffer.WriteUInt32(base.rows[i].Col1);
                buffer.WriteUInt16(base.rows[i].Col2);
                buffer.WriteUInt16(base.rows[i].Col3);
                buffer.WriteString(base.rows[i].Col4);
                buffer.WriteBlob(base.rows[i].Col5);
                buffer.WriteRID(base.rows[i].Col6, Table.Param);
            }
        }
    }
}

