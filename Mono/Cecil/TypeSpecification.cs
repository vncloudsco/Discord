namespace Mono.Cecil
{
    using System;

    internal abstract class TypeSpecification : TypeReference
    {
        private readonly TypeReference element_type;

        internal TypeSpecification(TypeReference type) : base(null, null)
        {
            this.element_type = type;
            base.token = new MetadataToken(TokenType.TypeSpec);
        }

        public override TypeReference GetElementType() => 
            this.element_type.GetElementType();

        public TypeReference ElementType =>
            this.element_type;

        public override string Name
        {
            get => 
                this.element_type.Name;
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override string Namespace
        {
            get => 
                this.element_type.Namespace;
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override IMetadataScope Scope
        {
            get => 
                this.element_type.Scope;
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override ModuleDefinition Module =>
            this.element_type.Module;

        public override string FullName =>
            this.element_type.FullName;

        public override bool ContainsGenericParameter =>
            this.element_type.ContainsGenericParameter;

        public override Mono.Cecil.MetadataType MetadataType =>
            ((Mono.Cecil.MetadataType) base.etype);
    }
}

