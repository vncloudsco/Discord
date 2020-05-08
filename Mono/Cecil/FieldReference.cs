namespace Mono.Cecil
{
    using System;

    internal class FieldReference : MemberReference
    {
        private TypeReference field_type;

        internal FieldReference()
        {
            base.token = new MetadataToken(TokenType.MemberRef);
        }

        public FieldReference(string name, TypeReference fieldType) : base(name)
        {
            if (fieldType == null)
            {
                throw new ArgumentNullException("fieldType");
            }
            this.field_type = fieldType;
            base.token = new MetadataToken(TokenType.MemberRef);
        }

        public FieldReference(string name, TypeReference fieldType, TypeReference declaringType) : this(name, fieldType)
        {
            if (declaringType == null)
            {
                throw new ArgumentNullException("declaringType");
            }
            this.DeclaringType = declaringType;
        }

        public virtual FieldDefinition Resolve()
        {
            ModuleDefinition module = this.Module;
            if (module == null)
            {
                throw new NotSupportedException();
            }
            return module.Resolve(this);
        }

        public TypeReference FieldType
        {
            get => 
                this.field_type;
            set => 
                (this.field_type = value);
        }

        public override string FullName =>
            (this.field_type.FullName + " " + base.MemberFullName());

        public override bool ContainsGenericParameter =>
            (this.field_type.ContainsGenericParameter || base.ContainsGenericParameter);
    }
}

