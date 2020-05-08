namespace Mono.Cecil.PE
{
    using Mono;
    using Mono.Cecil;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Metadata;
    using System;
    using System.IO;

    internal sealed class ImageWriter : BinaryStreamWriter
    {
        private const uint pe_header_size = 0x98;
        private const uint section_header_size = 40;
        private const uint file_alignment = 0x200;
        private const uint section_alignment = 0x2000;
        private const ulong image_base = 0x400000UL;
        internal const uint text_rva = 0x2000;
        private readonly ModuleDefinition module;
        private readonly MetadataBuilder metadata;
        private readonly TextMap text_map;
        private ImageDebugDirectory debug_directory;
        private byte[] debug_data;
        private ByteBuffer win32_resources;
        private readonly bool pe64;
        private readonly bool has_reloc;
        private readonly uint time_stamp;
        internal Section text;
        internal Section rsrc;
        internal Section reloc;
        private ushort sections;

        private ImageWriter(ModuleDefinition module, MetadataBuilder metadata, Stream stream) : base(stream)
        {
            this.module = module;
            this.metadata = metadata;
            this.pe64 = (module.Architecture == TargetArchitecture.AMD64) || (module.Architecture == TargetArchitecture.IA64);
            this.has_reloc = module.Architecture == TargetArchitecture.I386;
            this.GetDebugHeader();
            this.GetWin32Resources();
            this.text_map = this.BuildTextMap();
            this.sections = this.has_reloc ? ((ushort) 2) : ((ushort) 1);
            this.time_stamp = (uint) DateTime.UtcNow.Subtract(new DateTime(0x7b2, 1, 1)).TotalSeconds;
        }

        private static uint Align(uint value, uint align)
        {
            align--;
            return ((value + align) & ~align);
        }

        private void BuildSections()
        {
            bool flag = !ReferenceEquals(this.win32_resources, null);
            if (flag)
            {
                this.sections = (ushort) (this.sections + 1);
            }
            this.text = this.CreateSection(".text", this.text_map.GetLength(), null);
            Section text = this.text;
            if (flag)
            {
                this.rsrc = this.CreateSection(".rsrc", (uint) this.win32_resources.length, text);
                this.PatchWin32Resources(this.win32_resources);
                text = this.rsrc;
            }
            if (this.has_reloc)
            {
                this.reloc = this.CreateSection(".reloc", 12, text);
            }
        }

        private TextMap BuildTextMap()
        {
            TextMap map = this.metadata.text_map;
            map.AddMap(TextSegment.Code, this.metadata.code.length, !this.pe64 ? 4 : 0x10);
            map.AddMap(TextSegment.Resources, this.metadata.resources.length, 8);
            map.AddMap(TextSegment.Data, this.metadata.data.length, 4);
            if (this.metadata.data.length > 0)
            {
                this.metadata.table_heap.FixupData(map.GetRVA(TextSegment.Data));
            }
            map.AddMap(TextSegment.StrongNameSignature, this.GetStrongNameLength(), 4);
            map.AddMap(TextSegment.MetadataHeader, this.GetMetadataHeaderLength());
            map.AddMap(TextSegment.TableHeap, this.metadata.table_heap.length, 4);
            map.AddMap(TextSegment.StringHeap, this.metadata.string_heap.length, 4);
            map.AddMap(TextSegment.UserStringHeap, this.metadata.user_string_heap.IsEmpty ? 0 : this.metadata.user_string_heap.length, 4);
            map.AddMap(TextSegment.GuidHeap, 0x10);
            map.AddMap(TextSegment.BlobHeap, this.metadata.blob_heap.IsEmpty ? 0 : this.metadata.blob_heap.length, 4);
            int length = 0;
            if (!this.debug_data.IsNullOrEmpty<byte>())
            {
                this.debug_directory.AddressOfRawData = (int) (map.GetNextRVA(TextSegment.BlobHeap) + 0x1c);
                length = this.debug_data.Length + 0x1c;
            }
            map.AddMap(TextSegment.DebugDirectory, length, 4);
            if (!this.has_reloc)
            {
                uint num2 = map.GetNextRVA(TextSegment.DebugDirectory);
                map.AddMap(TextSegment.ImportDirectory, new Range(num2, 0));
                map.AddMap(TextSegment.ImportHintNameTable, new Range(num2, 0));
                map.AddMap(TextSegment.StartupStub, new Range(num2, 0));
                return map;
            }
            uint nextRVA = map.GetNextRVA(TextSegment.DebugDirectory);
            uint index = (uint) (((nextRVA + 0x30) + 15) & -16);
            uint num5 = (index - nextRVA) + ((uint) 0x1b);
            uint num6 = nextRVA + num5;
            map.AddMap(TextSegment.ImportDirectory, new Range(nextRVA, num5));
            map.AddMap(TextSegment.ImportHintNameTable, new Range(index, 0));
            map.AddMap(TextSegment.StartupStub, new Range((this.module.Architecture == TargetArchitecture.IA64) ? ((uint) ((num6 + 15) & -16)) : ((uint) (2 + ((num6 + 3) & -4))), this.GetStartupStubLength()));
            return map;
        }

        private Section CreateSection(string name, uint size, Section previous) => 
            new Section { 
                Name = name,
                VirtualAddress = (previous != null) ? (previous.VirtualAddress + Align(previous.VirtualSize, 0x2000)) : 0x2000,
                VirtualSize = size,
                PointerToRawData = (previous != null) ? (previous.PointerToRawData + previous.SizeOfRawData) : Align(this.GetHeaderSize(), 0x200),
                SizeOfRawData = Align(size, 0x200)
            };

        public static ImageWriter CreateWriter(ModuleDefinition module, MetadataBuilder metadata, Stream stream)
        {
            ImageWriter writer = new ImageWriter(module, metadata, stream);
            writer.BuildSections();
            return writer;
        }

        private void GetDebugHeader()
        {
            ISymbolWriter writer = this.metadata.symbol_writer;
            if ((writer != null) && !writer.GetDebugHeader(out this.debug_directory, out this.debug_data))
            {
                this.debug_data = Empty<byte>.Array;
            }
        }

        public uint GetHeaderSize() => 
            ((uint) ((0x98 + this.SizeOfOptionalHeader()) + (this.sections * 40)));

        private Section GetImageResourceSection() => 
            (this.module.HasImage ? this.module.Image.GetSection(".rsrc") : null);

        private ushort GetMachine()
        {
            switch (this.module.Architecture)
            {
                case TargetArchitecture.I386:
                    return 0x14c;

                case TargetArchitecture.AMD64:
                    return 0x8664;

                case TargetArchitecture.IA64:
                    return 0x200;

                case TargetArchitecture.ARMv7:
                    return 0x1c4;
            }
            throw new NotSupportedException();
        }

        private int GetMetadataHeaderLength() => 
            (((0x48 + (this.metadata.user_string_heap.IsEmpty ? 0 : 12)) + 0x10) + (this.metadata.blob_heap.IsEmpty ? 0 : 0x10));

        private uint GetMetadataLength() => 
            (this.text_map.GetRVA(TextSegment.DebugDirectory) - this.text_map.GetRVA(TextSegment.MetadataHeader));

        private byte[] GetRuntimeMain() => 
            (((this.module.Kind == ModuleKind.Dll) || (this.module.Kind == ModuleKind.NetModule)) ? GetSimpleString("_CorDllMain") : GetSimpleString("_CorExeMain"));

        private static byte[] GetSimpleString(string @string) => 
            GetString(@string, @string.Length);

        private uint GetStartupStubLength()
        {
            if (this.module.Architecture != TargetArchitecture.I386)
            {
                throw new NotSupportedException();
            }
            return 6;
        }

        private ushort GetStreamCount() => 
            ((ushort) (((2 + (this.metadata.user_string_heap.IsEmpty ? 0 : 1)) + 1) + (this.metadata.blob_heap.IsEmpty ? 0 : 1)));

        private static byte[] GetString(string @string, int length)
        {
            byte[] buffer = new byte[length];
            for (int i = 0; i < @string.Length; i++)
            {
                buffer[i] = (byte) @string[i];
            }
            return buffer;
        }

        private int GetStrongNameLength()
        {
            if (this.module.Assembly == null)
            {
                return 0;
            }
            byte[] publicKey = this.module.Assembly.Name.PublicKey;
            if (publicKey.IsNullOrEmpty<byte>())
            {
                return 0;
            }
            int length = publicKey.Length;
            return ((length <= 0x20) ? 0x80 : (length - 0x20));
        }

        public DataDirectory GetStrongNameSignatureDirectory() => 
            this.text_map.GetDataDirectory(TextSegment.StrongNameSignature);

        private ushort GetSubSystem()
        {
            switch (this.module.Kind)
            {
                case ModuleKind.Dll:
                case ModuleKind.Console:
                case ModuleKind.NetModule:
                    return 3;

                case ModuleKind.Windows:
                    return 2;
            }
            throw new ArgumentOutOfRangeException();
        }

        private void GetWin32Resources()
        {
            Section imageResourceSection = this.GetImageResourceSection();
            if (imageResourceSection != null)
            {
                byte[] dst = new byte[imageResourceSection.Data.Length];
                Buffer.BlockCopy(imageResourceSection.Data, 0, dst, 0, imageResourceSection.Data.Length);
                this.win32_resources = new ByteBuffer(dst);
            }
        }

        private static byte[] GetZeroTerminatedString(string @string) => 
            GetString(@string, ((@string.Length + 1) + 3) & -4);

        private Section LastSection() => 
            ((this.reloc == null) ? ((this.rsrc == null) ? this.text : this.rsrc) : this.reloc);

        private void MoveTo(uint pointer)
        {
            this.BaseStream.Seek((long) pointer, SeekOrigin.Begin);
        }

        private void MoveToRVA(TextSegment segment)
        {
            this.MoveToRVA(this.text, this.text_map.GetRVA(segment));
        }

        private void MoveToRVA(Section section, uint rva)
        {
            this.BaseStream.Seek((long) ((section.PointerToRawData + rva) - section.VirtualAddress), SeekOrigin.Begin);
        }

        private void PatchResourceDataEntry(ByteBuffer resources)
        {
            uint num = resources.ReadUInt32();
            resources.position -= 4;
            resources.WriteUInt32((num - this.GetImageResourceSection().VirtualAddress) + this.rsrc.VirtualAddress);
        }

        private void PatchResourceDirectoryEntry(ByteBuffer resources)
        {
            resources.Advance(4);
            uint num = resources.ReadUInt32();
            int position = resources.position;
            resources.position = ((int) num) & 0x7fffffff;
            if ((num & 0x80000000) != 0)
            {
                this.PatchResourceDirectoryTable(resources);
            }
            else
            {
                this.PatchResourceDataEntry(resources);
            }
            resources.position = position;
        }

        private void PatchResourceDirectoryTable(ByteBuffer resources)
        {
            resources.Advance(12);
            int num = resources.ReadUInt16() + resources.ReadUInt16();
            for (int i = 0; i < num; i++)
            {
                this.PatchResourceDirectoryEntry(resources);
            }
        }

        private void PatchWin32Resources(ByteBuffer resources)
        {
            this.PatchResourceDirectoryTable(resources);
        }

        private void PrepareSection(Section section)
        {
            this.MoveTo(section.PointerToRawData);
            if (section.SizeOfRawData <= 0x1000)
            {
                this.Write(new byte[section.SizeOfRawData]);
                this.MoveTo(section.PointerToRawData);
            }
            else
            {
                int num = 0;
                byte[] buffer = new byte[0x1000];
                while (num != section.SizeOfRawData)
                {
                    int count = Math.Min(((int) section.SizeOfRawData) - num, 0x1000);
                    this.Write(buffer, 0, count);
                    num += count;
                }
                this.MoveTo(section.PointerToRawData);
            }
        }

        private ushort SizeOfOptionalHeader() => 
            (!this.pe64 ? ((ushort) 0xe0) : ((ushort) 240));

        private void WriteDebugDirectory()
        {
            base.WriteInt32(this.debug_directory.Characteristics);
            base.WriteUInt32(this.time_stamp);
            base.WriteInt16(this.debug_directory.MajorVersion);
            base.WriteInt16(this.debug_directory.MinorVersion);
            base.WriteInt32(this.debug_directory.Type);
            base.WriteInt32(this.debug_directory.SizeOfData);
            base.WriteInt32(this.debug_directory.AddressOfRawData);
            base.WriteInt32(((int) this.BaseStream.Position) + 4);
            base.WriteBytes(this.debug_data);
        }

        private void WriteDOSHeader()
        {
            byte[] buffer = new byte[] { 
                0x4d, 90, 0x90, 0, 3, 0, 0, 0, 4, 0, 0, 0, 0xff, 0xff, 0, 0,
                0xb8, 0, 0, 0, 0, 0, 0, 0, 0x40, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0x80, 0, 0, 0,
                14, 0x1f, 0xba, 14, 0, 180, 9, 0xcd, 0x21, 0xb8, 1, 0x4c, 0xcd, 0x21, 0x54, 0x68,
                0x69, 0x73, 0x20, 0x70, 0x72, 0x6f, 0x67, 0x72, 0x61, 0x6d, 0x20, 0x63, 0x61, 110, 110, 0x6f,
                0x74, 0x20, 0x62, 0x65, 0x20, 0x72, 0x75, 110, 0x20, 0x69, 110, 0x20, 0x44, 0x4f, 0x53, 0x20,
                0x6d, 0x6f, 100, 0x65, 0x2e, 13, 13, 10, 0x24, 0, 0, 0, 0, 0, 0, 0
            };
            this.Write(buffer);
        }

        private void WriteGuidHeap()
        {
            this.MoveToRVA(TextSegment.GuidHeap);
            base.WriteBytes(this.module.Mvid.ToByteArray());
        }

        private void WriteHeap(TextSegment heap, HeapBuffer buffer)
        {
            if (!buffer.IsEmpty)
            {
                this.MoveToRVA(heap);
                base.WriteBuffer(buffer);
            }
        }

        public void WriteImage()
        {
            this.WriteDOSHeader();
            this.WritePEFileHeader();
            this.WriteOptionalHeaders();
            this.WriteSectionHeaders();
            this.WriteText();
            if (this.rsrc != null)
            {
                this.WriteRsrc();
            }
            if (this.reloc != null)
            {
                this.WriteReloc();
            }
        }

        private void WriteImportDirectory()
        {
            base.WriteUInt32(this.text_map.GetRVA(TextSegment.ImportDirectory) + ((uint) 40));
            base.WriteUInt32(0);
            base.WriteUInt32(0);
            base.WriteUInt32(this.text_map.GetRVA(TextSegment.ImportHintNameTable) + ((uint) 14));
            base.WriteUInt32(this.text_map.GetRVA(TextSegment.ImportAddressTable));
            base.Advance(20);
            base.WriteUInt32(this.text_map.GetRVA(TextSegment.ImportHintNameTable));
            this.MoveToRVA(TextSegment.ImportHintNameTable);
            base.WriteUInt16(0);
            base.WriteBytes(this.GetRuntimeMain());
            base.WriteByte(0);
            base.WriteBytes(GetSimpleString("mscoree.dll"));
            base.WriteUInt16(0);
        }

        private void WriteMetadata()
        {
            this.WriteHeap(TextSegment.TableHeap, this.metadata.table_heap);
            this.WriteHeap(TextSegment.StringHeap, this.metadata.string_heap);
            this.WriteHeap(TextSegment.UserStringHeap, this.metadata.user_string_heap);
            this.WriteGuidHeap();
            this.WriteHeap(TextSegment.BlobHeap, this.metadata.blob_heap);
        }

        private void WriteMetadataHeader()
        {
            base.WriteUInt32(0x424a5342);
            base.WriteUInt16(1);
            base.WriteUInt16(1);
            base.WriteUInt32(0);
            byte[] zeroTerminatedString = GetZeroTerminatedString(this.module.runtime_version);
            base.WriteUInt32((uint) zeroTerminatedString.Length);
            base.WriteBytes(zeroTerminatedString);
            base.WriteUInt16(0);
            base.WriteUInt16(this.GetStreamCount());
            uint offset = this.text_map.GetRVA(TextSegment.TableHeap) - this.text_map.GetRVA(TextSegment.MetadataHeader);
            this.WriteStreamHeader(ref offset, TextSegment.TableHeap, "#~");
            this.WriteStreamHeader(ref offset, TextSegment.StringHeap, "#Strings");
            this.WriteStreamHeader(ref offset, TextSegment.UserStringHeap, "#US");
            this.WriteStreamHeader(ref offset, TextSegment.GuidHeap, "#GUID");
            this.WriteStreamHeader(ref offset, TextSegment.BlobHeap, "#Blob");
        }

        private void WriteOptionalHeaders()
        {
            this.WriteUInt16(!this.pe64 ? ((ushort) 0x10b) : ((ushort) 0x20b));
            base.WriteByte(8);
            base.WriteByte(0);
            base.WriteUInt32(this.text.SizeOfRawData);
            this.WriteUInt32(((this.reloc != null) ? this.reloc.SizeOfRawData : 0) + ((this.rsrc != null) ? this.rsrc.SizeOfRawData : 0));
            base.WriteUInt32(0);
            Range range = this.text_map.GetRange(TextSegment.StartupStub);
            this.WriteUInt32((range.Length > 0) ? range.Start : 0);
            base.WriteUInt32(0x2000);
            if (this.pe64)
            {
                base.WriteUInt64(0x400000UL);
            }
            else
            {
                base.WriteUInt32(0);
                base.WriteUInt32(0x400000);
            }
            base.WriteUInt32(0x2000);
            base.WriteUInt32(0x200);
            base.WriteUInt16(4);
            base.WriteUInt16(0);
            base.WriteUInt16(0);
            base.WriteUInt16(0);
            base.WriteUInt16(4);
            base.WriteUInt16(0);
            base.WriteUInt32(0);
            Section section = this.LastSection();
            base.WriteUInt32(section.VirtualAddress + Align(section.VirtualSize, 0x2000));
            base.WriteUInt32(this.text.PointerToRawData);
            base.WriteUInt32(0);
            base.WriteUInt16(this.GetSubSystem());
            base.WriteUInt16((ushort) this.module.Characteristics);
            if (!this.pe64)
            {
                base.WriteUInt32(0x100000);
                base.WriteUInt32(0x1000);
                base.WriteUInt32(0x100000);
                base.WriteUInt32(0x1000);
            }
            else
            {
                base.WriteUInt64(0x100000UL);
                base.WriteUInt64(0x1000UL);
                base.WriteUInt64(0x100000UL);
                base.WriteUInt64(0x1000UL);
            }
            base.WriteUInt32(0);
            base.WriteUInt32(0x10);
            this.WriteZeroDataDirectory();
            base.WriteDataDirectory(this.text_map.GetDataDirectory(TextSegment.ImportDirectory));
            if (this.rsrc == null)
            {
                this.WriteZeroDataDirectory();
            }
            else
            {
                base.WriteUInt32(this.rsrc.VirtualAddress);
                base.WriteUInt32(this.rsrc.VirtualSize);
            }
            this.WriteZeroDataDirectory();
            this.WriteZeroDataDirectory();
            this.WriteUInt32((this.reloc != null) ? this.reloc.VirtualAddress : 0);
            this.WriteUInt32((this.reloc != null) ? this.reloc.VirtualSize : 0);
            if (this.text_map.GetLength(TextSegment.DebugDirectory) <= 0)
            {
                this.WriteZeroDataDirectory();
            }
            else
            {
                base.WriteUInt32(this.text_map.GetRVA(TextSegment.DebugDirectory));
                base.WriteUInt32(0x1c);
            }
            this.WriteZeroDataDirectory();
            this.WriteZeroDataDirectory();
            this.WriteZeroDataDirectory();
            this.WriteZeroDataDirectory();
            this.WriteZeroDataDirectory();
            base.WriteDataDirectory(this.text_map.GetDataDirectory(TextSegment.ImportAddressTable));
            this.WriteZeroDataDirectory();
            base.WriteDataDirectory(this.text_map.GetDataDirectory(TextSegment.CLIHeader));
            this.WriteZeroDataDirectory();
        }

        private void WritePEFileHeader()
        {
            base.WriteUInt32(0x4550);
            base.WriteUInt16(this.GetMachine());
            base.WriteUInt16(this.sections);
            base.WriteUInt32(this.time_stamp);
            base.WriteUInt32(0);
            base.WriteUInt32(0);
            base.WriteUInt16(this.SizeOfOptionalHeader());
            ushort num = (ushort) (2 | (!this.pe64 ? 0x100 : 0x20));
            if ((this.module.Kind == ModuleKind.Dll) || (this.module.Kind == ModuleKind.NetModule))
            {
                num = (ushort) (num | 0x2000);
            }
            base.WriteUInt16(num);
        }

        private void WriteReloc()
        {
            this.PrepareSection(this.reloc);
            uint num = this.text_map.GetRVA(TextSegment.StartupStub) + ((this.module.Architecture == TargetArchitecture.IA64) ? ((uint) 0x20) : 2);
            uint num2 = num & 0xfffff000;
            base.WriteUInt32(num2);
            base.WriteUInt32(12);
            if (this.module.Architecture != TargetArchitecture.I386)
            {
                throw new NotSupportedException();
            }
            base.WriteUInt32((0x3000 + num) - num2);
        }

        private void WriteRsrc()
        {
            this.PrepareSection(this.rsrc);
            base.WriteBuffer(this.win32_resources);
        }

        private void WriteRVA(uint rva)
        {
            if (!this.pe64)
            {
                base.WriteUInt32(rva);
            }
            else
            {
                base.WriteUInt64((ulong) rva);
            }
        }

        private void WriteSection(Section section, uint characteristics)
        {
            byte[] bytes = new byte[8];
            string name = section.Name;
            for (int i = 0; i < name.Length; i++)
            {
                bytes[i] = (byte) name[i];
            }
            base.WriteBytes(bytes);
            base.WriteUInt32(section.VirtualSize);
            base.WriteUInt32(section.VirtualAddress);
            base.WriteUInt32(section.SizeOfRawData);
            base.WriteUInt32(section.PointerToRawData);
            base.WriteUInt32(0);
            base.WriteUInt32(0);
            base.WriteUInt16(0);
            base.WriteUInt16(0);
            base.WriteUInt32(characteristics);
        }

        private void WriteSectionHeaders()
        {
            this.WriteSection(this.text, 0x60000020);
            if (this.rsrc != null)
            {
                this.WriteSection(this.rsrc, 0x40000040);
            }
            if (this.reloc != null)
            {
                this.WriteSection(this.reloc, 0x42000040);
            }
        }

        private void WriteStartupStub()
        {
            if (this.module.Architecture != TargetArchitecture.I386)
            {
                throw new NotSupportedException();
            }
            base.WriteUInt16(0x25ff);
            base.WriteUInt32(0x400000 + this.text_map.GetRVA(TextSegment.ImportAddressTable));
        }

        private void WriteStreamHeader(ref uint offset, TextSegment heap, string name)
        {
            uint length = (uint) this.text_map.GetLength(heap);
            if (length != 0)
            {
                base.WriteUInt32(offset);
                base.WriteUInt32(length);
                base.WriteBytes(GetZeroTerminatedString(name));
                offset += length;
            }
        }

        private void WriteText()
        {
            this.PrepareSection(this.text);
            if (this.has_reloc)
            {
                this.WriteRVA(this.text_map.GetRVA(TextSegment.ImportHintNameTable));
                this.WriteRVA(0);
            }
            base.WriteUInt32(0x48);
            base.WriteUInt16(2);
            this.WriteUInt16((this.module.Runtime <= TargetRuntime.Net_1_1) ? ((ushort) 0) : ((ushort) 5));
            base.WriteUInt32(this.text_map.GetRVA(TextSegment.MetadataHeader));
            base.WriteUInt32(this.GetMetadataLength());
            base.WriteUInt32((uint) this.module.Attributes);
            base.WriteUInt32(this.metadata.entry_point.ToUInt32());
            base.WriteDataDirectory(this.text_map.GetDataDirectory(TextSegment.Resources));
            base.WriteDataDirectory(this.text_map.GetDataDirectory(TextSegment.StrongNameSignature));
            this.WriteZeroDataDirectory();
            this.WriteZeroDataDirectory();
            this.WriteZeroDataDirectory();
            this.WriteZeroDataDirectory();
            this.MoveToRVA(TextSegment.Code);
            base.WriteBuffer(this.metadata.code);
            this.MoveToRVA(TextSegment.Resources);
            base.WriteBuffer(this.metadata.resources);
            if (this.metadata.data.length > 0)
            {
                this.MoveToRVA(TextSegment.Data);
                base.WriteBuffer(this.metadata.data);
            }
            this.MoveToRVA(TextSegment.MetadataHeader);
            this.WriteMetadataHeader();
            this.WriteMetadata();
            if (this.text_map.GetLength(TextSegment.DebugDirectory) > 0)
            {
                this.MoveToRVA(TextSegment.DebugDirectory);
                this.WriteDebugDirectory();
            }
            if (this.has_reloc)
            {
                this.MoveToRVA(TextSegment.ImportDirectory);
                this.WriteImportDirectory();
                this.MoveToRVA(TextSegment.StartupStub);
                this.WriteStartupStub();
            }
        }

        private void WriteZeroDataDirectory()
        {
            base.WriteUInt32(0);
            base.WriteUInt32(0);
        }
    }
}

