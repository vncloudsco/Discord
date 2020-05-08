namespace Mono.Cecil
{
    using System;

    internal sealed class AssemblyLinkedResource : Resource
    {
        private AssemblyNameReference reference;

        public AssemblyLinkedResource(string name, ManifestResourceAttributes flags) : base(name, flags)
        {
        }

        public AssemblyLinkedResource(string name, ManifestResourceAttributes flags, AssemblyNameReference reference) : base(name, flags)
        {
            this.reference = reference;
        }

        public AssemblyNameReference Assembly
        {
            get => 
                this.reference;
            set => 
                (this.reference = value);
        }

        public override Mono.Cecil.ResourceType ResourceType =>
            Mono.Cecil.ResourceType.AssemblyLinked;
    }
}

