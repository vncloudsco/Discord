﻿namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;

    internal sealed class ParameterDefinition : ParameterReference, ICustomAttributeProvider, IConstantProvider, IMarshalInfoProvider, IMetadataTokenProvider
    {
        private ushort attributes;
        internal IMethodSignature method;
        private object constant;
        private Collection<CustomAttribute> custom_attributes;
        private Mono.Cecil.MarshalInfo marshal_info;

        public ParameterDefinition(TypeReference parameterType) : this(string.Empty, ParameterAttributes.None, parameterType)
        {
        }

        internal ParameterDefinition(TypeReference parameterType, IMethodSignature method) : this(string.Empty, ParameterAttributes.None, parameterType)
        {
            this.method = method;
        }

        public ParameterDefinition(string name, ParameterAttributes attributes, TypeReference parameterType) : base(name, parameterType)
        {
            this.constant = Mixin.NotResolved;
            this.attributes = (ushort) attributes;
            base.token = new MetadataToken(TokenType.Param);
        }

        public override ParameterDefinition Resolve() => 
            this;

        public ParameterAttributes Attributes
        {
            get => 
                ((ParameterAttributes) this.attributes);
            set => 
                (this.attributes = (ushort) value);
        }

        public IMethodSignature Method =>
            this.method;

        public int Sequence =>
            ((this.method != null) ? (this.method.HasImplicitThis() ? (base.index + 1) : base.index) : -1);

        public bool HasConstant
        {
            get
            {
                this.ResolveConstant(ref this.constant, base.parameter_type.Module);
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
            ((this.custom_attributes == null) ? this.GetHasCustomAttributes(base.parameter_type.Module) : (this.custom_attributes.Count > 0));

        public Collection<CustomAttribute> CustomAttributes =>
            (this.custom_attributes ?? this.GetCustomAttributes(ref this.custom_attributes, base.parameter_type.Module));

        public bool HasMarshalInfo =>
            ((this.marshal_info == null) ? this.GetHasMarshalInfo(base.parameter_type.Module) : true);

        public Mono.Cecil.MarshalInfo MarshalInfo
        {
            get => 
                (this.marshal_info ?? this.GetMarshalInfo(ref this.marshal_info, base.parameter_type.Module));
            set => 
                (this.marshal_info = value);
        }

        public bool IsIn
        {
            get => 
                this.attributes.GetAttributes(1);
            set => 
                (this.attributes = this.attributes.SetAttributes(1, value));
        }

        public bool IsOut
        {
            get => 
                this.attributes.GetAttributes(2);
            set => 
                (this.attributes = this.attributes.SetAttributes(2, value));
        }

        public bool IsLcid
        {
            get => 
                this.attributes.GetAttributes(4);
            set => 
                (this.attributes = this.attributes.SetAttributes(4, value));
        }

        public bool IsReturnValue
        {
            get => 
                this.attributes.GetAttributes(8);
            set => 
                (this.attributes = this.attributes.SetAttributes(8, value));
        }

        public bool IsOptional
        {
            get => 
                this.attributes.GetAttributes(0x10);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x10, value));
        }

        public bool HasDefault
        {
            get => 
                this.attributes.GetAttributes(0x1000);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x1000, value));
        }

        public bool HasFieldMarshal
        {
            get => 
                this.attributes.GetAttributes(0x2000);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x2000, value));
        }
    }
}

