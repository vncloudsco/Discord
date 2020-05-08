namespace Mono.Cecil
{
    using Mono;
    using Mono.Cecil.Cil;
    using Mono.Cecil.Metadata;
    using Mono.Cecil.PE;
    using Mono.Collections.Generic;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    internal sealed class MetadataReader : ByteBuffer
    {
        internal readonly Image image;
        internal readonly ModuleDefinition module;
        internal readonly MetadataSystem metadata;
        internal IGenericContext context;
        internal CodeReader code;

        public MetadataReader(ModuleDefinition module) : base(module.Image.MetadataSection.Data)
        {
            this.image = module.Image;
            this.module = module;
            this.metadata = module.MetadataSystem;
            this.code = new CodeReader(this.image.MetadataSection, this);
        }

        private void AddGenericConstraintMapping(uint generic_parameter, MetadataToken constraint)
        {
            this.metadata.SetGenericConstraintMapping(generic_parameter, AddMapping<uint, MetadataToken>(this.metadata.GenericConstraints, generic_parameter, constraint));
        }

        private void AddInterfaceMapping(uint type, MetadataToken @interface)
        {
            this.metadata.SetInterfaceMapping(type, AddMapping<uint, MetadataToken>(this.metadata.Interfaces, type, @interface));
        }

        private static TValue[] AddMapping<TKey, TValue>(Dictionary<TKey, TValue[]> cache, TKey key, TValue value)
        {
            TValue[] localArray;
            if (!cache.TryGetValue(key, out localArray))
            {
                return new TValue[] { value };
            }
            TValue[] destinationArray = new TValue[localArray.Length + 1];
            Array.Copy(localArray, destinationArray, localArray.Length);
            destinationArray[localArray.Length] = value;
            return destinationArray;
        }

        private void AddNestedMapping(uint declaring, uint nested)
        {
            this.metadata.SetNestedTypeMapping(declaring, AddMapping<uint, uint>(this.metadata.NestedTypes, declaring, nested));
            this.metadata.SetReverseNestedTypeMapping(nested, declaring);
        }

        private void AddOverrideMapping(uint method_rid, MetadataToken @override)
        {
            this.metadata.SetOverrideMapping(method_rid, AddMapping<uint, MetadataToken>(this.metadata.Overrides, method_rid, @override));
        }

        private static void AddRange(Dictionary<MetadataToken, Range[]> ranges, MetadataToken owner, Range range)
        {
            if (owner.RID != 0)
            {
                Range[] rangeArray;
                if (!ranges.TryGetValue(owner, out rangeArray))
                {
                    Range[] rangeArray2 = new Range[] { range };
                    ranges.Add(owner, rangeArray2);
                }
                else
                {
                    rangeArray = rangeArray.Resize<Range>(rangeArray.Length + 1);
                    rangeArray[rangeArray.Length - 1] = range;
                    ranges[owner] = rangeArray;
                }
            }
        }

        private void CompleteTypes()
        {
            foreach (TypeDefinition definition in this.metadata.Types)
            {
                InitializeCollection(definition.Fields);
                InitializeCollection(definition.Methods);
            }
        }

        private int GetCodedIndexSize(CodedIndex index) => 
            this.image.GetCodedIndexSize(index);

        private static EventDefinition GetEvent(TypeDefinition type, MetadataToken token)
        {
            if (token.TokenType != TokenType.Event)
            {
                throw new ArgumentException();
            }
            return GetMember<EventDefinition>(type.Events, token);
        }

        private IMetadataScope GetExportedTypeScope(MetadataToken token)
        {
            IMetadataScope moduleReferenceFromFile;
            int position = base.position;
            TokenType tokenType = token.TokenType;
            if (tokenType == TokenType.AssemblyRef)
            {
                this.InitializeAssemblyReferences();
                moduleReferenceFromFile = this.metadata.AssemblyReferences[((int) token.RID) - 1];
            }
            else
            {
                if (tokenType != TokenType.File)
                {
                    throw new NotSupportedException();
                }
                this.InitializeModuleReferences();
                moduleReferenceFromFile = this.GetModuleReferenceFromFile(token);
            }
            base.position = position;
            return moduleReferenceFromFile;
        }

        public FieldDefinition GetFieldDefinition(uint rid)
        {
            this.InitializeTypeDefinitions();
            FieldDefinition fieldDefinition = this.metadata.GetFieldDefinition(rid);
            return ((fieldDefinition == null) ? this.LookupField(rid) : fieldDefinition);
        }

        private byte[] GetFieldInitializeValue(int size, uint rva)
        {
            Section sectionAtVirtualAddress = this.image.GetSectionAtVirtualAddress(rva);
            if (sectionAtVirtualAddress == null)
            {
                return Empty<byte>.Array;
            }
            byte[] dst = new byte[size];
            Buffer.BlockCopy(sectionAtVirtualAddress.Data, (int) (rva - sectionAtVirtualAddress.VirtualAddress), dst, 0, size);
            return dst;
        }

        private static int GetFieldTypeSize(TypeReference type)
        {
            int size = 0;
            switch (type.etype)
            {
                case ElementType.Boolean:
                case ElementType.I1:
                case ElementType.U1:
                    size = 1;
                    break;

                case ElementType.Char:
                case ElementType.I2:
                case ElementType.U2:
                    size = 2;
                    break;

                case ElementType.I4:
                case ElementType.U4:
                case ElementType.R4:
                    size = 4;
                    break;

                case ElementType.I8:
                case ElementType.U8:
                case ElementType.R8:
                    size = 8;
                    break;

                case ElementType.Ptr:
                case ElementType.FnPtr:
                    size = IntPtr.Size;
                    break;

                case ElementType.CModReqD:
                case ElementType.CModOpt:
                    return GetFieldTypeSize(((IModifierType) type).ElementType);

                default:
                {
                    TypeDefinition definition = type.Resolve();
                    if ((definition != null) && definition.HasLayoutInfo)
                    {
                        size = definition.ClassSize;
                    }
                    break;
                }
            }
            return size;
        }

        public MemoryStream GetManagedResourceStream(uint offset)
        {
            uint virtualAddress = this.image.Resources.VirtualAddress;
            Section sectionAtVirtualAddress = this.image.GetSectionAtVirtualAddress(virtualAddress);
            uint index = (virtualAddress - sectionAtVirtualAddress.VirtualAddress) + offset;
            byte[] data = sectionAtVirtualAddress.Data;
            return new MemoryStream(data, ((int) index) + 4, ((data[index] | (data[(int) ((IntPtr) (index + 1))] << 8)) | (data[(int) ((IntPtr) (index + 2))] << 0x10)) | (data[(int) ((IntPtr) (index + 3))] << 0x18));
        }

        private static TMember GetMember<TMember>(Collection<TMember> members, MetadataToken token) where TMember: IMemberDefinition
        {
            for (int i = 0; i < members.Count; i++)
            {
                TMember local = members[i];
                if (local.MetadataToken == token)
                {
                    return local;
                }
            }
            throw new ArgumentException();
        }

        private MemberReference GetMemberReference(uint rid)
        {
            this.InitializeMemberReferences();
            MemberReference memberReference = this.metadata.GetMemberReference(rid);
            if (memberReference == null)
            {
                memberReference = this.ReadMemberReference(rid);
                if ((memberReference != null) && !memberReference.ContainsGenericParameter)
                {
                    this.metadata.AddMemberReference(memberReference);
                }
            }
            return memberReference;
        }

        public IEnumerable<MemberReference> GetMemberReferences()
        {
            this.InitializeMemberReferences();
            int tableLength = this.image.GetTableLength(Table.MemberRef);
            TypeSystem typeSystem = this.module.TypeSystem;
            MethodReference reference = new MethodReference(string.Empty, typeSystem.Void) {
                DeclaringType = new TypeReference(string.Empty, string.Empty, this.module, typeSystem.Corlib)
            };
            MemberReference[] referenceArray = new MemberReference[tableLength];
            for (uint i = 1; i <= tableLength; i++)
            {
                this.context = reference;
                referenceArray[(int) ((IntPtr) (i - 1))] = this.GetMemberReference(i);
            }
            return referenceArray;
        }

        public MethodDefinition GetMethodDefinition(uint rid)
        {
            this.InitializeTypeDefinitions();
            MethodDefinition methodDefinition = this.metadata.GetMethodDefinition(rid);
            return ((methodDefinition == null) ? this.LookupMethod(rid) : methodDefinition);
        }

        private MethodSpecification GetMethodSpecification(uint rid)
        {
            if (!this.MoveTo(Table.MethodSpec, rid))
            {
                return null;
            }
            uint signature = this.ReadBlobIndex();
            MethodSpecification specification = this.ReadMethodSpecSignature(signature, (MethodReference) this.LookupToken(this.ReadMetadataToken(CodedIndex.MethodDefOrRef)));
            specification.token = new MetadataToken(TokenType.MethodSpec, rid);
            return specification;
        }

        private string GetModuleFileName(string name)
        {
            if (this.module.FullyQualifiedName == null)
            {
                throw new NotSupportedException();
            }
            return Path.Combine(Path.GetDirectoryName(this.module.FullyQualifiedName), name);
        }

        private ModuleReference GetModuleReferenceFromFile(MetadataToken token)
        {
            ModuleReference reference;
            if (!this.MoveTo(Table.File, token.RID))
            {
                return null;
            }
            base.ReadUInt32();
            string name = this.ReadString();
            Collection<ModuleReference> moduleReferences = this.module.ModuleReferences;
            for (int i = 0; i < moduleReferences.Count; i++)
            {
                reference = moduleReferences[i];
                if (reference.Name == name)
                {
                    return reference;
                }
            }
            reference = new ModuleReference(name);
            moduleReferences.Add(reference);
            return reference;
        }

        private TypeDefinition GetNestedTypeDeclaringType(TypeDefinition type)
        {
            uint num;
            if (!this.metadata.TryGetReverseNestedTypeMapping(type, out num))
            {
                return null;
            }
            this.metadata.RemoveReverseNestedTypeMapping(type);
            return this.GetTypeDefinition(num);
        }

        private static PropertyDefinition GetProperty(TypeDefinition type, MetadataToken token)
        {
            if (token.TokenType != TokenType.Property)
            {
                throw new ArgumentException();
            }
            return GetMember<PropertyDefinition>(type.Properties, token);
        }

        public TypeDefinition GetTypeDefinition(uint rid)
        {
            this.InitializeTypeDefinitions();
            TypeDefinition typeDefinition = this.metadata.GetTypeDefinition(rid);
            return ((typeDefinition == null) ? this.ReadTypeDefinition(rid) : typeDefinition);
        }

        public TypeReference GetTypeDefOrRef(MetadataToken token) => 
            ((TypeReference) this.LookupToken(token));

        private TypeReference GetTypeReference(uint rid)
        {
            this.InitializeTypeReferences();
            TypeReference typeReference = this.metadata.GetTypeReference(rid);
            return ((typeReference == null) ? this.ReadTypeReference(rid) : typeReference);
        }

        public TypeReference GetTypeReference(string scope, string full_name)
        {
            this.InitializeTypeReferences();
            int length = this.metadata.TypeReferences.Length;
            for (uint i = 1; i <= length; i++)
            {
                TypeReference typeReference = this.GetTypeReference(i);
                if (typeReference.FullName == full_name)
                {
                    if (string.IsNullOrEmpty(scope))
                    {
                        return typeReference;
                    }
                    if (typeReference.Scope.Name == scope)
                    {
                        return typeReference;
                    }
                }
            }
            return null;
        }

        public IEnumerable<TypeReference> GetTypeReferences()
        {
            this.InitializeTypeReferences();
            int tableLength = this.image.GetTableLength(Table.TypeRef);
            TypeReference[] referenceArray = new TypeReference[tableLength];
            for (uint i = 1; i <= tableLength; i++)
            {
                referenceArray[(int) ((IntPtr) (i - 1))] = this.GetTypeReference(i);
            }
            return referenceArray;
        }

        private IMetadataScope GetTypeReferenceScope(MetadataToken scope)
        {
            IMetadataScope[] moduleReferences;
            if (scope.TokenType == TokenType.Module)
            {
                return this.module;
            }
            TokenType tokenType = scope.TokenType;
            if (tokenType == TokenType.ModuleRef)
            {
                this.InitializeModuleReferences();
                moduleReferences = this.metadata.ModuleReferences;
            }
            else
            {
                if (tokenType != TokenType.AssemblyRef)
                {
                    throw new NotSupportedException();
                }
                this.InitializeAssemblyReferences();
                moduleReferences = this.metadata.AssemblyReferences;
            }
            uint index = scope.RID - 1;
            return (((index < 0) || (index >= moduleReferences.Length)) ? null : moduleReferences[index]);
        }

        private TypeReference GetTypeSpecification(uint rid)
        {
            if (!this.MoveTo(Table.TypeSpec, rid))
            {
                return null;
            }
            TypeReference reference = this.ReadSignature(this.ReadBlobIndex()).ReadTypeSignature();
            if (reference.token.RID == 0)
            {
                reference.token = new MetadataToken(TokenType.TypeSpec, rid);
            }
            return reference;
        }

        public bool HasCustomAttributes(ICustomAttributeProvider owner)
        {
            Range[] rangeArray;
            this.InitializeCustomAttributes();
            return (this.metadata.TryGetCustomAttributeRanges(owner, out rangeArray) ? (RangesSize(rangeArray) > 0) : false);
        }

        public bool HasEvents(TypeDefinition type)
        {
            Range range;
            this.InitializeEvents();
            return (this.metadata.TryGetEventsRange(type, out range) ? (range.Length != 0) : false);
        }

        public bool HasFileResource()
        {
            int num = this.MoveTo(Table.File);
            if (num != 0)
            {
                for (uint i = 1; i <= num; i++)
                {
                    if (((FileAttributes) this.ReadFileRecord(i).Col1) == FileAttributes.ContainsNoMetaData)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool HasGenericConstraints(GenericParameter generic_parameter)
        {
            MetadataToken[] tokenArray;
            this.InitializeGenericConstraints();
            return (this.metadata.TryGetGenericConstraintMapping(generic_parameter, out tokenArray) ? (tokenArray.Length > 0) : false);
        }

        public bool HasGenericParameters(IGenericParameterProvider provider)
        {
            Range[] rangeArray;
            this.InitializeGenericParameters();
            return (this.metadata.TryGetGenericParameterRanges(provider, out rangeArray) ? (RangesSize(rangeArray) > 0) : false);
        }

        public bool HasInterfaces(TypeDefinition type)
        {
            MetadataToken[] tokenArray;
            this.InitializeInterfaces();
            return this.metadata.TryGetInterfaceMapping(type, out tokenArray);
        }

        public bool HasMarshalInfo(IMarshalInfoProvider owner)
        {
            this.InitializeMarshalInfos();
            return this.metadata.FieldMarshals.ContainsKey(owner.MetadataToken);
        }

        public bool HasNestedTypes(TypeDefinition type)
        {
            uint[] numArray;
            this.InitializeNestedTypes();
            return (this.metadata.TryGetNestedTypeMapping(type, out numArray) ? (numArray.Length > 0) : false);
        }

        public bool HasOverrides(MethodDefinition method)
        {
            MetadataToken[] tokenArray;
            this.InitializeOverrides();
            return (this.metadata.TryGetOverrideMapping(method, out tokenArray) ? (tokenArray.Length > 0) : false);
        }

        public bool HasProperties(TypeDefinition type)
        {
            Range range;
            this.InitializeProperties();
            return (this.metadata.TryGetPropertiesRange(type, out range) ? (range.Length != 0) : false);
        }

        public bool HasSecurityDeclarations(ISecurityDeclarationProvider owner)
        {
            Range[] rangeArray;
            this.InitializeSecurityDeclarations();
            return (this.metadata.TryGetSecurityDeclarationRanges(owner, out rangeArray) ? (RangesSize(rangeArray) > 0) : false);
        }

        private void InitializeAssemblyReferences()
        {
            if (this.metadata.AssemblyReferences == null)
            {
                int num = this.MoveTo(Table.AssemblyRef);
                AssemblyNameReference[] referenceArray = this.metadata.AssemblyReferences = new AssemblyNameReference[num];
                for (uint i = 0; i < num; i++)
                {
                    AssemblyNameReference name = new AssemblyNameReference {
                        token = new MetadataToken(TokenType.AssemblyRef, i + 1)
                    };
                    this.PopulateVersionAndFlags(name);
                    byte[] buffer = this.ReadBlob();
                    if (name.HasPublicKey)
                    {
                        name.PublicKey = buffer;
                    }
                    else
                    {
                        name.PublicKeyToken = buffer;
                    }
                    this.PopulateNameAndCulture(name);
                    name.Hash = this.ReadBlob();
                    referenceArray[i] = name;
                }
            }
        }

        private static void InitializeCollection(object o)
        {
        }

        private void InitializeConstants()
        {
            if (this.metadata.Constants == null)
            {
                int capacity = this.MoveTo(Table.Constant);
                Dictionary<MetadataToken, Row<ElementType, uint>> dictionary = this.metadata.Constants = new Dictionary<MetadataToken, Row<ElementType, uint>>(capacity);
                for (uint i = 1; i <= capacity; i++)
                {
                    ElementType type = (ElementType) ((byte) base.ReadUInt16());
                    MetadataToken key = this.ReadMetadataToken(CodedIndex.HasConstant);
                    uint num3 = this.ReadBlobIndex();
                    dictionary.Add(key, new Row<ElementType, uint>(type, num3));
                }
            }
        }

        private void InitializeCustomAttributes()
        {
            if (this.metadata.CustomAttributes == null)
            {
                this.metadata.CustomAttributes = this.InitializeRanges(Table.CustomAttribute, delegate {
                    MetadataToken token = this.ReadMetadataToken(CodedIndex.HasCustomAttribute);
                    this.ReadMetadataToken(CodedIndex.CustomAttributeType);
                    this.ReadBlobIndex();
                    return token;
                });
            }
        }

        private void InitializeEvents()
        {
            if (this.metadata.Events == null)
            {
                int capacity = this.MoveTo(Table.EventMap);
                this.metadata.Events = new Dictionary<uint, Range>(capacity);
                for (uint i = 1; i <= capacity; i++)
                {
                    uint num3 = this.ReadTableIndex(Table.TypeDef);
                    Range range = this.ReadEventsRange(i);
                    this.metadata.AddEventsRange(num3, range);
                }
            }
        }

        private void InitializeFieldLayouts()
        {
            if (this.metadata.FieldLayouts == null)
            {
                int capacity = this.MoveTo(Table.FieldLayout);
                Dictionary<uint, uint> dictionary = this.metadata.FieldLayouts = new Dictionary<uint, uint>(capacity);
                for (int i = 0; i < capacity; i++)
                {
                    uint num3 = base.ReadUInt32();
                    uint key = this.ReadTableIndex(Table.Field);
                    dictionary.Add(key, num3);
                }
            }
        }

        private void InitializeFieldRVAs()
        {
            if (this.metadata.FieldRVAs == null)
            {
                int capacity = this.MoveTo(Table.FieldRVA);
                Dictionary<uint, uint> dictionary = this.metadata.FieldRVAs = new Dictionary<uint, uint>(capacity);
                for (int i = 0; i < capacity; i++)
                {
                    uint num3 = base.ReadUInt32();
                    uint key = this.ReadTableIndex(Table.Field);
                    dictionary.Add(key, num3);
                }
            }
        }

        private void InitializeFields()
        {
            if (this.metadata.Fields == null)
            {
                this.metadata.Fields = new FieldDefinition[this.image.GetTableLength(Table.Field)];
            }
        }

        private void InitializeGenericConstraints()
        {
            if (this.metadata.GenericConstraints == null)
            {
                int capacity = this.MoveTo(Table.GenericParamConstraint);
                this.metadata.GenericConstraints = new Dictionary<uint, MetadataToken[]>(capacity);
                for (int i = 1; i <= capacity; i++)
                {
                    this.AddGenericConstraintMapping(this.ReadTableIndex(Table.GenericParam), this.ReadMetadataToken(CodedIndex.TypeDefOrRef));
                }
            }
        }

        private void InitializeGenericParameters()
        {
            if (this.metadata.GenericParameters == null)
            {
                this.metadata.GenericParameters = this.InitializeRanges(Table.GenericParam, delegate {
                    base.Advance(4);
                    MetadataToken token = this.ReadMetadataToken(CodedIndex.TypeOrMethodDef);
                    this.ReadStringIndex();
                    return token;
                });
            }
        }

        private void InitializeInterfaces()
        {
            if (this.metadata.Interfaces == null)
            {
                int capacity = this.MoveTo(Table.InterfaceImpl);
                this.metadata.Interfaces = new Dictionary<uint, MetadataToken[]>(capacity);
                for (int i = 0; i < capacity; i++)
                {
                    uint type = this.ReadTableIndex(Table.TypeDef);
                    MetadataToken token = this.ReadMetadataToken(CodedIndex.TypeDefOrRef);
                    this.AddInterfaceMapping(type, token);
                }
            }
        }

        private void InitializeMarshalInfos()
        {
            if (this.metadata.FieldMarshals == null)
            {
                int capacity = this.MoveTo(Table.FieldMarshal);
                Dictionary<MetadataToken, uint> dictionary = this.metadata.FieldMarshals = new Dictionary<MetadataToken, uint>(capacity);
                for (int i = 0; i < capacity; i++)
                {
                    MetadataToken key = this.ReadMetadataToken(CodedIndex.HasFieldMarshal);
                    uint num3 = this.ReadBlobIndex();
                    if (key.RID != 0)
                    {
                        dictionary.Add(key, num3);
                    }
                }
            }
        }

        private void InitializeMemberReferences()
        {
            if (this.metadata.MemberReferences == null)
            {
                this.metadata.MemberReferences = new MemberReference[this.image.GetTableLength(Table.MemberRef)];
            }
        }

        private void InitializeMethods()
        {
            if (this.metadata.Methods == null)
            {
                this.metadata.Methods = new MethodDefinition[this.image.GetTableLength(Table.Method)];
            }
        }

        private void InitializeMethodSemantics()
        {
            if (this.metadata.Semantics == null)
            {
                int num = this.MoveTo(Table.MethodSemantics);
                Dictionary<uint, Row<MethodSemanticsAttributes, MetadataToken>> dictionary = this.metadata.Semantics = new Dictionary<uint, Row<MethodSemanticsAttributes, MetadataToken>>(0);
                for (uint i = 0; i < num; i++)
                {
                    MethodSemanticsAttributes attributes = (MethodSemanticsAttributes) base.ReadUInt16();
                    uint num3 = this.ReadTableIndex(Table.Method);
                    MetadataToken token = this.ReadMetadataToken(CodedIndex.HasSemantics);
                    dictionary[num3] = new Row<MethodSemanticsAttributes, MetadataToken>(attributes, token);
                }
            }
        }

        private void InitializeModuleReferences()
        {
            if (this.metadata.ModuleReferences == null)
            {
                int num = this.MoveTo(Table.ModuleRef);
                ModuleReference[] referenceArray = this.metadata.ModuleReferences = new ModuleReference[num];
                for (uint i = 0; i < num; i++)
                {
                    referenceArray[i] = new ModuleReference(this.ReadString()) { token = new MetadataToken(TokenType.ModuleRef, i + 1) };
                }
            }
        }

        private void InitializeNestedTypes()
        {
            if (this.metadata.NestedTypes == null)
            {
                int capacity = this.MoveTo(Table.NestedClass);
                this.metadata.NestedTypes = new Dictionary<uint, uint[]>(capacity);
                this.metadata.ReverseNestedTypes = new Dictionary<uint, uint>(capacity);
                if (capacity != 0)
                {
                    for (int i = 1; i <= capacity; i++)
                    {
                        uint nested = this.ReadTableIndex(Table.TypeDef);
                        uint declaring = this.ReadTableIndex(Table.TypeDef);
                        this.AddNestedMapping(declaring, nested);
                    }
                }
            }
        }

        private void InitializeOverrides()
        {
            if (this.metadata.Overrides == null)
            {
                int capacity = this.MoveTo(Table.MethodImpl);
                this.metadata.Overrides = new Dictionary<uint, MetadataToken[]>(capacity);
                for (int i = 1; i <= capacity; i++)
                {
                    this.ReadTableIndex(Table.TypeDef);
                    MetadataToken token = this.ReadMetadataToken(CodedIndex.MethodDefOrRef);
                    if (token.TokenType != TokenType.Method)
                    {
                        throw new NotSupportedException();
                    }
                    MetadataToken @override = this.ReadMetadataToken(CodedIndex.MethodDefOrRef);
                    this.AddOverrideMapping(token.RID, @override);
                }
            }
        }

        private void InitializePInvokes()
        {
            if (this.metadata.PInvokes == null)
            {
                int capacity = this.MoveTo(Table.ImplMap);
                Dictionary<uint, Row<PInvokeAttributes, uint, uint>> dictionary = this.metadata.PInvokes = new Dictionary<uint, Row<PInvokeAttributes, uint, uint>>(capacity);
                for (int i = 1; i <= capacity; i++)
                {
                    PInvokeAttributes attributes = (PInvokeAttributes) base.ReadUInt16();
                    MetadataToken token = this.ReadMetadataToken(CodedIndex.MemberForwarded);
                    uint num3 = this.ReadStringIndex();
                    uint num4 = this.ReadTableIndex(Table.File);
                    if (token.TokenType == TokenType.Method)
                    {
                        dictionary.Add(token.RID, new Row<PInvokeAttributes, uint, uint>(attributes, num3, num4));
                    }
                }
            }
        }

        private void InitializeProperties()
        {
            if (this.metadata.Properties == null)
            {
                int capacity = this.MoveTo(Table.PropertyMap);
                this.metadata.Properties = new Dictionary<uint, Range>(capacity);
                for (uint i = 1; i <= capacity; i++)
                {
                    uint num3 = this.ReadTableIndex(Table.TypeDef);
                    Range range = this.ReadPropertiesRange(i);
                    this.metadata.AddPropertiesRange(num3, range);
                }
            }
        }

        private unsafe Dictionary<MetadataToken, Range[]> InitializeRanges(Table table, Func<MetadataToken> get_next)
        {
            int capacity = this.MoveTo(table);
            Dictionary<MetadataToken, Range[]> ranges = new Dictionary<MetadataToken, Range[]>(capacity);
            if (capacity != 0)
            {
                MetadataToken zero = MetadataToken.Zero;
                Range range = new Range(1, 0);
                for (uint i = 1; i <= capacity; i++)
                {
                    MetadataToken token2 = get_next();
                    if (i == 1)
                    {
                        zero = token2;
                        Range* rangePtr1 = &range;
                        rangePtr1->Length++;
                    }
                    else if (!(token2 != zero))
                    {
                        Range* rangePtr2 = &range;
                        rangePtr2->Length++;
                    }
                    else
                    {
                        AddRange(ranges, zero, range);
                        range = new Range(i, 1);
                        zero = token2;
                    }
                }
                AddRange(ranges, zero, range);
            }
            return ranges;
        }

        private void InitializeSecurityDeclarations()
        {
            if (this.metadata.SecurityDeclarations == null)
            {
                this.metadata.SecurityDeclarations = this.InitializeRanges(Table.DeclSecurity, delegate {
                    base.ReadUInt16();
                    MetadataToken token = this.ReadMetadataToken(CodedIndex.HasDeclSecurity);
                    this.ReadBlobIndex();
                    return token;
                });
            }
        }

        private void InitializeTypeDefinitions()
        {
            if (this.metadata.Types == null)
            {
                this.InitializeNestedTypes();
                this.InitializeFields();
                this.InitializeMethods();
                int num = this.MoveTo(Table.TypeDef);
                TypeDefinition[] definitionArray = this.metadata.Types = new TypeDefinition[num];
                for (uint i = 0; i < num; i++)
                {
                    if (definitionArray[i] == null)
                    {
                        definitionArray[i] = this.ReadType(i + 1);
                    }
                }
            }
        }

        private void InitializeTypeLayouts()
        {
            if (this.metadata.ClassLayouts == null)
            {
                int capacity = this.MoveTo(Table.ClassLayout);
                Dictionary<uint, Row<ushort, uint>> dictionary = this.metadata.ClassLayouts = new Dictionary<uint, Row<ushort, uint>>(capacity);
                for (uint i = 0; i < capacity; i++)
                {
                    ushort num3 = base.ReadUInt16();
                    uint num4 = base.ReadUInt32();
                    uint key = this.ReadTableIndex(Table.TypeDef);
                    dictionary.Add(key, new Row<ushort, uint>(num3, num4));
                }
            }
        }

        private void InitializeTypeReferences()
        {
            if (this.metadata.TypeReferences == null)
            {
                this.metadata.TypeReferences = new TypeReference[this.image.GetTableLength(Table.TypeRef)];
            }
        }

        private static bool IsDeleted(IMemberDefinition member) => 
            (member.IsSpecialName && (member.Name == "_Deleted"));

        private static bool IsNested(TypeAttributes attributes)
        {
            switch ((attributes & (TypeAttributes.AnsiClass | TypeAttributes.NestedAssembly | TypeAttributes.NestedPublic)))
            {
                case (TypeAttributes.AnsiClass | TypeAttributes.NestedPublic):
                case (TypeAttributes.AnsiClass | TypeAttributes.NestedPrivate):
                case (TypeAttributes.AnsiClass | TypeAttributes.NestedFamily):
                case (TypeAttributes.AnsiClass | TypeAttributes.NestedAssembly):
                case (TypeAttributes.AnsiClass | TypeAttributes.NestedFamANDAssem):
                case (TypeAttributes.AnsiClass | TypeAttributes.NestedAssembly | TypeAttributes.NestedPublic):
                    return true;
            }
            return false;
        }

        private FieldDefinition LookupField(uint rid)
        {
            TypeDefinition fieldDeclaringType = this.metadata.GetFieldDeclaringType(rid);
            if (fieldDeclaringType == null)
            {
                return null;
            }
            InitializeCollection(fieldDeclaringType.Fields);
            return this.metadata.GetFieldDefinition(rid);
        }

        private MethodDefinition LookupMethod(uint rid)
        {
            TypeDefinition methodDeclaringType = this.metadata.GetMethodDeclaringType(rid);
            if (methodDeclaringType == null)
            {
                return null;
            }
            InitializeCollection(methodDeclaringType.Methods);
            return this.metadata.GetMethodDefinition(rid);
        }

        public IMetadataTokenProvider LookupToken(MetadataToken token)
        {
            IMetadataTokenProvider typeSpecification;
            uint rID = token.RID;
            if (rID == 0)
            {
                return null;
            }
            int position = base.position;
            IGenericContext context = this.context;
            TokenType tokenType = token.TokenType;
            if (tokenType > TokenType.Field)
            {
                if (tokenType > TokenType.MemberRef)
                {
                    if (tokenType == TokenType.TypeSpec)
                    {
                        typeSpecification = this.GetTypeSpecification(rID);
                    }
                    else if (tokenType == TokenType.MethodSpec)
                    {
                        typeSpecification = this.GetMethodSpecification(rID);
                    }
                    else
                    {
                        goto TR_0001;
                    }
                }
                else if (tokenType == TokenType.Method)
                {
                    typeSpecification = this.GetMethodDefinition(rID);
                }
                else if (tokenType == TokenType.MemberRef)
                {
                    typeSpecification = this.GetMemberReference(rID);
                }
                else
                {
                    goto TR_0001;
                }
                goto TR_0002;
            }
            else
            {
                if (tokenType == TokenType.TypeRef)
                {
                    typeSpecification = this.GetTypeReference(rID);
                }
                else if (tokenType == TokenType.TypeDef)
                {
                    typeSpecification = this.GetTypeDefinition(rID);
                }
                else if (tokenType == TokenType.Field)
                {
                    typeSpecification = this.GetFieldDefinition(rID);
                }
                else
                {
                    goto TR_0001;
                }
                goto TR_0002;
            }
        TR_0001:
            return null;
        TR_0002:
            base.position = position;
            this.context = context;
            return typeSpecification;
        }

        private int MoveTo(Table table)
        {
            TableInformation information = this.image.TableHeap[table];
            if (information.Length != 0)
            {
                this.Position = information.Offset;
            }
            return (int) information.Length;
        }

        private bool MoveTo(Table table, uint row)
        {
            TableInformation information = this.image.TableHeap[table];
            uint length = information.Length;
            if ((length == 0) || (row > length))
            {
                return false;
            }
            this.Position = information.Offset + (information.RowSize * (row - 1));
            return true;
        }

        public ModuleDefinition Populate(ModuleDefinition module)
        {
            if (this.MoveTo(Table.Module) != 0)
            {
                base.Advance(2);
                module.Name = this.ReadString();
                module.Mvid = this.image.GuidHeap.Read(this.ReadByIndexSize(this.image.GuidHeap.IndexSize));
            }
            return module;
        }

        private void PopulateNameAndCulture(AssemblyNameReference name)
        {
            name.Name = this.ReadString();
            name.Culture = this.ReadString();
        }

        private void PopulateVersionAndFlags(AssemblyNameReference name)
        {
            name.Version = new Version(base.ReadUInt16(), base.ReadUInt16(), base.ReadUInt16(), base.ReadUInt16());
            name.Attributes = (AssemblyAttributes) base.ReadUInt32();
        }

        private static int RangesSize(Range[] ranges)
        {
            uint num = 0;
            for (int i = 0; i < ranges.Length; i++)
            {
                num += ranges[i].Length;
            }
            return (int) num;
        }

        public MethodSemanticsAttributes ReadAllSemantics(MethodDefinition method)
        {
            this.ReadAllSemantics(method.DeclaringType);
            return method.SemanticsAttributes;
        }

        private void ReadAllSemantics(TypeDefinition type)
        {
            Collection<MethodDefinition> methods = type.Methods;
            for (int i = 0; i < methods.Count; i++)
            {
                MethodDefinition method = methods[i];
                if (!method.sem_attrs_ready)
                {
                    method.sem_attrs = this.ReadMethodSemantics(method);
                    method.sem_attrs_ready = true;
                }
            }
        }

        public AssemblyNameDefinition ReadAssemblyNameDefinition()
        {
            if (this.MoveTo(Table.Assembly) == 0)
            {
                return null;
            }
            AssemblyNameDefinition name = new AssemblyNameDefinition {
                HashAlgorithm = (AssemblyHashAlgorithm) base.ReadUInt32()
            };
            this.PopulateVersionAndFlags(name);
            name.PublicKey = this.ReadBlob();
            this.PopulateNameAndCulture(name);
            return name;
        }

        public Collection<AssemblyNameReference> ReadAssemblyReferences()
        {
            this.InitializeAssemblyReferences();
            return new Collection<AssemblyNameReference>(this.metadata.AssemblyReferences);
        }

        private byte[] ReadBlob()
        {
            BlobHeap blobHeap = this.image.BlobHeap;
            if (blobHeap != null)
            {
                return blobHeap.Read(this.ReadBlobIndex());
            }
            base.position += 2;
            return Empty<byte>.Array;
        }

        private byte[] ReadBlob(uint signature)
        {
            BlobHeap blobHeap = this.image.BlobHeap;
            return ((blobHeap != null) ? blobHeap.Read(signature) : Empty<byte>.Array);
        }

        private uint ReadBlobIndex()
        {
            BlobHeap blobHeap = this.image.BlobHeap;
            return this.ReadByIndexSize((blobHeap != null) ? blobHeap.IndexSize : 2);
        }

        private uint ReadByIndexSize(int size) => 
            ((size != 4) ? base.ReadUInt16() : base.ReadUInt32());

        public CallSite ReadCallSite(MetadataToken token)
        {
            if (!this.MoveTo(Table.StandAloneSig, token.RID))
            {
                return null;
            }
            uint signature = this.ReadBlobIndex();
            CallSite method = new CallSite();
            this.ReadMethodSignature(signature, method);
            method.MetadataToken = token;
            return method;
        }

        public object ReadConstant(IConstantProvider owner)
        {
            Row<ElementType, uint> row;
            this.InitializeConstants();
            if (!this.metadata.Constants.TryGetValue(owner.MetadataToken, out row))
            {
                return Mixin.NoValue;
            }
            this.metadata.Constants.Remove(owner.MetadataToken);
            ElementType type = row.Col1;
            return ((type == ElementType.String) ? ReadConstantString(this.ReadBlob(row.Col2)) : (((type == ElementType.Class) || (type == ElementType.Object)) ? null : this.ReadConstantPrimitive(row.Col1, row.Col2)));
        }

        private object ReadConstantPrimitive(ElementType type, uint signature) => 
            this.ReadSignature(signature).ReadConstantSignature(type);

        private static string ReadConstantString(byte[] blob)
        {
            int length = blob.Length;
            if ((length & 1) == 1)
            {
                length--;
            }
            return Encoding.Unicode.GetString(blob, 0, length);
        }

        public byte[] ReadCustomAttributeBlob(uint signature) => 
            this.ReadBlob(signature);

        private void ReadCustomAttributeRange(Range range, Collection<CustomAttribute> custom_attributes)
        {
            if (this.MoveTo(Table.CustomAttribute, range.Start))
            {
                for (int i = 0; i < range.Length; i++)
                {
                    this.ReadMetadataToken(CodedIndex.HasCustomAttribute);
                    MethodReference token = (MethodReference) this.LookupToken(this.ReadMetadataToken(CodedIndex.CustomAttributeType));
                    uint signature = this.ReadBlobIndex();
                    custom_attributes.Add(new CustomAttribute(signature, token));
                }
            }
        }

        public Collection<CustomAttribute> ReadCustomAttributes(ICustomAttributeProvider owner)
        {
            Range[] rangeArray;
            this.InitializeCustomAttributes();
            if (!this.metadata.TryGetCustomAttributeRanges(owner, out rangeArray))
            {
                return new Collection<CustomAttribute>();
            }
            Collection<CustomAttribute> collection = new Collection<CustomAttribute>(RangesSize(rangeArray));
            for (int i = 0; i < rangeArray.Length; i++)
            {
                this.ReadCustomAttributeRange(rangeArray[i], collection);
            }
            this.metadata.RemoveCustomAttributeRange(owner);
            return collection;
        }

        public void ReadCustomAttributeSignature(CustomAttribute attribute)
        {
            SignatureReader reader = this.ReadSignature(attribute.signature);
            if (reader.CanReadMore())
            {
                if (reader.ReadUInt16() != 1)
                {
                    throw new InvalidOperationException();
                }
                MethodReference constructor = attribute.Constructor;
                if (constructor.HasParameters)
                {
                    reader.ReadCustomAttributeConstructorArguments(attribute, constructor.Parameters);
                }
                if (reader.CanReadMore())
                {
                    ushort count = reader.ReadUInt16();
                    if (count != 0)
                    {
                        reader.ReadCustomAttributeNamedArguments(count, ref attribute.fields, ref attribute.properties);
                    }
                }
            }
        }

        public MethodDefinition ReadEntryPoint()
        {
            if (this.module.Image.EntryPointToken == 0)
            {
                return null;
            }
            MetadataToken token = new MetadataToken(this.module.Image.EntryPointToken);
            return this.GetMethodDefinition(token.RID);
        }

        private void ReadEvent(uint event_rid, Collection<EventDefinition> events)
        {
            EventDefinition member = new EventDefinition(this.ReadString(), (EventAttributes) base.ReadUInt16(), this.GetTypeDefOrRef(this.ReadMetadataToken(CodedIndex.TypeDefOrRef))) {
                token = new MetadataToken(TokenType.Event, event_rid)
            };
            if (!IsDeleted(member))
            {
                events.Add(member);
            }
        }

        public Collection<EventDefinition> ReadEvents(TypeDefinition type)
        {
            Range range;
            this.InitializeEvents();
            if (!this.metadata.TryGetEventsRange(type, out range))
            {
                return new MemberDefinitionCollection<EventDefinition>(type);
            }
            MemberDefinitionCollection<EventDefinition> members = new MemberDefinitionCollection<EventDefinition>(type, (int) range.Length);
            this.metadata.RemoveEventsRange(type);
            if (range.Length != 0)
            {
                this.context = type;
                if (this.MoveTo(Table.EventPtr, range.Start))
                {
                    this.ReadPointers<EventDefinition>(Table.EventPtr, Table.Event, range, members, new Action<uint, Collection<EventDefinition>>(this.ReadEvent));
                }
                else
                {
                    if (!this.MoveTo(Table.Event, range.Start))
                    {
                        return members;
                    }
                    for (uint i = 0; i < range.Length; i++)
                    {
                        this.ReadEvent(range.Start + i, members);
                    }
                }
            }
            return members;
        }

        private Range ReadEventsRange(uint rid) => 
            this.ReadListRange(rid, Table.EventMap, Table.Event);

        public Collection<ExportedType> ReadExportedTypes()
        {
            int capacity = this.MoveTo(Table.ExportedType);
            if (capacity == 0)
            {
                return new Collection<ExportedType>();
            }
            Collection<ExportedType> collection = new Collection<ExportedType>(capacity);
            for (int i = 1; i <= capacity; i++)
            {
                TypeAttributes attributes = (TypeAttributes) base.ReadUInt32();
                uint num3 = base.ReadUInt32();
                string name = this.ReadString();
                string str2 = this.ReadString();
                MetadataToken token = this.ReadMetadataToken(CodedIndex.Implementation);
                ExportedType type = null;
                IMetadataScope exportedTypeScope = null;
                TokenType tokenType = token.TokenType;
                if ((tokenType == TokenType.AssemblyRef) || (tokenType == TokenType.File))
                {
                    exportedTypeScope = this.GetExportedTypeScope(token);
                }
                else if (tokenType == TokenType.ExportedType)
                {
                    type = collection[((int) token.RID) - 1];
                }
                ExportedType item = new ExportedType(str2, name, this.module, exportedTypeScope) {
                    Attributes = attributes,
                    Identifier = (int) num3,
                    DeclaringType = type
                };
                item.token = new MetadataToken(TokenType.ExportedType, i);
                collection.Add(item);
            }
            return collection;
        }

        private void ReadField(uint field_rid, Collection<FieldDefinition> fields)
        {
            uint signature = this.ReadBlobIndex();
            FieldDefinition field = new FieldDefinition(this.ReadString(), (FieldAttributes) base.ReadUInt16(), this.ReadFieldType(signature)) {
                token = new MetadataToken(TokenType.Field, field_rid)
            };
            this.metadata.AddFieldDefinition(field);
            if (!IsDeleted(field))
            {
                fields.Add(field);
            }
        }

        public int ReadFieldLayout(FieldDefinition field)
        {
            uint num2;
            this.InitializeFieldLayouts();
            uint rID = field.token.RID;
            if (!this.metadata.FieldLayouts.TryGetValue(rID, out num2))
            {
                return -1;
            }
            this.metadata.FieldLayouts.Remove(rID);
            return (int) num2;
        }

        public int ReadFieldRVA(FieldDefinition field)
        {
            uint num2;
            this.InitializeFieldRVAs();
            uint rID = field.token.RID;
            if (!this.metadata.FieldRVAs.TryGetValue(rID, out num2))
            {
                return 0;
            }
            int fieldTypeSize = GetFieldTypeSize(field.FieldType);
            if ((fieldTypeSize == 0) || (num2 == 0))
            {
                return 0;
            }
            this.metadata.FieldRVAs.Remove(rID);
            field.InitialValue = this.GetFieldInitializeValue(fieldTypeSize, num2);
            return (int) num2;
        }

        public Collection<FieldDefinition> ReadFields(TypeDefinition type)
        {
            Range range = type.fields_range;
            if (range.Length == 0)
            {
                return new MemberDefinitionCollection<FieldDefinition>(type);
            }
            MemberDefinitionCollection<FieldDefinition> members = new MemberDefinitionCollection<FieldDefinition>(type, (int) range.Length);
            this.context = type;
            if (this.MoveTo(Table.FieldPtr, range.Start))
            {
                this.ReadPointers<FieldDefinition>(Table.FieldPtr, Table.Field, range, members, new Action<uint, Collection<FieldDefinition>>(this.ReadField));
            }
            else
            {
                if (!this.MoveTo(Table.Field, range.Start))
                {
                    return members;
                }
                for (uint i = 0; i < range.Length; i++)
                {
                    this.ReadField(range.Start + i, members);
                }
            }
            return members;
        }

        private Range ReadFieldsRange(uint type_index) => 
            this.ReadListRange(type_index, Table.TypeDef, Table.Field);

        private TypeReference ReadFieldType(uint signature)
        {
            SignatureReader reader = this.ReadSignature(signature);
            if (reader.ReadByte() != 6)
            {
                throw new NotSupportedException();
            }
            return reader.ReadTypeSignature();
        }

        private Row<FileAttributes, string, uint> ReadFileRecord(uint rid)
        {
            int position = base.position;
            if (!this.MoveTo(Table.File, rid))
            {
                throw new ArgumentException();
            }
            Row<FileAttributes, string, uint> row = new Row<FileAttributes, string, uint>((FileAttributes) base.ReadUInt32(), this.ReadString(), this.ReadBlobIndex());
            base.position = position;
            return row;
        }

        public Collection<TypeReference> ReadGenericConstraints(GenericParameter generic_parameter)
        {
            MetadataToken[] tokenArray;
            this.InitializeGenericConstraints();
            if (!this.metadata.TryGetGenericConstraintMapping(generic_parameter, out tokenArray))
            {
                return new Collection<TypeReference>();
            }
            Collection<TypeReference> collection = new Collection<TypeReference>(tokenArray.Length);
            this.context = (IGenericContext) generic_parameter.Owner;
            for (int i = 0; i < tokenArray.Length; i++)
            {
                collection.Add(this.GetTypeDefOrRef(tokenArray[i]));
            }
            this.metadata.RemoveGenericConstraintMapping(generic_parameter);
            return collection;
        }

        public Collection<GenericParameter> ReadGenericParameters(IGenericParameterProvider provider)
        {
            Range[] rangeArray;
            this.InitializeGenericParameters();
            if (!this.metadata.TryGetGenericParameterRanges(provider, out rangeArray))
            {
                return new GenericParameterCollection(provider);
            }
            this.metadata.RemoveGenericParameterRange(provider);
            GenericParameterCollection parameters = new GenericParameterCollection(provider, RangesSize(rangeArray));
            for (int i = 0; i < rangeArray.Length; i++)
            {
                this.ReadGenericParametersRange(rangeArray[i], provider, parameters);
            }
            return parameters;
        }

        private void ReadGenericParametersRange(Range range, IGenericParameterProvider provider, GenericParameterCollection generic_parameters)
        {
            if (this.MoveTo(Table.GenericParam, range.Start))
            {
                for (uint i = 0; i < range.Length; i++)
                {
                    base.ReadUInt16();
                    GenericParameterAttributes attributes = (GenericParameterAttributes) base.ReadUInt16();
                    this.ReadMetadataToken(CodedIndex.TypeOrMethodDef);
                    string name = this.ReadString();
                    GenericParameter item = new GenericParameter(name, provider) {
                        token = new MetadataToken(TokenType.GenericParam, range.Start + i),
                        Attributes = attributes
                    };
                    generic_parameters.Add(item);
                }
            }
        }

        public Collection<TypeReference> ReadInterfaces(TypeDefinition type)
        {
            MetadataToken[] tokenArray;
            this.InitializeInterfaces();
            if (!this.metadata.TryGetInterfaceMapping(type, out tokenArray))
            {
                return new Collection<TypeReference>();
            }
            Collection<TypeReference> collection = new Collection<TypeReference>(tokenArray.Length);
            this.context = type;
            for (int i = 0; i < tokenArray.Length; i++)
            {
                collection.Add(this.GetTypeDefOrRef(tokenArray[i]));
            }
            this.metadata.RemoveInterfaceMapping(type);
            return collection;
        }

        private Range ReadListRange(uint current_index, Table current, Table target)
        {
            uint num;
            Range range = new Range {
                Start = this.ReadTableIndex(target)
            };
            TableInformation information = this.image.TableHeap[current];
            if (current_index == information.Length)
            {
                num = this.image.TableHeap[target].Length + 1;
            }
            else
            {
                uint position = this.Position;
                this.Position += information.RowSize - ((uint) this.image.GetTableIndexSize(target));
                num = this.ReadTableIndex(target);
                this.Position = position;
            }
            range.Length = num - range.Start;
            return range;
        }

        public MarshalInfo ReadMarshalInfo(IMarshalInfoProvider owner)
        {
            uint num;
            this.InitializeMarshalInfos();
            if (!this.metadata.FieldMarshals.TryGetValue(owner.MetadataToken, out num))
            {
                return null;
            }
            SignatureReader reader = this.ReadSignature(num);
            this.metadata.FieldMarshals.Remove(owner.MetadataToken);
            return reader.ReadMarshalInfo();
        }

        private MemberReference ReadMemberReference(uint rid)
        {
            MemberReference reference;
            if (!this.MoveTo(Table.MemberRef, rid))
            {
                return null;
            }
            MetadataToken token = this.ReadMetadataToken(CodedIndex.MemberRefParent);
            string name = this.ReadString();
            uint signature = this.ReadBlobIndex();
            TokenType tokenType = token.TokenType;
            if (tokenType > TokenType.TypeDef)
            {
                if (tokenType == TokenType.Method)
                {
                    reference = this.ReadMethodMemberReference(token, name, signature);
                    goto TR_0002;
                }
                else if (tokenType != TokenType.TypeSpec)
                {
                    goto TR_0001;
                }
            }
            else if ((tokenType != TokenType.TypeRef) && (tokenType != TokenType.TypeDef))
            {
                goto TR_0001;
            }
            reference = this.ReadTypeMemberReference(token, name, signature);
            goto TR_0002;
        TR_0001:
            throw new NotSupportedException();
        TR_0002:
            reference.token = new MetadataToken(TokenType.MemberRef, rid);
            return reference;
        }

        private MemberReference ReadMemberReferenceSignature(uint signature, TypeReference declaring_type)
        {
            SignatureReader reader = this.ReadSignature(signature);
            if (reader.buffer[reader.position] != 6)
            {
                MethodReference method = new MethodReference {
                    DeclaringType = declaring_type
                };
                reader.ReadMethodSignature(method);
                return method;
            }
            reader.position++;
            return new FieldReference { 
                DeclaringType = declaring_type,
                FieldType = reader.ReadTypeSignature()
            };
        }

        private MetadataToken ReadMetadataToken(CodedIndex index) => 
            index.GetMetadataToken(this.ReadByIndexSize(this.GetCodedIndexSize(index)));

        private void ReadMethod(uint method_rid, Collection<MethodDefinition> methods)
        {
            MethodDefinition member = new MethodDefinition {
                rva = base.ReadUInt32(),
                ImplAttributes = (MethodImplAttributes) base.ReadUInt16(),
                Attributes = (MethodAttributes) base.ReadUInt16(),
                Name = this.ReadString(),
                token = new MetadataToken(TokenType.Method, method_rid)
            };
            if (!IsDeleted(member))
            {
                methods.Add(member);
                uint signature = this.ReadBlobIndex();
                Range range = this.ReadParametersRange(method_rid);
                this.context = member;
                this.ReadMethodSignature(signature, member);
                this.metadata.AddMethodDefinition(member);
                if (range.Length != 0)
                {
                    int position = base.position;
                    this.ReadParameters(member, range);
                    base.position = position;
                }
            }
        }

        public MethodBody ReadMethodBody(MethodDefinition method) => 
            this.code.ReadMethodBody(method);

        private MemberReference ReadMethodMemberReference(MetadataToken token, string name, uint signature)
        {
            MethodDefinition methodDefinition = this.GetMethodDefinition(token.RID);
            this.context = methodDefinition;
            MemberReference reference = this.ReadMemberReferenceSignature(signature, methodDefinition.DeclaringType);
            reference.Name = name;
            return reference;
        }

        public EventDefinition ReadMethods(EventDefinition @event)
        {
            this.ReadAllSemantics(@event.DeclaringType);
            return @event;
        }

        public PropertyDefinition ReadMethods(PropertyDefinition property)
        {
            this.ReadAllSemantics(property.DeclaringType);
            return property;
        }

        public Collection<MethodDefinition> ReadMethods(TypeDefinition type)
        {
            Range range = type.methods_range;
            if (range.Length == 0)
            {
                return new MemberDefinitionCollection<MethodDefinition>(type);
            }
            MemberDefinitionCollection<MethodDefinition> members = new MemberDefinitionCollection<MethodDefinition>(type, (int) range.Length);
            if (this.MoveTo(Table.MethodPtr, range.Start))
            {
                this.ReadPointers<MethodDefinition>(Table.MethodPtr, Table.Method, range, members, new Action<uint, Collection<MethodDefinition>>(this.ReadMethod));
            }
            else
            {
                if (!this.MoveTo(Table.Method, range.Start))
                {
                    return members;
                }
                for (uint i = 0; i < range.Length; i++)
                {
                    this.ReadMethod(range.Start + i, members);
                }
            }
            return members;
        }

        private MethodSemanticsAttributes ReadMethodSemantics(MethodDefinition method)
        {
            Row<MethodSemanticsAttributes, MetadataToken> row;
            this.InitializeMethodSemantics();
            if (!this.metadata.Semantics.TryGetValue(method.token.RID, out row))
            {
                return MethodSemanticsAttributes.None;
            }
            TypeDefinition declaringType = method.DeclaringType;
            MethodSemanticsAttributes attributes = row.Col1;
            if (attributes > MethodSemanticsAttributes.AddOn)
            {
                if (attributes == (MethodSemanticsAttributes.None | MethodSemanticsAttributes.RemoveOn))
                {
                    GetEvent(declaringType, row.Col2).remove_method = method;
                }
                else if (attributes == MethodSemanticsAttributes.Fire)
                {
                    GetEvent(declaringType, row.Col2).invoke_method = method;
                }
                else
                {
                    goto TR_0001;
                }
                goto TR_0002;
            }
            else
            {
                switch (attributes)
                {
                    case (MethodSemanticsAttributes.None | MethodSemanticsAttributes.Setter):
                        GetProperty(declaringType, row.Col2).set_method = method;
                        break;

                    case MethodSemanticsAttributes.Getter:
                        GetProperty(declaringType, row.Col2).get_method = method;
                        break;

                    case (MethodSemanticsAttributes.Getter | MethodSemanticsAttributes.Setter):
                        goto TR_0001;

                    case (MethodSemanticsAttributes.None | MethodSemanticsAttributes.Other):
                    {
                        TokenType tokenType = row.Col2.TokenType;
                        if (tokenType == TokenType.Event)
                        {
                            EventDefinition definition2 = GetEvent(declaringType, row.Col2);
                            if (definition2.other_methods == null)
                            {
                                definition2.other_methods = new Collection<MethodDefinition>();
                            }
                            definition2.other_methods.Add(method);
                        }
                        else
                        {
                            if (tokenType != TokenType.Property)
                            {
                                throw new NotSupportedException();
                            }
                            PropertyDefinition property = GetProperty(declaringType, row.Col2);
                            if (property.other_methods == null)
                            {
                                property.other_methods = new Collection<MethodDefinition>();
                            }
                            property.other_methods.Add(method);
                        }
                        break;
                    }
                    default:
                        if (attributes == MethodSemanticsAttributes.AddOn)
                        {
                            GetEvent(declaringType, row.Col2).add_method = method;
                        }
                        else
                        {
                            goto TR_0001;
                        }
                        break;
                }
                goto TR_0002;
            }
        TR_0001:
            throw new NotSupportedException();
        TR_0002:
            this.metadata.Semantics.Remove(method.token.RID);
            return row.Col1;
        }

        private void ReadMethodSignature(uint signature, IMethodSignature method)
        {
            this.ReadSignature(signature).ReadMethodSignature(method);
        }

        private MethodSpecification ReadMethodSpecSignature(uint signature, MethodReference method)
        {
            SignatureReader reader = this.ReadSignature(signature);
            if (reader.ReadByte() != 10)
            {
                throw new NotSupportedException();
            }
            GenericInstanceMethod instance = new GenericInstanceMethod(method);
            reader.ReadGenericInstanceSignature(method, instance);
            return instance;
        }

        private Range ReadMethodsRange(uint type_index) => 
            this.ReadListRange(type_index, Table.TypeDef, Table.Method);

        public Collection<ModuleReference> ReadModuleReferences()
        {
            this.InitializeModuleReferences();
            return new Collection<ModuleReference>(this.metadata.ModuleReferences);
        }

        public Collection<ModuleDefinition> ReadModules()
        {
            Collection<ModuleDefinition> collection = new Collection<ModuleDefinition>(1) {
                this.module
            };
            int num = this.MoveTo(Table.File);
            for (uint i = 1; i <= num; i++)
            {
                FileAttributes attributes = (FileAttributes) base.ReadUInt32();
                string name = this.ReadString();
                this.ReadBlobIndex();
                if (attributes == FileAttributes.ContainsMetaData)
                {
                    ReaderParameters parameters = new ReaderParameters {
                        ReadingMode = this.module.ReadingMode,
                        SymbolReaderProvider = this.module.SymbolReaderProvider,
                        AssemblyResolver = this.module.AssemblyResolver
                    };
                    collection.Add(ModuleDefinition.ReadModule(this.GetModuleFileName(name), parameters));
                }
            }
            return collection;
        }

        public Collection<TypeDefinition> ReadNestedTypes(TypeDefinition type)
        {
            uint[] numArray;
            this.InitializeNestedTypes();
            if (!this.metadata.TryGetNestedTypeMapping(type, out numArray))
            {
                return new MemberDefinitionCollection<TypeDefinition>(type);
            }
            MemberDefinitionCollection<TypeDefinition> definitions = new MemberDefinitionCollection<TypeDefinition>(type, numArray.Length);
            for (int i = 0; i < numArray.Length; i++)
            {
                TypeDefinition typeDefinition = this.GetTypeDefinition(numArray[i]);
                if (typeDefinition != null)
                {
                    definitions.Add(typeDefinition);
                }
            }
            this.metadata.RemoveNestedTypeMapping(type);
            return definitions;
        }

        public Collection<MethodReference> ReadOverrides(MethodDefinition method)
        {
            MetadataToken[] tokenArray;
            this.InitializeOverrides();
            if (!this.metadata.TryGetOverrideMapping(method, out tokenArray))
            {
                return new Collection<MethodReference>();
            }
            Collection<MethodReference> collection = new Collection<MethodReference>(tokenArray.Length);
            this.context = method;
            for (int i = 0; i < tokenArray.Length; i++)
            {
                collection.Add((MethodReference) this.LookupToken(tokenArray[i]));
            }
            this.metadata.RemoveOverrideMapping(method);
            return collection;
        }

        private void ReadParameter(uint param_rid, MethodDefinition method)
        {
            ParameterAttributes attributes = (ParameterAttributes) base.ReadUInt16();
            ushort num = base.ReadUInt16();
            string str = this.ReadString();
            ParameterDefinition definition = (num == 0) ? method.MethodReturnType.Parameter : method.Parameters[num - 1];
            definition.token = new MetadataToken(TokenType.Param, param_rid);
            definition.Name = str;
            definition.Attributes = attributes;
        }

        private void ReadParameterPointers(MethodDefinition method, Range range)
        {
            for (uint i = 0; i < range.Length; i++)
            {
                this.MoveTo(Table.ParamPtr, range.Start + i);
                uint row = this.ReadTableIndex(Table.Param);
                this.MoveTo(Table.Param, row);
                this.ReadParameter(row, method);
            }
        }

        private void ReadParameters(MethodDefinition method, Range param_range)
        {
            if (this.MoveTo(Table.ParamPtr, param_range.Start))
            {
                this.ReadParameterPointers(method, param_range);
            }
            else if (this.MoveTo(Table.Param, param_range.Start))
            {
                for (uint i = 0; i < param_range.Length; i++)
                {
                    this.ReadParameter(param_range.Start + i, method);
                }
            }
        }

        private Range ReadParametersRange(uint method_rid) => 
            this.ReadListRange(method_rid, Table.Method, Table.Param);

        public PInvokeInfo ReadPInvokeInfo(MethodDefinition method)
        {
            Row<PInvokeAttributes, uint, uint> row;
            this.InitializePInvokes();
            uint rID = method.token.RID;
            if (!this.metadata.PInvokes.TryGetValue(rID, out row))
            {
                return null;
            }
            this.metadata.PInvokes.Remove(rID);
            return new PInvokeInfo(row.Col1, this.image.StringHeap.Read(row.Col2), this.module.ModuleReferences[row.Col3 - 1]);
        }

        private void ReadPointers<TMember>(Table ptr, Table table, Range range, Collection<TMember> members, Action<uint, Collection<TMember>> reader) where TMember: IMemberDefinition
        {
            for (uint i = 0; i < range.Length; i++)
            {
                this.MoveTo(ptr, range.Start + i);
                uint row = this.ReadTableIndex(table);
                this.MoveTo(table, row);
                reader(row, members);
            }
        }

        public Collection<PropertyDefinition> ReadProperties(TypeDefinition type)
        {
            Range range;
            this.InitializeProperties();
            if (!this.metadata.TryGetPropertiesRange(type, out range))
            {
                return new MemberDefinitionCollection<PropertyDefinition>(type);
            }
            this.metadata.RemovePropertiesRange(type);
            MemberDefinitionCollection<PropertyDefinition> members = new MemberDefinitionCollection<PropertyDefinition>(type, (int) range.Length);
            if (range.Length != 0)
            {
                this.context = type;
                if (this.MoveTo(Table.PropertyPtr, range.Start))
                {
                    this.ReadPointers<PropertyDefinition>(Table.PropertyPtr, Table.Property, range, members, new Action<uint, Collection<PropertyDefinition>>(this.ReadProperty));
                }
                else
                {
                    if (!this.MoveTo(Table.Property, range.Start))
                    {
                        return members;
                    }
                    for (uint i = 0; i < range.Length; i++)
                    {
                        this.ReadProperty(range.Start + i, members);
                    }
                }
            }
            return members;
        }

        private Range ReadPropertiesRange(uint rid) => 
            this.ReadListRange(rid, Table.PropertyMap, Table.Property);

        private void ReadProperty(uint property_rid, Collection<PropertyDefinition> properties)
        {
            PropertyAttributes attributes = (PropertyAttributes) base.ReadUInt16();
            string name = this.ReadString();
            uint signature = this.ReadBlobIndex();
            SignatureReader reader = this.ReadSignature(signature);
            byte num2 = reader.ReadByte();
            if ((num2 & 8) == 0)
            {
                throw new NotSupportedException();
            }
            reader.ReadCompressedUInt32();
            PropertyDefinition member = new PropertyDefinition(name, attributes, reader.ReadTypeSignature()) {
                HasThis = (num2 & 0x20) != 0,
                token = new MetadataToken(TokenType.Property, property_rid)
            };
            if (!IsDeleted(member))
            {
                properties.Add(member);
            }
        }

        public Collection<Resource> ReadResources()
        {
            int capacity = this.MoveTo(Table.ManifestResource);
            Collection<Resource> collection = new Collection<Resource>(capacity);
            for (int i = 1; i <= capacity; i++)
            {
                Resource resource;
                uint offset = base.ReadUInt32();
                ManifestResourceAttributes attributes = (ManifestResourceAttributes) base.ReadUInt32();
                string name = this.ReadString();
                MetadataToken scope = this.ReadMetadataToken(CodedIndex.Implementation);
                if (scope.RID == 0)
                {
                    resource = new EmbeddedResource(name, attributes, offset, this);
                }
                else if (scope.TokenType == TokenType.AssemblyRef)
                {
                    AssemblyLinkedResource resource2 = new AssemblyLinkedResource(name, attributes) {
                        Assembly = (AssemblyNameReference) this.GetTypeReferenceScope(scope)
                    };
                    resource = resource2;
                }
                else
                {
                    if (scope.TokenType != TokenType.File)
                    {
                        throw new NotSupportedException();
                    }
                    Row<FileAttributes, string, uint> row = this.ReadFileRecord(scope.RID);
                    LinkedResource resource3 = new LinkedResource(name, attributes) {
                        File = row.Col2,
                        hash = this.ReadBlob(row.Col3)
                    };
                    resource = resource3;
                }
                collection.Add(resource);
            }
            return collection;
        }

        public byte[] ReadSecurityDeclarationBlob(uint signature) => 
            this.ReadBlob(signature);

        private void ReadSecurityDeclarationRange(Range range, Collection<SecurityDeclaration> security_declarations)
        {
            if (this.MoveTo(Table.DeclSecurity, range.Start))
            {
                for (int i = 0; i < range.Length; i++)
                {
                    SecurityAction action = (SecurityAction) base.ReadUInt16();
                    this.ReadMetadataToken(CodedIndex.HasDeclSecurity);
                    uint signature = this.ReadBlobIndex();
                    security_declarations.Add(new SecurityDeclaration(action, signature, this.module));
                }
            }
        }

        public Collection<SecurityDeclaration> ReadSecurityDeclarations(ISecurityDeclarationProvider owner)
        {
            Range[] rangeArray;
            this.InitializeSecurityDeclarations();
            if (!this.metadata.TryGetSecurityDeclarationRanges(owner, out rangeArray))
            {
                return new Collection<SecurityDeclaration>();
            }
            Collection<SecurityDeclaration> collection = new Collection<SecurityDeclaration>(RangesSize(rangeArray));
            for (int i = 0; i < rangeArray.Length; i++)
            {
                this.ReadSecurityDeclarationRange(rangeArray[i], collection);
            }
            this.metadata.RemoveSecurityDeclarationRange(owner);
            return collection;
        }

        public void ReadSecurityDeclarationSignature(SecurityDeclaration declaration)
        {
            uint signature = declaration.signature;
            SignatureReader reader = this.ReadSignature(signature);
            if (reader.buffer[reader.position] != 0x2e)
            {
                this.ReadXmlSecurityDeclaration(signature, declaration);
            }
            else
            {
                reader.position++;
                uint num2 = reader.ReadCompressedUInt32();
                Collection<SecurityAttribute> collection = new Collection<SecurityAttribute>((int) num2);
                for (int i = 0; i < num2; i++)
                {
                    collection.Add(reader.ReadSecurityAttribute());
                }
                declaration.security_attributes = collection;
            }
        }

        private SignatureReader ReadSignature(uint signature) => 
            new SignatureReader(signature, this);

        private string ReadString() => 
            this.image.StringHeap.Read(this.ReadByIndexSize(this.image.StringHeap.IndexSize));

        private uint ReadStringIndex() => 
            this.ReadByIndexSize(this.image.StringHeap.IndexSize);

        private uint ReadTableIndex(Table table) => 
            this.ReadByIndexSize(this.image.GetTableIndexSize(table));

        private TypeDefinition ReadType(uint rid)
        {
            if (!this.MoveTo(Table.TypeDef, rid))
            {
                return null;
            }
            TypeAttributes attributes = (TypeAttributes) base.ReadUInt32();
            TypeDefinition type = new TypeDefinition(this.ReadString(), this.ReadString(), attributes) {
                token = new MetadataToken(TokenType.TypeDef, rid),
                scope = this.module,
                module = this.module
            };
            this.metadata.AddTypeDefinition(type);
            this.context = type;
            type.BaseType = this.GetTypeDefOrRef(this.ReadMetadataToken(CodedIndex.TypeDefOrRef));
            type.fields_range = this.ReadFieldsRange(rid);
            type.methods_range = this.ReadMethodsRange(rid);
            if (IsNested(attributes))
            {
                type.DeclaringType = this.GetNestedTypeDeclaringType(type);
            }
            return type;
        }

        private TypeDefinition ReadTypeDefinition(uint rid) => 
            (this.MoveTo(Table.TypeDef, rid) ? this.ReadType(rid) : null);

        public Row<short, int> ReadTypeLayout(TypeDefinition type)
        {
            Row<ushort, uint> row;
            this.InitializeTypeLayouts();
            uint rID = type.token.RID;
            if (!this.metadata.ClassLayouts.TryGetValue(rID, out row))
            {
                return new Row<short, int>(-1, -1);
            }
            type.PackingSize = (short) row.Col1;
            type.ClassSize = row.Col2;
            this.metadata.ClassLayouts.Remove(rID);
            return new Row<short, int>((short) row.Col1, row.Col2);
        }

        private MemberReference ReadTypeMemberReference(MetadataToken type, string name, uint signature)
        {
            TypeReference typeDefOrRef = this.GetTypeDefOrRef(type);
            if (!typeDefOrRef.IsArray)
            {
                this.context = typeDefOrRef;
            }
            MemberReference reference2 = this.ReadMemberReferenceSignature(signature, typeDefOrRef);
            reference2.Name = name;
            return reference2;
        }

        private TypeReference ReadTypeReference(uint rid)
        {
            IMetadataScope typeReferenceScope;
            if (!this.MoveTo(Table.TypeRef, rid))
            {
                return null;
            }
            TypeReference typeDefOrRef = null;
            MetadataToken token = this.ReadMetadataToken(CodedIndex.ResolutionScope);
            TypeReference type = new TypeReference(this.ReadString(), this.ReadString(), this.module, null) {
                token = new MetadataToken(TokenType.TypeRef, rid)
            };
            this.metadata.AddTypeReference(type);
            if (token.TokenType != TokenType.TypeRef)
            {
                typeReferenceScope = this.GetTypeReferenceScope(token);
            }
            else
            {
                typeDefOrRef = this.GetTypeDefOrRef(token);
                typeReferenceScope = (typeDefOrRef != null) ? typeDefOrRef.Scope : this.module;
            }
            type.scope = typeReferenceScope;
            type.DeclaringType = typeDefOrRef;
            MetadataSystem.TryProcessPrimitiveTypeReference(type);
            return type;
        }

        public TypeDefinitionCollection ReadTypes()
        {
            this.InitializeTypeDefinitions();
            TypeDefinition[] types = this.metadata.Types;
            int capacity = types.Length - this.metadata.NestedTypes.Count;
            TypeDefinitionCollection definitions = new TypeDefinitionCollection(this.module, capacity);
            for (int i = 0; i < types.Length; i++)
            {
                TypeDefinition item = types[i];
                if (!IsNested(item.Attributes))
                {
                    definitions.Add(item);
                }
            }
            if (this.image.HasTable(Table.MethodPtr) || this.image.HasTable(Table.FieldPtr))
            {
                this.CompleteTypes();
            }
            return definitions;
        }

        public VariableDefinitionCollection ReadVariables(MetadataToken local_var_token)
        {
            if (!this.MoveTo(Table.StandAloneSig, local_var_token.RID))
            {
                return null;
            }
            SignatureReader reader = this.ReadSignature(this.ReadBlobIndex());
            if (reader.ReadByte() != 7)
            {
                throw new NotSupportedException();
            }
            uint num = reader.ReadCompressedUInt32();
            if (num == 0)
            {
                return null;
            }
            VariableDefinitionCollection definitions = new VariableDefinitionCollection((int) num);
            for (int i = 0; i < num; i++)
            {
                definitions.Add(new VariableDefinition(reader.ReadTypeSignature()));
            }
            return definitions;
        }

        private void ReadXmlSecurityDeclaration(uint signature, SecurityDeclaration declaration)
        {
            byte[] bytes = this.ReadBlob(signature);
            Collection<SecurityAttribute> collection = new Collection<SecurityAttribute>(1);
            SecurityAttribute item = new SecurityAttribute(this.module.TypeSystem.LookupType("System.Security.Permissions", "PermissionSetAttribute")) {
                properties = new Collection<CustomAttributeNamedArgument>(1)
            };
            item.properties.Add(new CustomAttributeNamedArgument("XML", new CustomAttributeArgument(this.module.TypeSystem.String, Encoding.Unicode.GetString(bytes, 0, bytes.Length))));
            collection.Add(item);
            declaration.security_attributes = collection;
        }

        private uint Position
        {
            get => 
                ((uint) base.position);
            set => 
                (base.position = (int) value);
        }
    }
}

