namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class AssemblyTable : OneRowTable<Row<AssemblyHashAlgorithm, ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint>>
    {
        public override void Write(TableHeapBuffer buffer)
        {
            buffer.WriteUInt32(base.row.Col1);
            buffer.WriteUInt16(base.row.Col2);
            buffer.WriteUInt16(base.row.Col3);
            buffer.WriteUInt16(base.row.Col4);
            buffer.WriteUInt16(base.row.Col5);
            buffer.WriteUInt32(base.row.Col6);
            buffer.WriteBlob(base.row.Col7);
            buffer.WriteString(base.row.Col8);
            buffer.WriteString(base.row.Col9);
        }
    }
}

