namespace Mono.Cecil
{
    using Mono.Cecil.Cil;
    using Mono.Cecil.Metadata;
    using Mono.Cecil.PE;
    using Mono.Collections.Generic;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;

    internal sealed class MetadataBuilder
    {
        internal readonly ModuleDefinition module;
        internal readonly ISymbolWriterProvider symbol_writer_provider;
        internal readonly ISymbolWriter symbol_writer;
        internal readonly TextMap text_map;
        internal readonly string fq_name;
        private readonly Dictionary<Row<uint, uint, uint>, MetadataToken> type_ref_map;
        private readonly Dictionary<uint, MetadataToken> type_spec_map;
        private readonly Dictionary<Row<uint, uint, uint>, MetadataToken> member_ref_map;
        private readonly Dictionary<Row<uint, uint>, MetadataToken> method_spec_map;
        private readonly Collection<GenericParameter> generic_parameters;
        private readonly Dictionary<MetadataToken, MetadataToken> method_def_map;
        internal readonly CodeWriter code;
        internal readonly DataBuffer data;
        internal readonly ResourceBuffer resources;
        internal readonly StringHeapBuffer string_heap;
        internal readonly UserStringHeapBuffer user_string_heap;
        internal readonly BlobHeapBuffer blob_heap;
        internal readonly TableHeapBuffer table_heap;
        internal MetadataToken entry_point;
        private uint type_rid = 1;
        private uint field_rid = 1;
        private uint method_rid = 1;
        private uint param_rid = 1;
        private uint property_rid = 1;
        private uint event_rid = 1;
        private readonly TypeRefTable type_ref_table;
        private readonly TypeDefTable type_def_table;
        private readonly FieldTable field_table;
        private readonly MethodTable method_table;
        private readonly ParamTable param_table;
        private readonly InterfaceImplTable iface_impl_table;
        private readonly MemberRefTable member_ref_table;
        private readonly ConstantTable constant_table;
        private readonly CustomAttributeTable custom_attribute_table;
        private readonly DeclSecurityTable declsec_table;
        private readonly StandAloneSigTable standalone_sig_table;
        private readonly EventMapTable event_map_table;
        private readonly EventTable event_table;
        private readonly PropertyMapTable property_map_table;
        private readonly PropertyTable property_table;
        private readonly TypeSpecTable typespec_table;
        private readonly MethodSpecTable method_spec_table;
        internal readonly bool write_symbols;

        public MetadataBuilder(ModuleDefinition module, string fq_name, ISymbolWriterProvider symbol_writer_provider, ISymbolWriter symbol_writer)
        {
            this.module = module;
            this.text_map = this.CreateTextMap();
            this.fq_name = fq_name;
            this.symbol_writer_provider = symbol_writer_provider;
            this.symbol_writer = symbol_writer;
            this.write_symbols = !ReferenceEquals(symbol_writer, null);
            this.code = new CodeWriter(this);
            this.data = new DataBuffer();
            this.resources = new ResourceBuffer();
            this.string_heap = new StringHeapBuffer();
            this.user_string_heap = new UserStringHeapBuffer();
            this.blob_heap = new BlobHeapBuffer();
            this.table_heap = new TableHeapBuffer(module, this);
            this.type_ref_table = this.GetTable<TypeRefTable>(Table.TypeRef);
            this.type_def_table = this.GetTable<TypeDefTable>(Table.TypeDef);
            this.field_table = this.GetTable<FieldTable>(Table.Field);
            this.method_table = this.GetTable<MethodTable>(Table.Method);
            this.param_table = this.GetTable<ParamTable>(Table.Param);
            this.iface_impl_table = this.GetTable<InterfaceImplTable>(Table.InterfaceImpl);
            this.member_ref_table = this.GetTable<MemberRefTable>(Table.MemberRef);
            this.constant_table = this.GetTable<ConstantTable>(Table.Constant);
            this.custom_attribute_table = this.GetTable<CustomAttributeTable>(Table.CustomAttribute);
            this.declsec_table = this.GetTable<DeclSecurityTable>(Table.DeclSecurity);
            this.standalone_sig_table = this.GetTable<StandAloneSigTable>(Table.StandAloneSig);
            this.event_map_table = this.GetTable<EventMapTable>(Table.EventMap);
            this.event_table = this.GetTable<EventTable>(Table.Event);
            this.property_map_table = this.GetTable<PropertyMapTable>(Table.PropertyMap);
            this.property_table = this.GetTable<PropertyTable>(Table.Property);
            this.typespec_table = this.GetTable<TypeSpecTable>(Table.TypeSpec);
            this.method_spec_table = this.GetTable<MethodSpecTable>(Table.MethodSpec);
            RowEqualityComparer comparer = new RowEqualityComparer();
            this.type_ref_map = new Dictionary<Row<uint, uint, uint>, MetadataToken>(comparer);
            this.type_spec_map = new Dictionary<uint, MetadataToken>();
            this.member_ref_map = new Dictionary<Row<uint, uint, uint>, MetadataToken>(comparer);
            this.method_spec_map = new Dictionary<Row<uint, uint>, MetadataToken>(comparer);
            this.generic_parameters = new Collection<GenericParameter>();
            if (this.write_symbols)
            {
                this.method_def_map = new Dictionary<MetadataToken, MetadataToken>();
            }
        }

        private void AddAssemblyReferences()
        {
            Collection<AssemblyNameReference> assemblyReferences = this.module.AssemblyReferences;
            AssemblyRefTable table = this.GetTable<AssemblyRefTable>(Table.AssemblyRef);
            for (int i = 0; i < assemblyReferences.Count; i++)
            {
                AssemblyNameReference reference = assemblyReferences[i];
                byte[] blob = reference.PublicKey.IsNullOrEmpty<byte>() ? reference.PublicKeyToken : reference.PublicKey;
                Version version = reference.Version;
                reference.token = new MetadataToken(TokenType.AssemblyRef, table.AddRow(new Row<ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint, uint>((ushort) version.Major, (ushort) version.Minor, (ushort) version.Build, (ushort) version.Revision, reference.Attributes, this.GetBlobIndex(blob), this.GetStringIndex(reference.Name), this.GetStringIndex(reference.Culture), this.GetBlobIndex(reference.Hash))));
            }
        }

