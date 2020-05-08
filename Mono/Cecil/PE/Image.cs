namespace Mono.Cecil.PE
{
    using Mono;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Metadata;
    using System;
    using System.Runtime.InteropServices;

    internal sealed class Image
    {
        public ModuleKind Kind;
        public string RuntimeVersion;
        public TargetArchitecture Architecture;
        public ModuleCharacteristics Characteristics;
        public string FileName;
        public Section[] Sections;
        public Section MetadataSection;
        public uint EntryPointToken;
        public ModuleAttributes Attributes;
        public DataDirectory Debug;
        public DataDirectory Resources;
        public DataDirectory StrongName;
        public Mono.Cecil.Metadata.StringHeap StringHeap;
        public Mono.Cecil.Metadata.BlobHeap BlobHeap;
        public Mono.Cecil.Metadata.UserStringHeap UserStringHeap;
        public Mono.Cecil.Metadata.GuidHeap GuidHeap;
        public Mono.Cecil.Metadata.TableHeap TableHeap;
        private readonly int[] coded_index_sizes = new int[13];
        private readonly Func<Table, int> counter;

        public Image()
        {
            this.counter = new Func<Table, int>(this.GetTableLength);
        }

        public int GetCodedIndexSize(CodedIndex coded_index)
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

        public ImageDebugDirectory GetDebugHeader(out byte[] header)
        {
            Section sectionAtVirtualAddress = this.GetSectionAtVirtualAddress(this.Debug.VirtualAddress);
            ByteBuffer buffer = new ByteBuffer(sectionAtVirtualAddress.Data) {
                position = (int) (this.Debug.VirtualAddress - sectionAtVirtualAddress.VirtualAddress)
            };
            ImageDebugDirectory directory = new ImageDebugDirectory {
                Characteristics = buffer.ReadInt32(),
                TimeDateStamp = buffer.ReadInt32(),
                MajorVersion = buffer.ReadInt16(),
                MinorVersion = buffer.ReadInt16(),
                Type = buffer.ReadInt32(),
                SizeOfData = buffer.ReadInt32(),
                AddressOfRawData = buffer.ReadInt32(),
                PointerToRawData = buffer.ReadInt32()
            };
            if ((directory.SizeOfData == 0) || (directory.PointerToRawData == 0))
            {
                header = Empty<byte>.Array;
                return directory;
            }
            buffer.position = directory.PointerToRawData - ((int) sectionAtVirtualAddress.PointerToRawData);
            header = new byte[directory.SizeOfData];
            Buffer.BlockCopy(buffer.buffer, buffer.position, header, 0, header.Length);
            return directory;
        }

        public Section GetSection(string name)
        {
            foreach (Section section in this.Sections)
            {
                if (section.Name == name)
                {
                    return section;
                }
            }
            return null;
        }

        public Section GetSectionAtVirtualAddress(uint rva)
        {
            foreach (Section section in this.Sections)
            {
                if ((rva >= section.VirtualAddress) && (rva < (section.VirtualAddress + section.SizeOfRawData)))
                {
                    return section;
                }
            }
            return null;
        }

        public int GetTableIndexSize(Table table) => 
            ((this.GetTableLength(table) < 0x10000) ? 2 : 4);

        public int GetTableLength(Table table) => 
            ((int) this.TableHeap[table].Length);

        public bool HasTable(Table table) => 
            (this.GetTableLength(table) > 0);

        public uint ResolveVirtualAddress(uint rva)
        {
            Section sectionAtVirtualAddress = this.GetSectionAtVirtualAddress(rva);
            if (sectionAtVirtualAddress == null)
            {
                throw new ArgumentOutOfRangeException();
            }
            return this.ResolveVirtualAddressInSection(rva, sectionAtVirtualAddress);
        }

        public uint ResolveVirtualAddressInSection(uint rva, Section section) => 
            ((rva + section.PointerToRawData) - section.VirtualAddress);
    }
}

