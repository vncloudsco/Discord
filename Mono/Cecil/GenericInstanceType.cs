namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using Mono.Collections.Generic;
    using System;
    using System.Text;

    internal sealed class GenericInstanceType : TypeSpecification, IGenericInstance, IMetadataTokenProvider, IGenericContext
    {
        private Collection<TypeReference> arguments;

        public GenericInstanceType(TypeReference type) : base(type)
        {
            base.IsValueType = type.IsValueType;
            base.etype = ElementType.GenericInst;
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

        public override TypeReference DeclaringType
        {
            get => 
                base.ElementType.DeclaringType;
            set
            {
                throw new NotSupportedException();
            }
        }

        public override string FullName
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(base.FullName);
                this.GenericInstanceFullName(builder);
                return builder.ToString();
            }
        }

        public override bool IsGenericInstance =>
            true;

        public override bool ContainsGenericParameter =>
            (this.ContainsGenericParameter() || base.ContainsGenericParameter);

        IGenericParameterProvider IGenericContext.Type =>
            base.ElementType;
    }
}