        private void AddConstant(IConstantProvider owner, TypeReference type)
        {
            object constant = owner.Constant;
            ElementType constantType = GetConstantType(type, constant);
            this.constant_table.AddRow(new Row<ElementType, uint, uint>(constantType, MakeCodedRID(owner.MetadataToken, CodedIndex.HasConstant), this.GetBlobIndex(this.GetConstantSignature(constantType, constant))));
        }

        private void AddConstraints(GenericParameter generic_parameter, GenericParamConstraintTable table)
        {
            Collection<TypeReference> constraints = generic_parameter.Constraints;
            uint rID = generic_parameter.token.RID;
            for (int i = 0; i < constraints.Count; i++)
            {
                table.AddRow(new Row<uint, uint>(rID, MakeCodedRID(this.GetTypeToken(constraints[i]), CodedIndex.TypeDefOrRef)));
            }
        }

        private void AddCustomAttributes(ICustomAttributeProvider owner)
        {
            Collection<CustomAttribute> customAttributes = owner.CustomAttributes;
            for (int i = 0; i < customAttributes.Count; i++)
            {
                CustomAttribute attribute = customAttributes[i];
                this.custom_attribute_table.AddRow(new Row<uint, uint, uint>(MakeCodedRID(owner, CodedIndex.HasCustomAttribute), MakeCodedRID(this.LookupToken(attribute.Constructor), CodedIndex.CustomAttributeType), this.GetBlobIndex(this.GetCustomAttributeSignature(attribute))));
            }
        }

        private uint AddEmbeddedResource(EmbeddedResource resource) => 
            this.resources.AddResource(resource.GetResourceData());

        private void AddEvent(EventDefinition @event)
        {
            uint num;
            this.event_table.AddRow(new Row<EventAttributes, uint, uint>(@event.Attributes, this.GetStringIndex(@event.Name), MakeCodedRID(this.GetTypeToken(@event.EventType), CodedIndex.TypeDefOrRef)));
            this.event_rid = (num = this.event_rid) + 1;
            @event.token = new MetadataToken(TokenType.Event, num);
            MethodDefinition addMethod = @event.AddMethod;
            if (addMethod != null)
            {
                this.AddSemantic(MethodSemanticsAttributes.AddOn, @event, addMethod);
            }
            addMethod = @event.InvokeMethod;
            if (addMethod != null)
            {
                this.AddSemantic(MethodSemanticsAttributes.Fire, @event, addMethod);
            }
            addMethod = @event.RemoveMethod;
            if (addMethod != null)
            {
                this.AddSemantic(MethodSemanticsAttributes.None | MethodSemanticsAttributes.RemoveOn, @event, addMethod);
            }
            if (@event.HasOtherMethods)
            {
                this.AddOtherSemantic(@event, @event.OtherMethods);
            }
            if (@event.HasCustomAttributes)
            {
                this.AddCustomAttributes(@event);
            }
        }

        private void AddEvents(TypeDefinition type)
        {
            Collection<EventDefinition> events = type.Events;
            this.event_map_table.AddRow(new Row<uint, uint>(type.token.RID, this.event_rid));
            for (int i = 0; i < events.Count; i++)
            {
                this.AddEvent(events[i]);
            }
        }

        private void AddExportedTypes()
        {
            Collection<ExportedType> exportedTypes = this.module.ExportedTypes;
            ExportedTypeTable table = this.GetTable<ExportedTypeTable>(Table.ExportedType);
            for (int i = 0; i < exportedTypes.Count; i++)
            {
                ExportedType type = exportedTypes[i];
                int rid = table.AddRow(new Row<TypeAttributes, uint, uint, uint, uint>(type.Attributes, (uint) type.Identifier, this.GetStringIndex(type.Name), this.GetStringIndex(type.Namespace), MakeCodedRID(this.GetExportedTypeScope(type), CodedIndex.Implementation)));
                type.token = new MetadataToken(TokenType.ExportedType, rid);
            }
        }

        private void AddField(FieldDefinition field)
        {
            this.field_table.AddRow(new Row<FieldAttributes, uint, uint>(field.Attributes, this.GetStringIndex(field.Name), this.GetBlobIndex(this.GetFieldSignature(field))));
            if (!field.InitialValue.IsNullOrEmpty<byte>())
            {
                this.AddFieldRVA(field);
            }
            if (field.HasLayoutInfo)
            {
                this.AddFieldLayout(field);
            }
            if (field.HasCustomAttributes)
            {
                this.AddCustomAttributes(field);
            }
            if (field.HasConstant)
            {
                this.AddConstant(field, field.FieldType);
            }
            if (field.HasMarshalInfo)
            {
                this.AddMarshalInfo(field);
            }
        }

        private void AddFieldLayout(FieldDefinition field)
        {
            this.GetTable<FieldLayoutTable>(Table.FieldLayout).AddRow(new Row<uint, uint>((uint) field.Offset, field.token.RID));
        }

        private void AddFieldRVA(FieldDefinition field)
        {
            this.GetTable<FieldRVATable>(Table.FieldRVA).AddRow(new Row<uint, uint>(this.data.AddData(field.InitialValue), field.token.RID));
        }

        private void AddFields(TypeDefinition type)
        {
            Collection<FieldDefinition> fields = type.Fields;
            for (int i = 0; i < fields.Count; i++)
            {
                this.AddField(fields[i]);
            }
        }

