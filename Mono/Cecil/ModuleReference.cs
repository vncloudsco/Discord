namespace Mono.Cecil
{
    using System;

    internal class ModuleReference : IMetadataScope, IMetadataTokenProvider
    {
        private string name;
        internal Mono.Cecil.MetadataToken token;

        internal ModuleReference()
        {
            this.token = new Mono.Cecil.MetadataToken(TokenType.ModuleRef);
        }

        public ModuleReference(string name) : this()
        {
            this.name = name;
        }

        public override string ToString() => 
            this.name;

        public string Name
        {
            get => 
                this.name;
            set => 
                (this.name = value);
        }

        public virtual Mono.Cecil.MetadataScopeType MetadataScopeType =>
            Mono.Cecil.MetadataScopeType.ModuleReference;

        public Mono.Cecil.MetadataToken MetadataToken
        {
            get => 
                this.token;
            set => 
                (this.token = value);
        }
    }
}

