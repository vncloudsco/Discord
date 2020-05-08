namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;

    internal abstract class MethodSpecification : MethodReference
    {
        private readonly MethodReference method;

        internal MethodSpecification(MethodReference method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            this.method = method;
            base.token = new MetadataToken(TokenType.MethodSpec);
        }

        public sealed override MethodReference GetElementMethod() => 
            this.method.GetElementMethod();

        public MethodReference ElementMethod =>
            this.method;

        public override string Name
        {
            get => 
                this.method.Name;
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override MethodCallingConvention CallingConvention
        {
            get => 
                this.method.CallingConvention;
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override bool HasThis
        {
            get => 
                this.method.HasThis;
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override bool ExplicitThis
        {
            get => 
                this.method.ExplicitThis;
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override Mono.Cecil.MethodReturnType MethodReturnType
        {
            get => 
                this.method.MethodReturnType;
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override TypeReference DeclaringType
        {
            get => 
                this.method.DeclaringType;
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override ModuleDefinition Module =>
            this.method.Module;

        public override bool HasParameters =>
            this.method.HasParameters;

        public override Collection<ParameterDefinition> Parameters =>
            this.method.Parameters;

        public override bool ContainsGenericParameter =>
            this.method.ContainsGenericParameter;
    }
}