        private void AddGenericParameters()
        {
            GenericParameter[] items = this.generic_parameters.items;
            int size = this.generic_parameters.size;
            Array.Sort<GenericParameter>(items, 0, size, new GenericParameterComparer());
            GenericParamTable table = this.GetTable<GenericParamTable>(Table.GenericParam);
            GenericParamConstraintTable table2 = this.GetTable<GenericParamConstraintTable>(Table.GenericParamConstraint);
            for (int i = 0; i < size; i++)
            {
                GenericParameter parameter = items[i];
                int rid = table.AddRow(new Row<ushort, GenericParameterAttributes, uint, uint>((ushort) parameter.Position, parameter.Attributes, MakeCodedRID(parameter.Owner, CodedIndex.TypeOrMethodDef), this.GetStringIndex(parameter.Name)));
                parameter.token = new MetadataToken(TokenType.GenericParam, rid);
                if (parameter.HasConstraints)
                {
                    this.AddConstraints(parameter, table2);
                }
                if (parameter.HasCustomAttributes)
                {
                    this.AddCustomAttributes(parameter);
                }
            }
        }

        private void AddGenericParameters(IGenericParameterProvider owner)
        {
            Collection<GenericParameter> genericParameters = owner.GenericParameters;
            for (int i = 0; i < genericParameters.Count; i++)
            {
                this.generic_parameters.Add(genericParameters[i]);
            }
        }

        private void AddInterfaces(TypeDefinition type)
        {
            Collection<TypeReference> interfaces = type.Interfaces;
            uint rID = type.token.RID;
            for (int i = 0; i < interfaces.Count; i++)
            {
                this.iface_impl_table.AddRow(new Row<uint, uint>(rID, MakeCodedRID(this.GetTypeToken(interfaces[i]), CodedIndex.TypeDefOrRef)));
            }
        }

        private void AddLayoutInfo(TypeDefinition type)
        {
            this.GetTable<ClassLayoutTable>(Table.ClassLayout).AddRow(new Row<ushort, uint, uint>((ushort) type.PackingSize, (uint) type.ClassSize, type.token.RID));
        }

        private uint AddLinkedResource(LinkedResource resource)
        {
            FileTable table = this.GetTable<FileTable>(Table.File);
            byte[] blob = resource.Hash.IsNullOrEmpty<byte>() ? CryptoService.ComputeHash(resource.File) : resource.Hash;
            return (uint) table.AddRow(new Row<FileAttributes, uint, uint>(FileAttributes.ContainsNoMetaData, this.GetStringIndex(resource.File), this.GetBlobIndex(blob)));
        }

        private void AddMarshalInfo(IMarshalInfoProvider owner)
        {
            this.GetTable<FieldMarshalTable>(Table.FieldMarshal).AddRow(new Row<uint, uint>(MakeCodedRID(owner, CodedIndex.HasFieldMarshal), this.GetBlobIndex(this.GetMarshalInfoSignature(owner))));
        }

        private void AddMemberReference(MemberReference member, Row<uint, uint, uint> row)
        {
            member.token = new MetadataToken(TokenType.MemberRef, this.member_ref_table.AddRow(row));
            this.member_ref_map.Add(row, member.token);
        }

        private void AddMethod(MethodDefinition method)
        {
            this.method_table.AddRow(new Row<uint, MethodImplAttributes, MethodAttributes, uint, uint, uint>(method.HasBody ? this.code.WriteMethodBody(method) : 0, method.ImplAttributes, method.Attributes, this.GetStringIndex(method.Name), this.GetBlobIndex(this.GetMethodSignature(method)), this.param_rid));
            this.AddParameters(method);
            if (method.HasGenericParameters)
            {
                this.AddGenericParameters(method);
            }
            if (method.IsPInvokeImpl)
            {
                this.AddPInvokeInfo(method);
            }
            if (method.HasCustomAttributes)
            {
                this.AddCustomAttributes(method);
            }
            if (method.HasSecurityDeclarations)
            {
                this.AddSecurityDeclarations(method);
            }
            if (method.HasOverrides)
            {
                this.AddOverrides(method);
            }
        }

        private void AddMethods(TypeDefinition type)
        {
            Collection<MethodDefinition> methods = type.Methods;
            for (int i = 0; i < methods.Count; i++)
            {
                this.AddMethod(methods[i]);
            }
        }

        private void AddMethodSpecification(MethodSpecification method_spec, Row<uint, uint> row)
        {
            method_spec.token = new MetadataToken(TokenType.MethodSpec, this.method_spec_table.AddRow(row));
            this.method_spec_map.Add(row, method_spec.token);
        }

        private void AddModuleReferences()
        {
            Collection<ModuleReference> moduleReferences = this.module.ModuleReferences;
            ModuleRefTable table = this.GetTable<ModuleRefTable>(Table.ModuleRef);
            for (int i = 0; i < moduleReferences.Count; i++)
            {
                ModuleReference reference = moduleReferences[i];
                reference.token = new MetadataToken(TokenType.ModuleRef, table.AddRow(this.GetStringIndex(reference.Name)));
            }
        }

        private void AddNestedTypes(TypeDefinition type)
        {
            Collection<TypeDefinition> nestedTypes = type.NestedTypes;
            NestedClassTable table = this.GetTable<NestedClassTable>(Table.NestedClass);
            for (int i = 0; i < nestedTypes.Count; i++)
            {
                TypeDefinition definition = nestedTypes[i];
                this.AddType(definition);
                table.AddRow(new Row<uint, uint>(definition.token.RID, type.token.RID));
            }
        }

        private void AddOtherSemantic(IMetadataTokenProvider owner, Collection<MethodDefinition> others)
        {
            for (int i = 0; i < others.Count; i++)
            {
                this.AddSemantic(MethodSemanticsAttributes.None | MethodSemanticsAttributes.Other, owner, others[i]);
            }
        }

