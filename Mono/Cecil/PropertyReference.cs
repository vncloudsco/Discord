namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;

    internal abstract class PropertyReference : MemberReference
    {
        private TypeReference property_type;

        internal PropertyReference(string name, TypeReference propertyType) : base(name)
        {
            if (propertyType == null)
            {
                throw new ArgumentNullException("propertyType");
            }
            this.property_type = propertyType;
        }

        public abstract PropertyDefinition Resolve();

        public TypeReference PropertyType
        {
            get => 
                this.property_type;
            set => 
                (this.property_type = value);
        }

        public abstract Collection<ParameterDefinition> Parameters { get; }
    }
}

