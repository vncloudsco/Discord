﻿namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class FileTable : MetadataTable<Row<FileAttributes, uint, uint>>
    {
        public override void Write(TableHeapBuffer buffer)
        {
            for (int i = 0; i < base.length; i++)
            {
                buffer.WriteUInt32(base.rows[i].Col1);
                buffer.WriteString(base.rows[i].Col2);
                buffer.WriteBlob(base.rows[i].Col3);
            }
        }
    }
}

