namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class ModuleRefTable : MetadataTable<uint>
    {
        public override void Write(TableHeapBuffer buffer)
        {
            for (int i = 0; i < base.length; i++)
            {
                buffer.WriteString(base.rows[i]);
            }
        }
    }
}

