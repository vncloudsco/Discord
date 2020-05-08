namespace Mono.Cecil
{
    using System;

    internal abstract class MemberReference : IMetadataTokenProvider
    {
        private string name;
        private TypeReference declaring_type;
        internal Mono.Cecil.MetadataToken token;

        internal MemberReference()
        {
        }

        internal MemberReference(string name)
        {
            this.name = name ?? string.Empty;
        }

        internal string MemberFullName() => 
            ((this.declaring_type != null) ? (this.declaring_type.FullName + "::" + this.name) : this.name);

        public override string ToString() => 
            this.FullName;

        public virtual string Name
        {
            get => 
                this.name;
            set => 
                (this.name = value);
        }

        public abstract string FullName { get; }

        public virtual TypeReference DeclaringType
        {
            get => 
                this.declaring_type;
            set => 
                (this.declaring_type = value);
        }

        public Mono.Cecil.MetadataToken MetadataToken
        {
            get => 
                this.token;
            set => 
                (this.token = value);
        }

        internal bool HasImage
        {
            get
            {
                ModuleDefinition module = this.Module;
                return ((module != null) ? module.HasImage : false);
            }
        }

        public virtual ModuleDefinition Module =>
            this.declaring_type?.Module;

        public virtual bool IsDefinition =>
            false;

        public virtual bool ContainsGenericParameter =>
            ((this.declaring_type != null) && this.declaring_type.ContainsGenericParameter);
    }
}

