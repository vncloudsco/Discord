namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class ParamTable : MetadataTable<Row<ParameterAttributes, ushort, uint>>
    {
        public override void Write(TableHeapBuffer buffer)
        {
            for (int i = 0; i < base.length; i++)
            {
                buffer.WriteUInt16(base.rows[i].Col1);
                buffer.WriteUInt16(base.rows[i].Col2);
                buffer.WriteString(base.rows[i].Col3);
            }
        }
    }
}