        private void AddOverrides(MethodDefinition method)
        {
            Collection<MethodReference> overrides = method.Overrides;
            MethodImplTable table = this.GetTable<MethodImplTable>(Table.MethodImpl);
            for (int i = 0; i < overrides.Count; i++)
            {
                table.AddRow(new Row<uint, uint, uint>(method.DeclaringType.token.RID, MakeCodedRID(method, CodedIndex.MethodDefOrRef), MakeCodedRID(this.LookupToken(overrides[i]), CodedIndex.MethodDefOrRef)));
            }
        }

        private void AddParameter(ushort sequence, ParameterDefinition parameter, ParamTable table)
        {
            uint num;
            table.AddRow(new Row<ParameterAttributes, ushort, uint>(parameter.Attributes, sequence, this.GetStringIndex(parameter.Name)));
            this.param_rid = (num = this.param_rid) + 1;
            parameter.token = new MetadataToken(TokenType.Param, num);
            if (parameter.HasCustomAttributes)
            {
                this.AddCustomAttributes(parameter);
            }
            if (parameter.HasConstant)
            {
                this.AddConstant(parameter, parameter.ParameterType);
            }
            if (parameter.HasMarshalInfo)
            {
                this.AddMarshalInfo(parameter);
            }
        }

        private void AddParameters(MethodDefinition method)
        {
            ParameterDefinition parameter = method.MethodReturnType.parameter;
            if ((parameter != null) && RequiresParameterRow(parameter))
            {
                this.AddParameter(0, parameter, this.param_table);
            }
            if (method.HasParameters)
            {
                Collection<ParameterDefinition> parameters = method.Parameters;
                for (int i = 0; i < parameters.Count; i++)
                {
                    ParameterDefinition definition2 = parameters[i];
                    if (RequiresParameterRow(definition2))
                    {
                        this.AddParameter((ushort) (i + 1), definition2, this.param_table);
                    }
                }
            }
        }

        private void AddPInvokeInfo(MethodDefinition method)
        {
            PInvokeInfo pInvokeInfo = method.PInvokeInfo;
            if (pInvokeInfo != null)
            {
                this.GetTable<ImplMapTable>(Table.ImplMap).AddRow(new Row<PInvokeAttributes, uint, uint, uint>(pInvokeInfo.Attributes, MakeCodedRID(method, CodedIndex.MemberForwarded), this.GetStringIndex(pInvokeInfo.EntryPoint), pInvokeInfo.Module.MetadataToken.RID));
            }
        }

        private void AddProperties(TypeDefinition type)
        {
            Collection<PropertyDefinition> properties = type.Properties;
            this.property_map_table.AddRow(new Row<uint, uint>(type.token.RID, this.property_rid));
            for (int i = 0; i < properties.Count; i++)
            {
                this.AddProperty(properties[i]);
            }
        }

        private void AddProperty(PropertyDefinition property)
        {
            uint num;
            this.property_table.AddRow(new Row<PropertyAttributes, uint, uint>(property.Attributes, this.GetStringIndex(property.Name), this.GetBlobIndex(this.GetPropertySignature(property))));
            this.property_rid = (num = this.property_rid) + 1;
            property.token = new MetadataToken(TokenType.Property, num);
            MethodDefinition getMethod = property.GetMethod;
            if (getMethod != null)
            {
                this.AddSemantic(MethodSemanticsAttributes.Getter, property, getMethod);
            }
            getMethod = property.SetMethod;
            if (getMethod != null)
            {
                this.AddSemantic(MethodSemanticsAttributes.None | MethodSemanticsAttributes.Setter, property, getMethod);
            }
            if (property.HasOtherMethods)
            {
                this.AddOtherSemantic(property, property.OtherMethods);
            }
            if (property.HasCustomAttributes)
            {
                this.AddCustomAttributes(property);
            }
            if (property.HasConstant)
            {
                this.AddConstant(property, property.PropertyType);
            }
        }

        private void AddResources()
        {
            Collection<Resource> resources = this.module.Resources;
            ManifestResourceTable table = this.GetTable<ManifestResourceTable>(Table.ManifestResource);
            for (int i = 0; i < resources.Count; i++)
            {
                Resource resource = resources[i];
                Row<uint, ManifestResourceAttributes, uint, uint> row = new Row<uint, ManifestResourceAttributes, uint, uint>(0, resource.Attributes, this.GetStringIndex(resource.Name), 0);
                ResourceType resourceType = resource.ResourceType;
                switch (resourceType)
                {
                    case ResourceType.Linked:
                        row.Col4 = CodedIndex.Implementation.CompressMetadataToken(new MetadataToken(TokenType.File, this.AddLinkedResource((LinkedResource) resource)));
                        break;

                    case ResourceType.Embedded:
                        row.Col1 = this.AddEmbeddedResource((EmbeddedResource) resource);
                        break;

                    case ResourceType.AssemblyLinked:
                        row.Col4 = CodedIndex.Implementation.CompressMetadataToken(((AssemblyLinkedResource) resource).Assembly.MetadataToken);
                        break;

                    default:
                        throw new NotSupportedException();
                }
                table.AddRow(row);
            }
        }

        private void AddSecurityDeclarations(ISecurityDeclarationProvider owner)
        {
            Collection<SecurityDeclaration> securityDeclarations = owner.SecurityDeclarations;
            for (int i = 0; i < securityDeclarations.Count; i++)
            {
                SecurityDeclaration declaration = securityDeclarations[i];
                this.declsec_table.AddRow(new Row<SecurityAction, uint, uint>(declaration.Action, MakeCodedRID(owner, CodedIndex.HasDeclSecurity), this.GetBlobIndex(this.GetSecurityDeclarationSignature(declaration))));
            }
        }

        private void AddSemantic(MethodSemanticsAttributes semantics, IMetadataTokenProvider provider, MethodDefinition method)
        {
            method.SemanticsAttributes = semantics;
            this.GetTable<MethodSemanticsTable>(Table.MethodSemantics).AddRow(new Row<MethodSemanticsAttributes, uint, uint>(semantics, method.token.RID, MakeCodedRID(provider, CodedIndex.HasSemantics)));
        }

