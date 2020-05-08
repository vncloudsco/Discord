namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class ModuleTable : OneRowTable<uint>
    {
        public override void Write(TableHeapBuffer buffer)
        {
            buffer.WriteUInt16(0);
            buffer.WriteString(base.row);
            buffer.WriteUInt16(1);
            buffer.WriteUInt16(0);
            buffer.WriteUInt16(0);
        }
    }
}

