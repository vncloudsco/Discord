namespace Mono.Cecil.PE
{
    using Mono.Cecil;
    using Mono.Cecil.Metadata;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    internal sealed class ImageReader : BinaryStreamReader
    {
        private readonly Image image;
        private DataDirectory cli;
        private DataDirectory metadata;

        public ImageReader(Stream stream) : base(stream)
        {
            this.image = new Image();
            this.image.FileName = stream.GetFullyQualifiedName();
        }

        private void ComputeTableInformations()
        {
            uint num = ((uint) this.BaseStream.Position) - this.image.MetadataSection.PointerToRawData;
            int indexSize = this.image.StringHeap.IndexSize;
            int num3 = (this.image.BlobHeap != null) ? this.image.BlobHeap.IndexSize : 2;
            TableHeap tableHeap = this.image.TableHeap;
            TableInformation[] tables = tableHeap.Tables;
            for (int i = 0; i < 0x2d; i++)
            {
                Table table = (Table) ((byte) i);
                if (tableHeap.HasTable(table))
                {
                    int tableIndexSize;
                    switch (table)
                    {
                        case Table.Module:
                            tableIndexSize = (2 + indexSize) + (this.image.GuidHeap.IndexSize * 3);
                            break;

                        case Table.TypeRef:
                            tableIndexSize = this.GetCodedIndexSize(CodedIndex.ResolutionScope) + (indexSize * 2);
                            break;

                        case Table.TypeDef:
                            tableIndexSize = (((4 + (indexSize * 2)) + this.GetCodedIndexSize(CodedIndex.TypeDefOrRef)) + this.GetTableIndexSize(Table.Field)) + this.GetTableIndexSize(Table.Method);
                            break;

                        case Table.FieldPtr:
                            tableIndexSize = this.GetTableIndexSize(Table.Field);
                            break;

                        case Table.Field:
                            tableIndexSize = (2 + indexSize) + num3;
                            break;

                        case Table.MethodPtr:
                            tableIndexSize = this.GetTableIndexSize(Table.Method);
                            break;

                        case Table.Method:
                            tableIndexSize = ((8 + indexSize) + num3) + this.GetTableIndexSize(Table.Param);
                            break;

                        case Table.ParamPtr:
                            tableIndexSize = this.GetTableIndexSize(Table.Param);
                            break;

                        case Table.Param:
                            tableIndexSize = 4 + indexSize;
                            break;

                        case Table.InterfaceImpl:
                            tableIndexSize = this.GetTableIndexSize(Table.TypeDef) + this.GetCodedIndexSize(CodedIndex.TypeDefOrRef);
                            break;

                        case Table.MemberRef:
                            tableIndexSize = (this.GetCodedIndexSize(CodedIndex.MemberRefParent) + indexSize) + num3;
                            break;

                        case Table.Constant:
                            tableIndexSize = (2 + this.GetCodedIndexSize(CodedIndex.HasConstant)) + num3;
                            break;

                        case Table.CustomAttribute:
                            tableIndexSize = (this.GetCodedIndexSize(CodedIndex.HasCustomAttribute) + this.GetCodedIndexSize(CodedIndex.CustomAttributeType)) + num3;
                            break;

                        case Table.FieldMarshal:
                            tableIndexSize = this.GetCodedIndexSize(CodedIndex.HasFieldMarshal) + num3;
                            break;

                        case Table.DeclSecurity:
                            tableIndexSize = (2 + this.GetCodedIndexSize(CodedIndex.HasDeclSecurity)) + num3;
                            break;

                        case Table.ClassLayout:
                            tableIndexSize = 6 + this.GetTableIndexSize(Table.TypeDef);
                            break;

                        case Table.FieldLayout:
                            tableIndexSize = 4 + this.GetTableIndexSize(Table.Field);
                            break;

                        case Table.StandAloneSig:
                            tableIndexSize = num3;
                            break;

                        case Table.EventMap:
                            tableIndexSize = this.GetTableIndexSize(Table.TypeDef) + this.GetTableIndexSize(Table.Event);
                            break;

                        case Table.EventPtr:
                            tableIndexSize = this.GetTableIndexSize(Table.Event);
                            break;

                        case Table.Event:
                            tableIndexSize = (2 + indexSize) + this.GetCodedIndexSize(CodedIndex.TypeDefOrRef);
                            break;

                        case Table.PropertyMap:
                            tableIndexSize = this.GetTableIndexSize(Table.TypeDef) + this.GetTableIndexSize(Table.Property);
                            break;

                        case Table.PropertyPtr:
                            tableIndexSize = this.GetTableIndexSize(Table.Property);
                            break;

                        case Table.Property:
                            tableIndexSize = (2 + indexSize) + num3;
                            break;

                        case Table.MethodSemantics:
                            tableIndexSize = (2 + this.GetTableIndexSize(Table.Method)) + this.GetCodedIndexSize(CodedIndex.HasSemantics);
                            break;

                        case Table.MethodImpl:
                            tableIndexSize = (this.GetTableIndexSize(Table.TypeDef) + this.GetCodedIndexSize(CodedIndex.MethodDefOrRef)) + this.GetCodedIndexSize(CodedIndex.MethodDefOrRef);
                            break;

                        case Table.ModuleRef:
                            tableIndexSize = indexSize;
                            break;

                        case Table.TypeSpec:
                            tableIndexSize = num3;
                            break;

                        case Table.ImplMap:
                            tableIndexSize = ((2 + this.GetCodedIndexSize(CodedIndex.MemberForwarded)) + indexSize) + this.GetTableIndexSize(Table.ModuleRef);
                            break;

                        case Table.FieldRVA:
                            tableIndexSize = 4 + this.GetTableIndexSize(Table.Field);
                            break;

                        case Table.EncLog:
                            tableIndexSize = 8;
                            break;

                        case Table.EncMap:
                            tableIndexSize = 4;
                            break;

                        case Table.Assembly:
                            tableIndexSize = (0x10 + num3) + (indexSize * 2);
                            break;

                        case Table.AssemblyProcessor:
                            tableIndexSize = 4;
                            break;

                        case Table.AssemblyOS:
                            tableIndexSize = 12;
                            break;

                        case Table.AssemblyRef:
                            tableIndexSize = (12 + (num3 * 2)) + (indexSize * 2);
                            break;

                        case Table.AssemblyRefProcessor:
                            tableIndexSize = 4 + this.GetTableIndexSize(Table.AssemblyRef);
                            break;

                        case Table.AssemblyRefOS:
                            tableIndexSize = 12 + this.GetTableIndexSize(Table.AssemblyRef);
                            break;

                        case Table.File:
                            tableIndexSize = (4 + indexSize) + num3;
                            break;

                        case Table.ExportedType:
                            tableIndexSize = (8 + (indexSize * 2)) + this.GetCodedIndexSize(CodedIndex.Implementation);
                            break;

                        case Table.ManifestResource:
                            tableIndexSize = (8 + indexSize) + this.GetCodedIndexSize(CodedIndex.Implementation);
                            break;

                        case Table.NestedClass:
                            tableIndexSize = this.GetTableIndexSize(Table.TypeDef) + this.GetTableIndexSize(Table.TypeDef);
                            break;

                        case Table.GenericParam:
                            tableIndexSize = (4 + this.GetCodedIndexSize(CodedIndex.TypeOrMethodDef)) + indexSize;
                            break;

                        case Table.MethodSpec:
                            tableIndexSize = this.GetCodedIndexSize(CodedIndex.MethodDefOrRef) + num3;
                            break;

                        case Table.GenericParamConstraint:
                            tableIndexSize = this.GetTableIndexSize(Table.GenericParam) + this.GetCodedIndexSize(CodedIndex.TypeDefOrRef);
                            break;

                        default:
                            throw new NotSupportedException();
                    }
                    tables[i].RowSize = (uint) tableIndexSize;
                    tables[i].Offset = num;
                    num += (uint) (tableIndexSize * tables[i].Length);
                }
            }
        }

        private int GetCodedIndexSize(CodedIndex index) => 
            this.image.GetCodedIndexSize(index);

        private static ModuleKind GetModuleKind(ushort characteristics, ushort subsystem) => 
            (((characteristics & 0x2000) == 0) ? (((subsystem == 2) || (subsystem == 9)) ? ModuleKind.Windows : ModuleKind.Console) : ModuleKind.Dll);

        private int GetTableIndexSize(Table table) => 
            this.image.GetTableIndexSize(table);

        private void MoveTo(DataDirectory directory)
        {
            this.BaseStream.Position = this.image.ResolveVirtualAddress(directory.VirtualAddress);
        }

        private void MoveTo(uint position)
        {
            this.BaseStream.Position = position;
        }

        private string ReadAlignedString(int length)
        {
            int num = 0;
            char[] chArray = new char[length];
            while (true)
            {
                if (num < length)
                {
                    byte num2 = this.ReadByte();
                    if (num2 != 0)
                    {
                        chArray[num++] = (char) num2;
                        continue;
                    }
                }
                base.Advance((-1 + ((num + 4) & -4)) - num);
                return new string(chArray, 0, num);
            }
        }

        private TargetArchitecture ReadArchitecture()
        {
            ushort num2 = this.ReadUInt16();
            if (num2 <= 0x1c4)
            {
                if (num2 == 0x14c)
                {
                    return TargetArchitecture.I386;
                }
                if (num2 == 0x1c4)
                {
                    return TargetArchitecture.ARMv7;
                }
            }
            else
            {
                if (num2 == 0x200)
                {
                    return TargetArchitecture.IA64;
                }
                if (num2 == 0x8664)
                {
                    return TargetArchitecture.AMD64;
                }
            }
            throw new NotSupportedException();
        }

        private void ReadCLIHeader()
        {
            this.MoveTo(this.cli);
            base.Advance(8);
            this.metadata = base.ReadDataDirectory();
            this.image.Attributes = (ModuleAttributes) this.ReadUInt32();
            this.image.EntryPointToken = this.ReadUInt32();
            this.image.Resources = base.ReadDataDirectory();
            this.image.StrongName = base.ReadDataDirectory();
        }

        private void ReadImage()
        {
            ushort num3;
            ushort num4;
            if (this.BaseStream.Length < 0x80L)
            {
                throw new BadImageFormatException();
            }
            if (this.ReadUInt16() != 0x5a4d)
            {
                throw new BadImageFormatException();
            }
            base.Advance(0x3a);
            this.MoveTo(this.ReadUInt32());
            if (this.ReadUInt32() != 0x4550)
            {
                throw new BadImageFormatException();
            }
            this.image.Architecture = this.ReadArchitecture();
            ushort count = this.ReadUInt16();
            base.Advance(14);
            ushort characteristics = this.ReadUInt16();
            this.ReadOptionalHeaders(out num3, out num4);
            this.ReadSections(count);
            this.ReadCLIHeader();
            this.ReadMetadata();
            this.image.Kind = GetModuleKind(characteristics, num3);
            this.image.Characteristics = (ModuleCharacteristics) num4;
        }

        public static Image ReadImageFrom(Stream stream)
        {
            Image image;
            try
            {
                ImageReader reader = new ImageReader(stream);
                reader.ReadImage();
                image = reader.image;
            }
            catch (EndOfStreamException exception)
            {
                throw new BadImageFormatException(stream.GetFullyQualifiedName(), exception);
            }
            return image;
        }

        private void ReadMetadata()
        {
            this.MoveTo(this.metadata);
            if (this.ReadUInt32() != 0x424a5342)
            {
                throw new BadImageFormatException();
            }
            base.Advance(8);
            this.image.RuntimeVersion = this.ReadZeroTerminatedString(this.ReadInt32());
            base.Advance(2);
            ushort num = this.ReadUInt16();
            Section sectionAtVirtualAddress = this.image.GetSectionAtVirtualAddress(this.metadata.VirtualAddress);
            if (sectionAtVirtualAddress == null)
            {
                throw new BadImageFormatException();
            }
            this.image.MetadataSection = sectionAtVirtualAddress;
            for (int i = 0; i < num; i++)
            {
                this.ReadMetadataStream(sectionAtVirtualAddress);
            }
            if (this.image.TableHeap != null)
            {
                this.ReadTableHeap();
            }
        }

        private void ReadMetadataStream(Section section)
        {
            uint start = (this.metadata.VirtualAddress - section.VirtualAddress) + this.ReadUInt32();
            uint size = this.ReadUInt32();
            string str2 = this.ReadAlignedString(0x10);
            if (str2 != null)
            {
                if ((str2 == "#~") || (str2 == "#-"))
                {
                    this.image.TableHeap = new TableHeap(section, start, size);
                }
                else if (str2 == "#Strings")
                {
                    this.image.StringHeap = new StringHeap(section, start, size);
                }
                else if (str2 == "#Blob")
                {
                    this.image.BlobHeap = new BlobHeap(section, start, size);
                }
                else if (str2 == "#GUID")
                {
                    this.image.GuidHeap = new GuidHeap(section, start, size);
                }
                else if (str2 == "#US")
                {
                    this.image.UserStringHeap = new UserStringHeap(section, start, size);
                }
            }
        }

        private void ReadOptionalHeaders(out ushort subsystem, out ushort dll_characteristics)
        {
            bool flag = this.ReadUInt16() == 0x20b;
            base.Advance(0x42);
            subsystem = this.ReadUInt16();
            dll_characteristics = this.ReadUInt16();
            this.Advance(flag ? 0x58 : 0x48);
            this.image.Debug = base.ReadDataDirectory();
            base.Advance(0x38);
            this.cli = base.ReadDataDirectory();
            if (this.cli.IsZero)
            {
                throw new BadImageFormatException();
            }
            base.Advance(8);
        }

        private void ReadSectionData(Section section)
        {
            int num4;
            long position = this.BaseStream.Position;
            this.MoveTo(section.PointerToRawData);
            int sizeOfRawData = (int) section.SizeOfRawData;
            byte[] buffer = new byte[sizeOfRawData];
            for (int i = 0; (num4 = this.Read(buffer, i, sizeOfRawData - i)) > 0; i += num4)
            {
            }
            section.Data = buffer;
            this.BaseStream.Position = position;
        }

        private void ReadSections(ushort count)
        {
            Section[] sectionArray = new Section[count];
            for (int i = 0; i < count; i++)
            {
                Section section = new Section {
                    Name = this.ReadZeroTerminatedString(8)
                };
                base.Advance(4);
                section.VirtualAddress = this.ReadUInt32();
                section.SizeOfRawData = this.ReadUInt32();
                section.PointerToRawData = this.ReadUInt32();
                base.Advance(0x10);
                sectionArray[i] = section;
                this.ReadSectionData(section);
            }
            this.image.Sections = sectionArray;
        }

        private void ReadTableHeap()
        {
            TableHeap tableHeap = this.image.TableHeap;
            uint pointerToRawData = tableHeap.Section.PointerToRawData;
            this.MoveTo((uint) (tableHeap.Offset + pointerToRawData));
            base.Advance(6);
            byte sizes = this.ReadByte();
            base.Advance(1);
            tableHeap.Valid = this.ReadInt64();
            tableHeap.Sorted = this.ReadInt64();
            for (int i = 0; i < 0x2d; i++)
            {
                if (tableHeap.HasTable((Table) ((byte) i)))
                {
                    tableHeap.Tables[i].Length = this.ReadUInt32();
                }
            }
            SetIndexSize(this.image.StringHeap, sizes, 1);
            SetIndexSize(this.image.GuidHeap, sizes, 2);
            SetIndexSize(this.image.BlobHeap, sizes, 4);
            this.ComputeTableInformations();
        }

        private string ReadZeroTerminatedString(int length)
        {
            int index = 0;
            char[] chArray = new char[length];
            byte[] buffer = this.ReadBytes(length);
            while (true)
            {
                if (index < length)
                {
                    byte num2 = buffer[index];
                    if (num2 != 0)
                    {
                        chArray[index++] = (char) num2;
                        continue;
                    }
                }
                return new string(chArray, 0, index);
            }
        }

        private static void SetIndexSize(Heap heap, uint sizes, byte flag)
        {
            if (heap != null)
            {
                heap.IndexSize = ((sizes & flag) > 0) ? 4 : 2;
            }
        }
    }
}

