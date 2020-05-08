namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using Mono.Collections.Generic;
    using System;

    internal sealed class TypeDefinition : TypeReference, IMemberDefinition, ICustomAttributeProvider, ISecurityDeclarationProvider, IMetadataTokenProvider
    {
        private uint attributes;
        private TypeReference base_type;
        internal Range fields_range;
        internal Range methods_range;
        private short packing_size;
        private int class_size;
        private Collection<TypeReference> interfaces;
        private Collection<TypeDefinition> nested_types;
        private Collection<MethodDefinition> methods;
        private Collection<FieldDefinition> fields;
        private Collection<EventDefinition> events;
        private Collection<PropertyDefinition> properties;
        private Collection<CustomAttribute> custom_attributes;
        private Collection<SecurityDeclaration> security_declarations;

        public TypeDefinition(string @namespace, string name, TypeAttributes attributes) : base(@namespace, name)
        {
            this.packing_size = -2;
            this.class_size = -2;
            this.attributes = (uint) attributes;
            base.token = new MetadataToken(TokenType.TypeDef);
        }

        public TypeDefinition(string @namespace, string name, TypeAttributes attributes, TypeReference baseType) : this(@namespace, name, attributes)
        {
            this.BaseType = baseType;
        }

        public override TypeDefinition Resolve() => 
            this;

        private void ResolveLayout()
        {
            if ((this.packing_size == -2) && (this.class_size == -2))
            {
                if (!base.HasImage)
                {
                    this.packing_size = -1;
                    this.class_size = -1;
                }
                else
                {
                    Row<short, int> row = this.Module.Read<TypeDefinition, Row<short, int>>(this, (type, reader) => reader.ReadTypeLayout(type));
                    this.packing_size = row.Col1;
                    this.class_size = row.Col2;
                }
            }
        }

        public TypeAttributes Attributes
        {
            get => 
                ((TypeAttributes) this.attributes);
            set => 
                (this.attributes = (uint) value);
        }

        public TypeReference BaseType
        {
            get => 
                this.base_type;
            set => 
                (this.base_type = value);
        }

        public bool HasLayoutInfo
        {
            get
            {
                if ((this.packing_size >= 0) || (this.class_size >= 0))
                {
                    return true;
                }
                this.ResolveLayout();
                return ((this.packing_size >= 0) || (this.class_size >= 0));
            }
        }

        public short PackingSize
        {
            get
            {
                if (this.packing_size >= 0)
                {
                    return this.packing_size;
                }
                this.ResolveLayout();
                return ((this.packing_size >= 0) ? this.packing_size : -1);
            }
            set => 
                (this.packing_size = value);
        }

        public int ClassSize
        {
            get
            {
                if (this.class_size >= 0)
                {
                    return this.class_size;
                }
                this.ResolveLayout();
                return ((this.class_size >= 0) ? this.class_size : -1);
            }
            set => 
                (this.class_size = value);
        }

        public bool HasInterfaces
        {
            get
            {
                if (this.interfaces != null)
                {
                    return (this.interfaces.Count > 0);
                }
                if (!base.HasImage)
                {
                    return false;
                }
                return this.Module.Read<TypeDefinition, bool>(this, (type, reader) => reader.HasInterfaces(type));
            }
        }

        public Collection<TypeReference> Interfaces
        {
            get
            {
                if (this.interfaces != null)
                {
                    return this.interfaces;
                }
                if (!base.HasImage)
                {
                    Collection<TypeReference> collection;
                    this.interfaces = collection = new Collection<TypeReference>();
                    return collection;
                }
                return this.Module.Read<TypeDefinition, Collection<TypeReference>>(ref this.interfaces, this, (type, reader) => reader.ReadInterfaces(type));
            }
        }

        public bool HasNestedTypes
        {
            get
            {
                if (this.nested_types != null)
                {
                    return (this.nested_types.Count > 0);
                }
                if (!base.HasImage)
                {
                    return false;
                }
                return this.Module.Read<TypeDefinition, bool>(this, (type, reader) => reader.HasNestedTypes(type));
            }
        }

        public Collection<TypeDefinition> NestedTypes
        {
            get
            {
                if (this.nested_types != null)
                {
                    return this.nested_types;
                }
                if (!base.HasImage)
                {
                    Collection<TypeDefinition> collection;
                    this.nested_types = collection = new MemberDefinitionCollection<TypeDefinition>(this);
                    return collection;
                }
                return this.Module.Read<TypeDefinition, Collection<TypeDefinition>>(ref this.nested_types, this, (type, reader) => reader.ReadNestedTypes(type));
            }
        }

        public bool HasMethods =>
            ((this.methods == null) ? (base.HasImage && (this.methods_range.Length != 0)) : (this.methods.Count > 0));

        public Collection<MethodDefinition> Methods
        {
            get
            {
                if (this.methods != null)
                {
                    return this.methods;
                }
                if (!base.HasImage)
                {
                    Collection<MethodDefinition> collection;
                    this.methods = collection = new MemberDefinitionCollection<MethodDefinition>(this);
                    return collection;
                }
                return this.Module.Read<TypeDefinition, Collection<MethodDefinition>>(ref this.methods, this, (type, reader) => reader.ReadMethods(type));
            }
        }

        public bool HasFields =>
            ((this.fields == null) ? (base.HasImage && (this.fields_range.Length != 0)) : (this.fields.Count > 0));

        public Collection<FieldDefinition> Fields
        {
            get
            {
                if (this.fields != null)
                {
                    return this.fields;
                }
                if (!base.HasImage)
                {
                    Collection<FieldDefinition> collection;
                    this.fields = collection = new MemberDefinitionCollection<FieldDefinition>(this);
                    return collection;
                }
                return this.Module.Read<TypeDefinition, Collection<FieldDefinition>>(ref this.fields, this, (type, reader) => reader.ReadFields(type));
            }
        }

        public bool HasEvents
        {
            get
            {
                if (this.events != null)
                {
                    return (this.events.Count > 0);
                }
                if (!base.HasImage)
                {
                    return false;
                }
                return this.Module.Read<TypeDefinition, bool>(this, (type, reader) => reader.HasEvents(type));
            }
        }

        public Collection<EventDefinition> Events
        {
            get
            {
                if (this.events != null)
                {
                    return this.events;
                }
                if (!base.HasImage)
                {
                    Collection<EventDefinition> collection;
                    this.events = collection = new MemberDefinitionCollection<EventDefinition>(this);
                    return collection;
                }
                return this.Module.Read<TypeDefinition, Collection<EventDefinition>>(ref this.events, this, (type, reader) => reader.ReadEvents(type));
            }
        }

        public bool HasProperties
        {
            get
            {
                if (this.properties != null)
                {
                    return (this.properties.Count > 0);
                }
                if (!base.HasImage)
                {
                    return false;
                }
                return this.Module.Read<TypeDefinition, bool>(this, (type, reader) => reader.HasProperties(type));
            }
        }

        public Collection<PropertyDefinition> Properties
        {
            get
            {
                if (this.properties != null)
                {
                    return this.properties;
                }
                if (!base.HasImage)
                {
                    Collection<PropertyDefinition> collection;
                    this.properties = collection = new MemberDefinitionCollection<PropertyDefinition>(this);
                    return collection;
                }
                return this.Module.Read<TypeDefinition, Collection<PropertyDefinition>>(ref this.properties, this, (type, reader) => reader.ReadProperties(type));
            }
        }

        public bool HasSecurityDeclarations =>
            ((this.security_declarations == null) ? this.GetHasSecurityDeclarations(this.Module) : (this.security_declarations.Count > 0));

        public Collection<SecurityDeclaration> SecurityDeclarations =>
            (this.security_declarations ?? this.GetSecurityDeclarations(ref this.security_declarations, this.Module));

        public bool HasCustomAttributes =>
            ((this.custom_attributes == null) ? this.GetHasCustomAttributes(this.Module) : (this.custom_attributes.Count > 0));

        public Collection<CustomAttribute> CustomAttributes =>
            (this.custom_attributes ?? this.GetCustomAttributes(ref this.custom_attributes, this.Module));

        public override bool HasGenericParameters =>
            ((base.generic_parameters == null) ? this.GetHasGenericParameters(this.Module) : (base.generic_parameters.Count > 0));

        public override Collection<GenericParameter> GenericParameters =>
            (base.generic_parameters ?? this.GetGenericParameters(ref base.generic_parameters, this.Module));

        public bool IsNotPublic
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 0);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 0, value));
        }

        public bool IsPublic
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 1);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 1, value));
        }

        public bool IsNestedPublic
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 2);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 2, value));
        }

        public bool IsNestedPrivate
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 3);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 3, value));
        }

        public bool IsNestedFamily
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 4);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 4, value));
        }

        public bool IsNestedAssembly
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 5);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 5, value));
        }

        public bool IsNestedFamilyAndAssembly
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 6);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 6, value));
        }

        public bool IsNestedFamilyOrAssembly
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 7);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 7, value));
        }

        public bool IsAutoLayout
        {
            get => 
                this.attributes.GetMaskedAttributes(0x18, 0);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x18, 0, value));
        }

        public bool IsSequentialLayout
        {
            get => 
                this.attributes.GetMaskedAttributes(0x18, 8);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x18, 8, value));
        }

        public bool IsExplicitLayout
        {
            get => 
                this.attributes.GetMaskedAttributes(0x18, 0x10);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x18, 0x10, value));
        }

        public bool IsClass
        {
            get => 
                this.attributes.GetMaskedAttributes(0x20, 0);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x20, 0, value));
        }

        public bool IsInterface
        {
            get => 
                this.attributes.GetMaskedAttributes(0x20, 0x20);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x20, 0x20, value));
        }

        public bool IsAbstract
        {
            get => 
                this.attributes.GetAttributes(0x80);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x80, value));
        }

        public bool IsSealed
        {
            get => 
                this.attributes.GetAttributes(0x100);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x100, value));
        }

        public bool IsSpecialName
        {
            get => 
                this.attributes.GetAttributes(0x400);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x400, value));
        }

        public bool IsImport
        {
            get => 
                this.attributes.GetAttributes(0x1000);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x1000, value));
        }

        public bool IsSerializable
        {
            get => 
                this.attributes.GetAttributes(0x2000);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x2000, value));
        }

        public bool IsWindowsRuntime
        {
            get => 
                this.attributes.GetAttributes(0x4000);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x4000, value));
        }

        public bool IsAnsiClass
        {
            get => 
                this.attributes.GetMaskedAttributes(0x30000, 0);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x30000, 0, value));
        }

        public bool IsUnicodeClass
        {
            get => 
                this.attributes.GetMaskedAttributes(0x30000, 0x10000);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x30000, 0x10000, value));
        }

        public bool IsAutoClass
        {
            get => 
                this.attributes.GetMaskedAttributes(0x30000, 0x20000);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x30000, 0x20000, value));
        }

        public bool IsBeforeFieldInit
        {
            get => 
                this.attributes.GetAttributes(0x100000);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x100000, value));
        }

        public bool IsRuntimeSpecialName
        {
            get => 
                this.attributes.GetAttributes(0x800);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x800, value));
        }

        public bool HasSecurity
        {
            get => 
                this.attributes.GetAttributes(0x40000);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x40000, value));
        }

        public bool IsEnum =>
            ((this.base_type != null) && this.base_type.IsTypeOf("System", "Enum"));

        public override bool IsValueType =>
            ((this.base_type != null) ? (this.base_type.IsTypeOf("System", "Enum") || (this.base_type.IsTypeOf("System", "ValueType") && !this.IsTypeOf("System", "Enum"))) : false);

        public override bool IsPrimitive
        {
            get
            {
                ElementType type;
                return MetadataSystem.TryGetPrimitiveElementType(this, out type);
            }
        }

        public override Mono.Cecil.MetadataType MetadataType
        {
            get
            {
                ElementType type;
                return (!MetadataSystem.TryGetPrimitiveElementType(this, out type) ? base.MetadataType : ((Mono.Cecil.MetadataType) type));
            }
        }

        public override bool IsDefinition =>
            true;

        public TypeDefinition DeclaringType
        {
            get => 
                ((TypeDefinition) base.DeclaringType);
            set => 
                (base.DeclaringType = value);
        }
    }
}

