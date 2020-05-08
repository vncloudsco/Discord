namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class GenericParamTable : MetadataTable<Row<ushort, GenericParameterAttributes, uint, uint>>
    {
        public override void Write(TableHeapBuffer buffer)
        {
            for (int i = 0; i < base.length; i++)
            {
                buffer.WriteUInt16(base.rows[i].Col1);
                buffer.WriteUInt16(base.rows[i].Col2);
                buffer.WriteCodedRID(base.rows[i].Col3, CodedIndex.TypeOrMethodDef);
                buffer.WriteString(base.rows[i].Col4);
            }
        }
    }
}