        public uint AddStandAloneSignature(uint signature) => 
            ((uint) this.standalone_sig_table.AddRow(signature));

        private void AddType(TypeDefinition type)
        {
            this.type_def_table.AddRow(new Row<TypeAttributes, uint, uint, uint, uint, uint>(type.Attributes, this.GetStringIndex(type.Name), this.GetStringIndex(type.Namespace), MakeCodedRID(this.GetTypeToken(type.BaseType), CodedIndex.TypeDefOrRef), type.fields_range.Start, type.methods_range.Start));
            if (type.HasGenericParameters)
            {
                this.AddGenericParameters(type);
            }
            if (type.HasInterfaces)
            {
                this.AddInterfaces(type);
            }
            if (type.HasLayoutInfo)
            {
                this.AddLayoutInfo(type);
            }
            if (type.HasFields)
            {
                this.AddFields(type);
            }
            if (type.HasMethods)
            {
                this.AddMethods(type);
            }
            if (type.HasProperties)
            {
                this.AddProperties(type);
            }
            if (type.HasEvents)
            {
                this.AddEvents(type);
            }
            if (type.HasCustomAttributes)
            {
                this.AddCustomAttributes(type);
            }
            if (type.HasSecurityDeclarations)
            {
                this.AddSecurityDeclarations(type);
            }
            if (type.HasNestedTypes)
            {
                this.AddNestedTypes(type);
            }
        }

        private void AddTypeDefs()
        {
            Collection<TypeDefinition> types = this.module.Types;
            for (int i = 0; i < types.Count; i++)
            {
                this.AddType(types[i]);
            }
        }

        private MetadataToken AddTypeReference(TypeReference type, Row<uint, uint, uint> row)
        {
            type.token = new MetadataToken(TokenType.TypeRef, this.type_ref_table.AddRow(row));
            MetadataToken token = type.token;
            this.type_ref_map.Add(row, token);
            return token;
        }

        private MetadataToken AddTypeSpecification(TypeReference type, uint row)
        {
            type.token = new MetadataToken(TokenType.TypeSpec, this.typespec_table.AddRow(row));
            MetadataToken token = type.token;
            this.type_spec_map.Add(row, token);
            return token;
        }

        private void AttachFieldsDefToken(TypeDefinition type)
        {
            Collection<FieldDefinition> fields = type.Fields;
            type.fields_range.Length = (uint) fields.Count;
            for (int i = 0; i < fields.Count; i++)
            {
                uint num2;
                this.field_rid = (num2 = this.field_rid) + 1;
                fields[i].token = new MetadataToken(TokenType.Field, num2);
            }
        }

        private void AttachMethodsDefToken(TypeDefinition type)
        {
            Collection<MethodDefinition> methods = type.Methods;
            type.methods_range.Length = (uint) methods.Count;
            for (int i = 0; i < methods.Count; i++)
            {
                uint num2;
                MethodDefinition definition = methods[i];
                this.method_rid = (num2 = this.method_rid) + 1;
                MetadataToken key = new MetadataToken(TokenType.Method, num2);
                if (this.write_symbols && (definition.token != MetadataToken.Zero))
                {
                    this.method_def_map.Add(key, definition.token);
                }
                definition.token = key;
            }
        }

        private void AttachNestedTypesDefToken(TypeDefinition type)
        {
            Collection<TypeDefinition> nestedTypes = type.NestedTypes;
            for (int i = 0; i < nestedTypes.Count; i++)
            {
                this.AttachTypeDefToken(nestedTypes[i]);
            }
        }

        private void AttachTokens()
        {
            Collection<TypeDefinition> types = this.module.Types;
            for (int i = 0; i < types.Count; i++)
            {
                this.AttachTypeDefToken(types[i]);
            }
        }

        private void AttachTypeDefToken(TypeDefinition type)
        {
            uint num;
            this.type_rid = (num = this.type_rid) + 1;
            type.token = new MetadataToken(TokenType.TypeDef, num);
            type.fields_range.Start = this.field_rid;
            type.methods_range.Start = this.method_rid;
            if (type.HasFields)
            {
                this.AttachFieldsDefToken(type);
            }
            if (type.HasMethods)
            {
                this.AttachMethodsDefToken(type);
            }
            if (type.HasNestedTypes)
            {
                this.AttachNestedTypesDefToken(type);
            }
        }

        private void BuildAssembly()
        {
            AssemblyDefinition assembly = this.module.Assembly;
            AssemblyNameDefinition name = assembly.Name;
            this.GetTable<AssemblyTable>(Table.Assembly).row = new Row<AssemblyHashAlgorithm, ushort, ushort, ushort, ushort, AssemblyAttributes, uint, uint, uint>(name.HashAlgorithm, (ushort) name.Version.Major, (ushort) name.Version.Minor, (ushort) name.Version.Build, (ushort) name.Version.Revision, name.Attributes, this.GetBlobIndex(name.PublicKey), this.GetStringIndex(name.Name), this.GetStringIndex(name.Culture));
            if (assembly.Modules.Count > 1)
            {
                this.BuildModules();
            }
        }

        public void BuildMetadata()
        {
            this.BuildModule();
            this.table_heap.WriteTableHeap();
        }

        private void BuildModule()
        {
            this.GetTable<ModuleTable>(Table.Module).row = this.GetStringIndex(this.module.Name);
            AssemblyDefinition assembly = this.module.Assembly;
            if (assembly != null)
            {
                this.BuildAssembly();
            }
            if (this.module.HasAssemblyReferences)
            {
                this.AddAssemblyReferences();
            }
            if (this.module.HasModuleReferences)
            {
                this.AddModuleReferences();
            }
            if (this.module.HasResources)
            {
                this.AddResources();
            }
            if (this.module.HasExportedTypes)
            {
                this.AddExportedTypes();
            }
            this.BuildTypes();
            if (assembly != null)
            {
                if (assembly.HasCustomAttributes)
                {
                    this.AddCustomAttributes(assembly);
                }
                if (assembly.HasSecurityDeclarations)
                {
                    this.AddSecurityDeclarations(assembly);
                }
            }
            if (this.module.HasCustomAttributes)
            {
                this.AddCustomAttributes(this.module);
            }
            if (this.module.EntryPoint != null)
            {
                this.entry_point = this.LookupToken(this.module.EntryPoint);
            }
        }

