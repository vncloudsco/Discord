namespace Mono.Cecil.Metadata
{
    using Mono.Cecil;
    using System;

    internal sealed class TableHeapBuffer : HeapBuffer
    {
        private readonly ModuleDefinition module;
        private readonly MetadataBuilder metadata;
        internal MetadataTable[] tables;
        private bool large_string;
        private bool large_blob;
        private readonly int[] coded_index_sizes;
        private readonly Func<Table, int> counter;

        public TableHeapBuffer(ModuleDefinition module, MetadataBuilder metadata) : base(0x18)
        {
            this.tables = new MetadataTable[0x2d];
            this.coded_index_sizes = new int[13];
            this.module = module;
            this.metadata = metadata;
            this.counter = new Func<Table, int>(this.GetTableLength);
        }

        public void FixupData(uint data_rva)
        {
            FieldRVATable table = this.GetTable<FieldRVATable>(Table.FieldRVA);
            if (table.length != 0)
            {
                int num = this.GetTable<FieldTable>(Table.Field).IsLarge ? 4 : 2;
                int position = base.position;
                base.position = table.position;
                for (int i = 0; i < table.length; i++)
                {
                    uint num4 = base.ReadUInt32();
                    base.position -= 4;
                    base.WriteUInt32(num4 + data_rva);
                    base.position += num;
                }
                base.position = position;
            }
        }

        private int GetCodedIndexSize(CodedIndex coded_index)
        {
            int num3;
            int index = (int) coded_index;
            int num2 = this.coded_index_sizes[index];
            if (num2 != 0)
            {
                return num2;
            }
            this.coded_index_sizes[index] = num3 = coded_index.GetSize(this.counter);
            return num3;
        }

        private byte GetHeapSizes()
        {
            byte num = 0;
            if (this.metadata.string_heap.IsLarge)
            {
                this.large_string = true;
                num = (byte) (num | 1);
            }
            if (this.metadata.blob_heap.IsLarge)
            {
                this.large_blob = true;
                num = (byte) (num | 4);
            }
            return num;
        }

        public TTable GetTable<TTable>(Table table) where TTable: MetadataTable, new()
        {
            TTable local = (TTable) this.tables[(int) table];
            if (local == null)
            {
                local = Activator.CreateInstance<TTable>();
                this.tables[(int) table] = local;
            }
            return local;
        }

        private byte GetTableHeapVersion()
        {
            switch (this.module.Runtime)
            {
                case TargetRuntime.Net_1_0:
                case TargetRuntime.Net_1_1:
                    return 1;
            }
            return 2;
        }

        private int GetTableLength(Table table)
        {
            MetadataTable table2 = this.tables[(int) table];
            return ((table2 != null) ? table2.Length : 0);
        }

        private ulong GetValid()
        {
            ulong num = 0UL;
            for (int i = 0; i < this.tables.Length; i++)
            {
                MetadataTable table = this.tables[i];
                if ((table != null) && (table.Length != 0))
                {
                    table.Sort();
                    num |= (ulong) (1L << (i & 0x3f));
                }
            }
            return num;
        }

        public void WriteBlob(uint blob)
        {
            this.WriteBySize(blob, this.large_blob);
        }

        public void WriteBySize(uint value, bool large)
        {
            if (large)
            {
                base.WriteUInt32(value);
            }
            else
            {
                base.WriteUInt16((ushort) value);
            }
        }

        public void WriteBySize(uint value, int size)
        {
            if (size == 4)
            {
                base.WriteUInt32(value);
            }
            else
            {
                base.WriteUInt16((ushort) value);
            }
        }

        public void WriteCodedRID(uint rid, CodedIndex coded_index)
        {
            this.WriteBySize(rid, this.GetCodedIndexSize(coded_index));
        }

        public void WriteRID(uint rid, Table table)
        {
            MetadataTable table2 = this.tables[(int) table];
            this.WriteBySize(rid, (table2 != null) && table2.IsLarge);
        }

        private void WriteRowCount()
        {
            for (int i = 0; i < this.tables.Length; i++)
            {
                MetadataTable table = this.tables[i];
                if ((table != null) && (table.Length != 0))
                {
                    base.WriteUInt32((uint) table.Length);
                }
            }
        }

        public void WriteString(uint @string)
        {
            this.WriteBySize(@string, this.large_string);
        }

        public void WriteTableHeap()
        {
            base.WriteUInt32(0);
            base.WriteByte(this.GetTableHeapVersion());
            base.WriteByte(0);
            base.WriteByte(this.GetHeapSizes());
            base.WriteByte(10);
            base.WriteUInt64(this.GetValid());
            base.WriteUInt64(0x16003301fa00UL);
            this.WriteRowCount();
            this.WriteTables();
        }

        private void WriteTables()
        {
            for (int i = 0; i < this.tables.Length; i++)
            {
                MetadataTable table = this.tables[i];
                if ((table != null) && (table.Length != 0))
                {
                    table.Write(this);
                }
            }
        }

        public override bool IsEmpty =>
            false;
    }
}

