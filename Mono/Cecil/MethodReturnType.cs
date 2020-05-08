namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;
    using System.Threading;

    internal sealed class MethodReturnType : IConstantProvider, ICustomAttributeProvider, IMarshalInfoProvider, IMetadataTokenProvider
    {
        internal IMethodSignature method;
        internal ParameterDefinition parameter;
        private TypeReference return_type;

        public MethodReturnType(IMethodSignature method)
        {
            this.method = method;
        }

        public IMethodSignature Method =>
            this.method;

        public TypeReference ReturnType
        {
            get => 
                this.return_type;
            set => 
                (this.return_type = value);
        }

        internal ParameterDefinition Parameter
        {
            get
            {
                if (this.parameter == null)
                {
                    Interlocked.CompareExchange<ParameterDefinition>(ref this.parameter, new ParameterDefinition(this.return_type, this.method), null);
                }
                return this.parameter;
            }
        }

        public Mono.Cecil.MetadataToken MetadataToken
        {
            get => 
                this.Parameter.MetadataToken;
            set => 
                (this.Parameter.MetadataToken = value);
        }

        public ParameterAttributes Attributes
        {
            get => 
                this.Parameter.Attributes;
            set => 
                (this.Parameter.Attributes = value);
        }

        public bool HasCustomAttributes =>
            ((this.parameter != null) && this.parameter.HasCustomAttributes);

        public Collection<CustomAttribute> CustomAttributes =>
            this.Parameter.CustomAttributes;

        public bool HasDefault
        {
            get => 
                ((this.parameter != null) && this.parameter.HasDefault);
            set => 
                (this.Parameter.HasDefault = value);
        }

        public bool HasConstant
        {
            get => 
                ((this.parameter != null) && this.parameter.HasConstant);
            set => 
                (this.Parameter.HasConstant = value);
        }

        public object Constant
        {
            get => 
                this.Parameter.Constant;
            set => 
                (this.Parameter.Constant = value);
        }

        public bool HasFieldMarshal
        {
            get => 
                ((this.parameter != null) && this.parameter.HasFieldMarshal);
            set => 
                (this.Parameter.HasFieldMarshal = value);
        }

        public bool HasMarshalInfo =>
            ((this.parameter != null) && this.parameter.HasMarshalInfo);

        public Mono.Cecil.MarshalInfo MarshalInfo
        {
            get => 
                this.Parameter.MarshalInfo;
            set => 
                (this.Parameter.MarshalInfo = value);
        }
    }
}