        private void BuildModules()
        {
            Collection<ModuleDefinition> modules = this.module.Assembly.Modules;
            FileTable table = this.GetTable<FileTable>(Table.File);
            for (int i = 0; i < modules.Count; i++)
            {
                ModuleDefinition definition = modules[i];
                if (!definition.IsMain)
                {
                    WriterParameters parameters = new WriterParameters {
                        SymbolWriterProvider = this.symbol_writer_provider
                    };
                    string moduleFileName = this.GetModuleFileName(definition.Name);
                    definition.Write(moduleFileName, parameters);
                    byte[] blob = CryptoService.ComputeHash(moduleFileName);
                    table.AddRow(new Row<FileAttributes, uint, uint>(FileAttributes.ContainsMetaData, this.GetStringIndex(definition.Name), this.GetBlobIndex(blob)));
                }
            }
        }

        private void BuildTypes()
        {
            if (this.module.HasTypes)
            {
                this.AttachTokens();
                this.AddTypeDefs();
                this.AddGenericParameters();
            }
        }

        private static Exception CreateForeignMemberException(MemberReference member) => 
            new ArgumentException($"Member '{member}' is declared in another module and needs to be imported");

        private Row<uint, uint, uint> CreateMemberRefRow(MemberReference member) => 
            new Row<uint, uint, uint>(MakeCodedRID(this.GetTypeToken(member.DeclaringType), CodedIndex.MemberRefParent), this.GetStringIndex(member.Name), this.GetBlobIndex(this.GetMemberRefSignature(member)));

        private Row<uint, uint> CreateMethodSpecRow(MethodSpecification method_spec) => 
            new Row<uint, uint>(MakeCodedRID(this.LookupToken(method_spec.ElementMethod), CodedIndex.MethodDefOrRef), this.GetBlobIndex(this.GetMethodSpecSignature(method_spec)));

        private SignatureWriter CreateSignatureWriter() => 
            new SignatureWriter(this);

        private TextMap CreateTextMap()
        {
            TextMap map = new TextMap();
            map.AddMap(TextSegment.ImportAddressTable, (this.module.Architecture == TargetArchitecture.I386) ? 8 : 0);
            map.AddMap(TextSegment.CLIHeader, 0x48, 8);
            return map;
        }

        private Row<uint, uint, uint> CreateTypeRefRow(TypeReference type) => 
            new Row<uint, uint, uint>(MakeCodedRID(this.GetScopeToken(type), CodedIndex.ResolutionScope), this.GetStringIndex(type.Name), this.GetStringIndex(type.Namespace));

        private uint GetBlobIndex(ByteBuffer blob) => 
            ((blob.length != 0) ? this.blob_heap.GetBlobIndex(blob) : 0);

        private uint GetBlobIndex(byte[] blob) => 
            (!blob.IsNullOrEmpty<byte>() ? this.GetBlobIndex(new ByteBuffer(blob)) : 0);

        public uint GetCallSiteBlobIndex(CallSite call_site) => 
            this.GetBlobIndex(this.GetMethodSignature(call_site));

        private SignatureWriter GetConstantSignature(ElementType type, object value)
        {
            SignatureWriter writer = this.CreateSignatureWriter();
            ElementType type2 = type;
            switch (type2)
            {
                case ElementType.String:
                    writer.WriteConstantString((string) value);
                    return writer;

                case ElementType.Ptr:
                case ElementType.ByRef:
                case ElementType.ValueType:
                    goto TR_0001;

                case ElementType.Class:
                case ElementType.Var:
                case ElementType.Array:
                    break;

                default:
                    switch (type2)
                    {
                        case ElementType.Object:
                        case ElementType.SzArray:
                        case ElementType.MVar:
                            break;

                        default:
                            goto TR_0001;
                    }
                    break;
            }
            writer.WriteInt32(0);
            return writer;
        TR_0001:
            writer.WriteConstantPrimitive(value);
            return writer;
        }

        private static ElementType GetConstantType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return ElementType.Boolean;

                case TypeCode.Char:
                    return ElementType.Char;

                case TypeCode.SByte:
                    return ElementType.I1;

                case TypeCode.Byte:
                    return ElementType.U1;

                case TypeCode.Int16:
                    return ElementType.I2;

                case TypeCode.UInt16:
                    return ElementType.U2;

                case TypeCode.Int32:
                    return ElementType.I4;

                case TypeCode.UInt32:
                    return ElementType.U4;

                case TypeCode.Int64:
                    return ElementType.I8;

                case TypeCode.UInt64:
                    return ElementType.U8;

                case TypeCode.Single:
                    return ElementType.R4;

                case TypeCode.Double:
                    return ElementType.R8;

