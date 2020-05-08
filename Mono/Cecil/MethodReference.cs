namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;
    using System.Text;

    internal class MethodReference : MemberReference, IMethodSignature, IGenericParameterProvider, IMetadataTokenProvider, IGenericContext
    {
        internal ParameterDefinitionCollection parameters;
        private Mono.Cecil.MethodReturnType return_type;
        private bool has_this;
        private bool explicit_this;
        private MethodCallingConvention calling_convention;
        internal Collection<GenericParameter> generic_parameters;

        internal MethodReference()
        {
            this.return_type = new Mono.Cecil.MethodReturnType(this);
            base.token = new MetadataToken(TokenType.MemberRef);
        }

        public MethodReference(string name, TypeReference returnType) : base(name)
        {
            if (returnType == null)
            {
                throw new ArgumentNullException("returnType");
            }
            this.return_type = new Mono.Cecil.MethodReturnType(this);
            this.return_type.ReturnType = returnType;
            base.token = new MetadataToken(TokenType.MemberRef);
        }

        public MethodReference(string name, TypeReference returnType, TypeReference declaringType) : this(name, returnType)
        {
            if (declaringType == null)
            {
                throw new ArgumentNullException("declaringType");
            }
            this.DeclaringType = declaringType;
        }

        public virtual MethodReference GetElementMethod() => 
            this;

        public virtual MethodDefinition Resolve()
        {
            ModuleDefinition module = this.Module;
            if (module == null)
            {
                throw new NotSupportedException();
            }
            return module.Resolve(this);
        }

        public virtual bool HasThis
        {
            get => 
                this.has_this;
            set => 
                (this.has_this = value);
        }

        public virtual bool ExplicitThis
        {
            get => 
                this.explicit_this;
            set => 
                (this.explicit_this = value);
        }

        public virtual MethodCallingConvention CallingConvention
        {
            get => 
                this.calling_convention;
            set => 
                (this.calling_convention = value);
        }

        public virtual bool HasParameters =>
            !this.parameters.IsNullOrEmpty<ParameterDefinition>();

        public virtual Collection<ParameterDefinition> Parameters
        {
            get
            {
                if (this.parameters == null)
                {
                    this.parameters = new ParameterDefinitionCollection(this);
                }
                return this.parameters;
            }
        }

        IGenericParameterProvider IGenericContext.Type
        {
            get
            {
                TypeReference declaringType = this.DeclaringType;
                GenericInstanceType type = declaringType as GenericInstanceType;
                return ((type == null) ? declaringType : type.ElementType);
            }
        }

        IGenericParameterProvider IGenericContext.Method =>
            this;

        GenericParameterType IGenericParameterProvider.GenericParameterType =>
            GenericParameterType.Method;

        public virtual bool HasGenericParameters =>
            !this.generic_parameters.IsNullOrEmpty<GenericParameter>();

        public virtual Collection<GenericParameter> GenericParameters
        {
            get
            {
                Collection<GenericParameter> collection;
                if (this.generic_parameters != null)
                {
                    return this.generic_parameters;
                }
                this.generic_parameters = collection = new GenericParameterCollection(this);
                return collection;
            }
        }

        public TypeReference ReturnType
        {
            get
            {
                Mono.Cecil.MethodReturnType methodReturnType = this.MethodReturnType;
                return methodReturnType?.ReturnType;
            }
            set
            {
                Mono.Cecil.MethodReturnType methodReturnType = this.MethodReturnType;
                if (methodReturnType != null)
                {
                    methodReturnType.ReturnType = value;
                }
            }
        }

        public virtual Mono.Cecil.MethodReturnType MethodReturnType
        {
            get => 
                this.return_type;
            set => 
                (this.return_type = value);
        }

        public override string FullName
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(this.ReturnType.FullName).Append(" ").Append(base.MemberFullName());
                this.MethodSignatureFullName(builder);
                return builder.ToString();
            }
        }

        public virtual bool IsGenericInstance =>
            false;

        public override bool ContainsGenericParameter
        {
            get
            {
                if (this.ReturnType.ContainsGenericParameter || base.ContainsGenericParameter)
                {
                    return true;
                }
                Collection<ParameterDefinition> parameters = this.Parameters;
                for (int i = 0; i < parameters.Count; i++)
                {
                    if (parameters[i].ParameterType.ContainsGenericParameter)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}

