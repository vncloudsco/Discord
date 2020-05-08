namespace Mono.Cecil
{
    using System;

    internal sealed class LinkedResource : Resource
    {
        internal byte[] hash;
        private string file;

        public LinkedResource(string name, ManifestResourceAttributes flags) : base(name, flags)
        {
        }

        public LinkedResource(string name, ManifestResourceAttributes flags, string file) : base(name, flags)
        {
            this.file = file;
        }

        public byte[] Hash =>
            this.hash;

        public string File
        {
            get => 
                this.file;
            set => 
                (this.file = value);
        }

        public override Mono.Cecil.ResourceType ResourceType =>
            Mono.Cecil.ResourceType.Linked;
    }
}