                case TypeCode.String:
                    return ElementType.String;
            }
            throw new NotSupportedException(type.FullName);
        }

        private static ElementType GetConstantType(TypeReference constant_type, object constant)
        {
            if (constant == null)
            {
                return ElementType.Class;
            }
            ElementType etype = constant_type.etype;
            ElementType type3 = etype;
            switch (type3)
            {
                case ElementType.None:
                {
                    TypeDefinition self = constant_type.CheckedResolve();
                    return (!self.IsEnum ? ElementType.Class : GetConstantType(self.GetEnumUnderlyingType(), constant));
                }
                case ElementType.Void:
                case ElementType.Ptr:
                case ElementType.ValueType:
                case ElementType.Class:
                case ElementType.TypedByRef:
                case (ElementType.Array | ElementType.Boolean | ElementType.Void):
                case (ElementType.Boolean | ElementType.ByRef | ElementType.I4):
                case ElementType.FnPtr:
                    return etype;

                case ElementType.Boolean:
                case ElementType.Char:
                case ElementType.I1:
                case ElementType.U1:
                case ElementType.I2:
                case ElementType.U2:
                case ElementType.I4:
                case ElementType.U4:
                case ElementType.I8:
                case ElementType.U8:
                case ElementType.R4:
                case ElementType.R8:
                case ElementType.I:
                case ElementType.U:
                    return GetConstantType(constant.GetType());

                case ElementType.String:
                    return ElementType.String;

                case ElementType.ByRef:
                case ElementType.CModReqD:
                case ElementType.CModOpt:
                    break;

                case ElementType.Var:
                case ElementType.Array:
                case ElementType.SzArray:
                case ElementType.MVar:
                    return ElementType.Class;

                case ElementType.GenericInst:
                {
                    GenericInstanceType type2 = (GenericInstanceType) constant_type;
                    return (!type2.ElementType.IsTypeOf("System", "Nullable`1") ? GetConstantType(((TypeSpecification) constant_type).ElementType, constant) : GetConstantType(type2.GenericArguments[0], constant));
                }
                case ElementType.Object:
                    return GetConstantType(constant.GetType());

                default:
                    if (type3 == ElementType.Sentinel)
                    {
                        break;
                    }
                    return etype;
            }
            return GetConstantType(((TypeSpecification) constant_type).ElementType, constant);
        }

        private SignatureWriter GetCustomAttributeSignature(CustomAttribute attribute)
        {
            SignatureWriter writer = this.CreateSignatureWriter();
            if (!attribute.resolved)
            {
                writer.WriteBytes(attribute.GetBlob());
                return writer;
            }
            writer.WriteUInt16(1);
            writer.WriteCustomAttributeConstructorArguments(attribute);
            writer.WriteCustomAttributeNamedArguments(attribute);
            return writer;
        }

        private MetadataToken GetExportedTypeScope(ExportedType exported_type)
        {
            if (exported_type.DeclaringType != null)
            {
                return exported_type.DeclaringType.MetadataToken;
            }
            IMetadataScope scope = exported_type.Scope;
            TokenType tokenType = scope.MetadataToken.TokenType;
            if (tokenType != TokenType.ModuleRef)
            {
                if (tokenType == TokenType.AssemblyRef)
                {
                    return scope.MetadataToken;
                }
            }
            else
            {
                FileTable table = this.GetTable<FileTable>(Table.File);
                for (int i = 0; i < table.length; i++)
                {
                    if (table.rows[i].Col2 == this.GetStringIndex(scope.Name))
                    {
                        return new MetadataToken(TokenType.File, i + 1);
                    }
                }
            }
            throw new NotSupportedException();
        }

        private SignatureWriter GetFieldSignature(FieldReference field)
        {
            SignatureWriter writer = this.CreateSignatureWriter();
            writer.WriteByte(6);
            writer.WriteTypeSignature(field.FieldType);
            return writer;
        }

        public uint GetLocalVariableBlobIndex(Collection<VariableDefinition> variables) => 
            this.GetBlobIndex(this.GetVariablesSignature(variables));

        private SignatureWriter GetMarshalInfoSignature(IMarshalInfoProvider owner)
        {
            SignatureWriter writer = this.CreateSignatureWriter();
            writer.WriteMarshalInfo(owner.MarshalInfo);
            return writer;
        }

        private SignatureWriter GetMemberRefSignature(MemberReference member)
        {
            FieldReference field = member as FieldReference;
            if (field != null)
            {
                return this.GetFieldSignature(field);
            }
            MethodReference method = member as MethodReference;
            if (method == null)
            {
                throw new NotSupportedException();
            }
            return this.GetMethodSignature(method);
        }

        private MetadataToken GetMemberRefToken(MemberReference member)
        {
            MetadataToken token;
            Row<uint, uint, uint> key = this.CreateMemberRefRow(member);
            if (this.member_ref_map.TryGetValue(key, out token))
            {
                return token;
            }
            this.AddMemberReference(member, key);
            return member.token;
        }

        private SignatureWriter GetMethodSignature(IMethodSignature method)
        {
            SignatureWriter writer = this.CreateSignatureWriter();
            writer.WriteMethodSignature(method);
            return writer;
        }

        private SignatureWriter GetMethodSpecSignature(MethodSpecification method_spec)
        {
            if (!method_spec.IsGenericInstance)
            {
                throw new NotSupportedException();
            }
            SignatureWriter writer = this.CreateSignatureWriter();
            writer.WriteByte(10);
            writer.WriteGenericInstanceSignature((GenericInstanceMethod) method_spec);
            return writer;
        }

        private MetadataToken GetMethodSpecToken(MethodSpecification method_spec)
        {
            MetadataToken token;
            Row<uint, uint> key = this.CreateMethodSpecRow(method_spec);
            if (this.method_spec_map.TryGetValue(key, out token))
            {
                return token;
            }
            this.AddMethodSpecification(method_spec, key);
            return method_spec.token;
        }

        private string GetModuleFileName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new NotSupportedException();
            }
            return Path.Combine(Path.GetDirectoryName(this.fq_name), name);
        }

        private SignatureWriter GetPropertySignature(PropertyDefinition property)
        {
            SignatureWriter writer = this.CreateSignatureWriter();
            byte num = 8;
            if (property.HasThis)
            {
                num = (byte) (num | 0x20);
            }
            uint count = 0;
            Collection<ParameterDefinition> parameters = null;
            if (property.HasParameters)
            {
                parameters = property.Parameters;
                count = (uint) parameters.Count;
            }
            writer.WriteByte(num);
            writer.WriteCompressedUInt32(count);
            writer.WriteTypeSignature(property.PropertyType);
            if (count != 0)
            {
                for (int i = 0; i < count; i++)
                {
                    writer.WriteTypeSignature(parameters[i].ParameterType);
                }
            }
            return writer;
        }

        private MetadataToken GetScopeToken(TypeReference type)
        {
            if (type.IsNested)
            {
                return this.GetTypeRefToken(type.DeclaringType);
            }
            IMetadataScope scope = type.Scope;
            return ((scope != null) ? scope.MetadataToken : MetadataToken.Zero);
        }

        private SignatureWriter GetSecurityDeclarationSignature(SecurityDeclaration declaration)
        {
            SignatureWriter writer = this.CreateSignatureWriter();
            if (!declaration.resolved)
            {
                writer.WriteBytes(declaration.GetBlob());
            }
            else if (this.module.Runtime < TargetRuntime.Net_2_0)
            {
                writer.WriteXmlSecurityDeclaration(declaration);
            }
            else
            {
                writer.WriteSecurityDeclaration(declaration);
            }
            return writer;
        }

        private uint GetStringIndex(string @string) => 
            (!string.IsNullOrEmpty(@string) ? this.string_heap.GetStringIndex(@string) : 0);

        private TTable GetTable<TTable>(Table table) where TTable: MetadataTable, new() => 
            this.table_heap.GetTable<TTable>(table);

        private MetadataToken GetTypeRefToken(TypeReference type)
        {
            MetadataToken token;
            Row<uint, uint, uint> key = this.CreateTypeRefRow(type);
            return (!this.type_ref_map.TryGetValue(key, out token) ? this.AddTypeReference(type, key) : token);
        }

        private SignatureWriter GetTypeSpecSignature(TypeReference type)
        {
            SignatureWriter writer = this.CreateSignatureWriter();
            writer.WriteTypeSignature(type);
            return writer;
        }

        private MetadataToken GetTypeSpecToken(TypeReference type)
        {
            MetadataToken token;
            uint blobIndex = this.GetBlobIndex(this.GetTypeSpecSignature(type));
            return (!this.type_spec_map.TryGetValue(blobIndex, out token) ? this.AddTypeSpecification(type, blobIndex) : token);
        }

        private MetadataToken GetTypeToken(TypeReference type) => 
            ((type != null) ? (!type.IsDefinition ? (!type.IsTypeSpecification() ? this.GetTypeRefToken(type) : this.GetTypeSpecToken(type)) : type.token) : MetadataToken.Zero);

        private SignatureWriter GetVariablesSignature(Collection<VariableDefinition> variables)
        {
            SignatureWriter writer = this.CreateSignatureWriter();
            writer.WriteByte(7);
            writer.WriteCompressedUInt32((uint) variables.Count);
            for (int i = 0; i < variables.Count; i++)
            {
                writer.WriteTypeSignature(variables[i].VariableType);
            }
            return writer;
        }

        public MetadataToken LookupToken(IMetadataTokenProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException();
            }
            MemberReference member = provider as MemberReference;
            if ((member == null) || !ReferenceEquals(member.Module, this.module))
            {
                throw CreateForeignMemberException(member);
            }
            MetadataToken metadataToken = provider.MetadataToken;
            TokenType tokenType = metadataToken.TokenType;
            if (tokenType > TokenType.MemberRef)
            {
                if (tokenType > TokenType.Property)
                {
                    if ((tokenType == TokenType.TypeSpec) || (tokenType == TokenType.GenericParam))
                    {
                        goto TR_0004;
                    }
                    else if (tokenType == TokenType.MethodSpec)
                    {
                        return this.GetMethodSpecToken((MethodSpecification) provider);
                    }
                }
                else if ((tokenType == TokenType.Event) || (tokenType == TokenType.Property))
                {
                    return metadataToken;
                }
            }
            else
            {
                if (tokenType > TokenType.TypeDef)
                {
                    if ((tokenType != TokenType.Field) && (tokenType != TokenType.Method))
                    {
                        if (tokenType == TokenType.MemberRef)
                        {
                            return this.GetMemberRefToken(member);
                        }
                        goto TR_0001;
                    }
                }
                else if (tokenType == TokenType.TypeRef)
                {
                    goto TR_0004;
                }
                else if (tokenType != TokenType.TypeDef)
                {
                    goto TR_0001;
                }
                return metadataToken;
            }
        TR_0001:
            throw new NotSupportedException();
        TR_0004:
            return this.GetTypeToken((TypeReference) provider);
        }

        private static uint MakeCodedRID(IMetadataTokenProvider provider, CodedIndex index) => 
            MakeCodedRID(provider.MetadataToken, index);

        private static uint MakeCodedRID(MetadataToken token, CodedIndex index) => 
            index.CompressMetadataToken(token);

        private static bool RequiresParameterRow(ParameterDefinition parameter) => 
            (!string.IsNullOrEmpty(parameter.Name) || ((parameter.Attributes != ParameterAttributes.None) || (parameter.HasMarshalInfo || (parameter.HasConstant || parameter.HasCustomAttributes))));

        public bool TryGetOriginalMethodToken(MetadataToken new_token, out MetadataToken original) => 
            this.method_def_map.TryGetValue(new_token, out original);

        private sealed class GenericParameterComparer : IComparer<GenericParameter>
        {
            public int Compare(GenericParameter a, GenericParameter b)
            {
                uint num = MetadataBuilder.MakeCodedRID(a.Owner, CodedIndex.TypeOrMethodDef);
                uint num2 = MetadataBuilder.MakeCodedRID(b.Owner, CodedIndex.TypeOrMethodDef);
                if (num != num2)
                {
                    return ((num > num2) ? 1 : -1);
                }
                int position = a.Position;
                int num4 = b.Position;
                return ((position == num4) ? 0 : ((position > num4) ? 1 : -1));
            }
        }
    }
}

