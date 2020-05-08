namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using Mono.Collections.Generic;
    using System;

    internal sealed class GenericParameter : TypeReference, ICustomAttributeProvider, IMetadataTokenProvider
    {
        internal int position;
        internal GenericParameterType type;
        internal IGenericParameterProvider owner;
        private ushort attributes;
        private Collection<TypeReference> constraints;
        private Collection<CustomAttribute> custom_attributes;

        public GenericParameter(IGenericParameterProvider owner) : this(string.Empty, owner)
        {
        }

        public GenericParameter(string name, IGenericParameterProvider owner) : base(string.Empty, name)
        {
            if (owner == null)
            {
                throw new ArgumentNullException();
            }
            this.position = -1;
            this.owner = owner;
            this.type = owner.GenericParameterType;
            base.etype = ConvertGenericParameterType(this.type);
            base.token = new MetadataToken(TokenType.GenericParam);
        }

        internal GenericParameter(int position, GenericParameterType type, ModuleDefinition module) : base(string.Empty, string.Empty)
        {
            if (module == null)
            {
                throw new ArgumentNullException();
            }
            this.position = position;
            this.type = type;
            base.etype = ConvertGenericParameterType(type);
            base.module = module;
            base.token = new MetadataToken(TokenType.GenericParam);
        }

        private static ElementType ConvertGenericParameterType(GenericParameterType type)
        {
            switch (type)
            {
                case GenericParameterType.Type:
                    return ElementType.Var;

                case GenericParameterType.Method:
                    return ElementType.MVar;
            }
            throw new ArgumentOutOfRangeException();
        }

        public override TypeDefinition Resolve() => 
            null;

        public GenericParameterAttributes Attributes
        {
            get => 
                ((GenericParameterAttributes) this.attributes);
            set => 
                (this.attributes = (ushort) value);
        }

        public int Position =>
            this.position;

        public GenericParameterType Type =>
            this.type;

        public IGenericParameterProvider Owner =>
            this.owner;

        public bool HasConstraints
        {
            get
            {
                if (this.constraints != null)
                {
                    return (this.constraints.Count > 0);
                }
                if (!base.HasImage)
                {
                    return false;
                }
                return this.Module.Read<GenericParameter, bool>(this, (generic_parameter, reader) => reader.HasGenericConstraints(generic_parameter));
            }
        }

        public Collection<TypeReference> Constraints
        {
            get
            {
                if (this.constraints != null)
                {
                    return this.constraints;
                }
                if (!base.HasImage)
                {
                    Collection<TypeReference> collection;
                    this.constraints = collection = new Collection<TypeReference>();
                    return collection;
                }
                return this.Module.Read<GenericParameter, Collection<TypeReference>>(ref this.constraints, this, (generic_parameter, reader) => reader.ReadGenericConstraints(generic_parameter));
            }
        }

        public bool HasCustomAttributes =>
            ((this.custom_attributes == null) ? this.GetHasCustomAttributes(this.Module) : (this.custom_attributes.Count > 0));

        public Collection<CustomAttribute> CustomAttributes =>
            (this.custom_attributes ?? this.GetCustomAttributes(ref this.custom_attributes, this.Module));

        public override IMetadataScope Scope
        {
            get => 
                ((this.owner?.GenericParameterType == GenericParameterType.Method) ? ((MethodReference) this.owner).DeclaringType.Scope : ((TypeReference) this.owner).Scope);
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override TypeReference DeclaringType
        {
            get => 
                (this.owner as TypeReference);
            set
            {
                throw new InvalidOperationException();
            }
        }

        public MethodReference DeclaringMethod =>
            (this.owner as MethodReference);

        public override ModuleDefinition Module =>
            (base.module ?? this.owner.Module);

        public override string Name
        {
            get
            {
                string str;
                if (!string.IsNullOrEmpty(base.Name))
                {
                    return base.Name;
                }
                this.Name = str = ((this.type == GenericParameterType.Method) ? "!!" : "!") + this.position;
                return str;
            }
        }

        public override string Namespace
        {
            get => 
                string.Empty;
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override string FullName =>
            this.Name;

        public override bool IsGenericParameter =>
            true;

        public override bool ContainsGenericParameter =>
            true;

        public override Mono.Cecil.MetadataType MetadataType =>
            ((Mono.Cecil.MetadataType) base.etype);

        public bool IsNonVariant
        {
            get => 
                this.attributes.GetMaskedAttributes(3, 0);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(3, 0, value));
        }

        public bool IsCovariant
        {
            get => 
                this.attributes.GetMaskedAttributes(3, 1);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(3, 1, value));
        }

        public bool IsContravariant
        {
            get => 
                this.attributes.GetMaskedAttributes(3, 2);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(3, 2, value));
        }

        public bool HasReferenceTypeConstraint
        {
            get => 
                this.attributes.GetAttributes(4);
            set => 
                (this.attributes = this.attributes.SetAttributes(4, value));
        }

        public bool HasNotNullableValueTypeConstraint
        {
            get => 
                this.attributes.GetAttributes(8);
            set => 
                (this.attributes = this.attributes.SetAttributes(8, value));
        }

        public bool HasDefaultConstructorConstraint
        {
            get => 
                this.attributes.GetAttributes(0x10);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x10, value));
        }
    }
}

