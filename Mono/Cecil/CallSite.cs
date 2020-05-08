namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;
    using System.Text;

    internal sealed class CallSite : IMethodSignature, IMetadataTokenProvider
    {
        private readonly MethodReference signature;

        internal CallSite()
        {
            this.signature = new MethodReference();
            this.signature.token = new Mono.Cecil.MetadataToken(TokenType.Signature, 0);
        }

        public CallSite(TypeReference returnType) : this()
        {
            if (returnType == null)
            {
                throw new ArgumentNullException("returnType");
            }
            this.signature.ReturnType = returnType;
        }

        public override string ToString() => 
            this.FullName;

        public bool HasThis
        {
            get => 
                this.signature.HasThis;
            set => 
                (this.signature.HasThis = value);
        }

        public bool ExplicitThis
        {
            get => 
                this.signature.ExplicitThis;
            set => 
                (this.signature.ExplicitThis = value);
        }

        public MethodCallingConvention CallingConvention
        {
            get => 
                this.signature.CallingConvention;
            set => 
                (this.signature.CallingConvention = value);
        }

        public bool HasParameters =>
            this.signature.HasParameters;

        public Collection<ParameterDefinition> Parameters =>
            this.signature.Parameters;

        public TypeReference ReturnType
        {
            get => 
                this.signature.MethodReturnType.ReturnType;
            set => 
                (this.signature.MethodReturnType.ReturnType = value);
        }

        public Mono.Cecil.MethodReturnType MethodReturnType =>
            this.signature.MethodReturnType;

        public string Name
        {
            get => 
                string.Empty;
            set
            {
                throw new InvalidOperationException();
            }
        }

        public string Namespace
        {
            get => 
                string.Empty;
            set
            {
                throw new InvalidOperationException();
            }
        }

        public ModuleDefinition Module =>
            this.ReturnType.Module;

        public IMetadataScope Scope =>
            this.signature.ReturnType.Scope;

        public Mono.Cecil.MetadataToken MetadataToken
        {
            get => 
                this.signature.token;
            set => 
                (this.signature.token = value);
        }

        public string FullName
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(this.ReturnType.FullName);
                this.MethodSignatureFullName(builder);
                return builder.ToString();
            }
        }
    }
}

