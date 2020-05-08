namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class AssemblyRefTable : MetadataTable<Row<ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint, uint>>
    {
        public override void Write(TableHeapBuffer buffer)
        {
            for (int i = 0; i < base.length; i++)
            {
                buffer.WriteUInt16(base.rows[i].Col1);
                buffer.WriteUInt16(base.rows[i].Col2);
                buffer.WriteUInt16(base.rows[i].Col3);
                buffer.WriteUInt16(base.rows[i].Col4);
                buffer.WriteUInt32(base.rows[i].Col5);
                buffer.WriteBlob(base.rows[i].Col6);
                buffer.WriteString(base.rows[i].Col7);
                buffer.WriteString(base.rows[i].Col8);
                buffer.WriteBlob(base.rows[i].Col9);
            }
        }
    }
}

