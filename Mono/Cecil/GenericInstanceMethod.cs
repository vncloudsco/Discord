namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;
    using System.Text;

    internal sealed class GenericInstanceMethod : MethodSpecification, IGenericInstance, IMetadataTokenProvider, IGenericContext
    {
        private Collection<TypeReference> arguments;

        public GenericInstanceMethod(MethodReference method) : base(method)
        {
        }

        public bool HasGenericArguments =>
            !this.arguments.IsNullOrEmpty<TypeReference>();

        public Collection<TypeReference> GenericArguments
        {
            get
            {
                Collection<TypeReference> arguments = this.arguments;
                if (this.arguments == null)
                {
                    Collection<TypeReference> local1 = this.arguments;
                    arguments = this.arguments = new Collection<TypeReference>();
                }
                return arguments;
            }
        }

        public override bool IsGenericInstance =>
            true;

        IGenericParameterProvider IGenericContext.Method =>
            base.ElementMethod;

        IGenericParameterProvider IGenericContext.Type =>
            base.ElementMethod.DeclaringType;

        public override bool ContainsGenericParameter =>
            (this.ContainsGenericParameter() || base.ContainsGenericParameter);

        public override string FullName
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                MethodReference elementMethod = base.ElementMethod;
                builder.Append(elementMethod.ReturnType.FullName).Append(" ").Append(elementMethod.DeclaringType.FullName).Append("::").Append(elementMethod.Name);
                this.GenericInstanceFullName(builder);
                this.MethodSignatureFullName(builder);
                return builder.ToString();
            }
        }
    }
}

