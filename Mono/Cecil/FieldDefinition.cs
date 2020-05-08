namespace Mono.Cecil
{
    using Mono;
    using Mono.Collections.Generic;
    using System;

    internal sealed class FieldDefinition : FieldReference, IMemberDefinition, ICustomAttributeProvider, IConstantProvider, IMarshalInfoProvider, IMetadataTokenProvider
    {
        private ushort attributes;
        private Collection<CustomAttribute> custom_attributes;
        private int offset;
        internal int rva;
        private byte[] initial_value;
        private object constant;
        private Mono.Cecil.MarshalInfo marshal_info;

        public FieldDefinition(string name, FieldAttributes attributes, TypeReference fieldType) : base(name, fieldType)
        {
            this.offset = -2;
            this.rva = -2;
            this.constant = Mixin.NotResolved;
            this.attributes = (ushort) attributes;
        }

        public override FieldDefinition Resolve() => 
            this;

        private void ResolveLayout()
        {
            if (this.offset == -2)
            {
                if (!base.HasImage)
                {
                    this.offset = -1;
                }
                else
                {
                    this.offset = this.Module.Read<FieldDefinition, int>(this, (field, reader) => reader.ReadFieldLayout(field));
                }
            }
        }

        private void ResolveRVA()
        {
            if ((this.rva == -2) && base.HasImage)
            {
                this.rva = this.Module.Read<FieldDefinition, int>(this, (field, reader) => reader.ReadFieldRVA(field));
            }
        }

        public bool HasLayoutInfo
        {
            get
            {
                if (this.offset >= 0)
                {
                    return true;
                }
                this.ResolveLayout();
                return (this.offset >= 0);
            }
        }

        public int Offset
        {
            get
            {
                if (this.offset >= 0)
                {
                    return this.offset;
                }
                this.ResolveLayout();
                return ((this.offset >= 0) ? this.offset : -1);
            }
            set => 
                (this.offset = value);
        }

        public int RVA
        {
            get
            {
                if (this.rva > 0)
                {
                    return this.rva;
                }
                this.ResolveRVA();
                return ((this.rva > 0) ? this.rva : 0);
            }
        }

        public byte[] InitialValue
        {
            get
            {
                if (this.initial_value == null)
                {
                    this.ResolveRVA();
                    if (this.initial_value == null)
                    {
                        this.initial_value = Empty<byte>.Array;
                    }
                }
                return this.initial_value;
            }
            set
            {
                this.initial_value = value;
                this.rva = 0;
            }
        }

        public FieldAttributes Attributes
        {
            get => 
                ((FieldAttributes) this.attributes);
            set => 
                (this.attributes = (ushort) value);
        }

        public bool HasConstant
        {
            get
            {
                this.ResolveConstant(ref this.constant, this.Module);
                return (this.constant != Mixin.NoValue);
            }
            set
            {
                if (!value)
                {
                    this.constant = Mixin.NoValue;
                }
            }
        }

        public object Constant
        {
            get => 
                (this.HasConstant ? this.constant : null);
            set => 
                (this.constant = value);
        }

        public bool HasCustomAttributes =>
            ((this.custom_attributes == null) ? this.GetHasCustomAttributes(this.Module) : (this.custom_attributes.Count > 0));

        public Collection<CustomAttribute> CustomAttributes =>
            (this.custom_attributes ?? this.GetCustomAttributes(ref this.custom_attributes, this.Module));

        public bool HasMarshalInfo =>
            ((this.marshal_info == null) ? this.GetHasMarshalInfo(this.Module) : true);

        public Mono.Cecil.MarshalInfo MarshalInfo
        {
            get => 
                (this.marshal_info ?? this.GetMarshalInfo(ref this.marshal_info, this.Module));
            set => 
                (this.marshal_info = value);
        }

        public bool IsCompilerControlled
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 0);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 0, value));
        }

        public bool IsPrivate
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 1);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 1, value));
        }

        public bool IsFamilyAndAssembly
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 2);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 2, value));
        }

        public bool IsAssembly
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 3);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 3, value));
        }

        public bool IsFamily
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 4);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 4, value));
        }

        public bool IsFamilyOrAssembly
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 5);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 5, value));
        }

        public bool IsPublic
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 6);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 6, value));
        }

        public bool IsStatic
        {
            get => 
                this.attributes.GetAttributes(0x10);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x10, value));
        }

        public bool IsInitOnly
        {
            get => 
                this.attributes.GetAttributes(0x20);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x20, value));
        }

        public bool IsLiteral
        {
            get => 
                this.attributes.GetAttributes(0x40);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x40, value));
        }

        public bool IsNotSerialized
        {
            get => 
                this.attributes.GetAttributes(0x80);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x80, value));
        }

        public bool IsSpecialName
        {
            get => 
                this.attributes.GetAttributes(0x200);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x200, value));
        }

        public bool IsPInvokeImpl
        {
            get => 
                this.attributes.GetAttributes(0x2000);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x2000, value));
        }

        public bool IsRuntimeSpecialName
        {
            get => 
                this.attributes.GetAttributes(0x400);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x400, value));
        }

        public bool HasDefault
        {
            get => 
                this.attributes.GetAttributes(0x8000);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x8000, value));
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

