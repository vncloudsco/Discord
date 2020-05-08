namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using Mono.Collections.Generic;
    using System;
    using System.Text;

    internal sealed class FunctionPointerType : TypeSpecification, IMethodSignature, IMetadataTokenProvider
    {
        private readonly MethodReference function;

        public FunctionPointerType() : base(null)
        {
            this.function = new MethodReference();
            this.function.Name = "method";
            base.etype = ElementType.FnPtr;
        }

        public override TypeReference GetElementType() => 
            this;

        public override TypeDefinition Resolve() => 
            null;

        public bool HasThis
        {
            get => 
                this.function.HasThis;
            set => 
                (this.function.HasThis = value);
        }

        public bool ExplicitThis
        {
            get => 
                this.function.ExplicitThis;
            set => 
                (this.function.ExplicitThis = value);
        }

        public MethodCallingConvention CallingConvention
        {
            get => 
                this.function.CallingConvention;
            set => 
                (this.function.CallingConvention = value);
        }

        public bool HasParameters =>
            this.function.HasParameters;

        public Collection<ParameterDefinition> Parameters =>
            this.function.Parameters;

        public TypeReference ReturnType
        {
            get => 
                this.function.MethodReturnType.ReturnType;
            set => 
                (this.function.MethodReturnType.ReturnType = value);
        }

        public Mono.Cecil.MethodReturnType MethodReturnType =>
            this.function.MethodReturnType;

        public override string Name
        {
            get => 
                this.function.Name;
            set
            {
                throw new InvalidOperationException();
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

        public override ModuleDefinition Module =>
            this.ReturnType.Module;

        public override IMetadataScope Scope
        {
            get => 
                this.function.ReturnType.Scope;
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override bool IsFunctionPointer =>
            true;

        public override bool ContainsGenericParameter =>
            this.function.ContainsGenericParameter;

        public override string FullName
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(this.function.Name);
                builder.Append(" ");
                builder.Append(this.function.ReturnType.FullName);
                builder.Append(" *");
                this.MethodSignatureFullName(builder);
                return builder.ToString();
            }
        }
    }
}

