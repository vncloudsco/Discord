namespace Mono.Cecil
{
    using System;

    internal abstract class Resource
    {
        private string name;
        private uint attributes;

        internal Resource(string name, ManifestResourceAttributes attributes)
        {
            this.name = name;
            this.attributes = (uint) attributes;
        }

        public string Name
        {
            get => 
                this.name;
            set => 
                (this.name = value);
        }

        public ManifestResourceAttributes Attributes
        {
            get => 
                ((ManifestResourceAttributes) this.attributes);
            set => 
                (this.attributes = (uint) value);
        }

        public abstract Mono.Cecil.ResourceType ResourceType { get; }

        public bool IsPublic
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 1);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 1, value));
        }

        public bool IsPrivate
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 2);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 2, value));
        }
    }
}

