namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;

    internal sealed class SecurityAttribute : ICustomAttribute
    {
        private TypeReference attribute_type;
        internal Collection<CustomAttributeNamedArgument> fields;
        internal Collection<CustomAttributeNamedArgument> properties;

        public SecurityAttribute(TypeReference attributeType)
        {
            this.attribute_type = attributeType;
        }

        public TypeReference AttributeType
        {
            get => 
                this.attribute_type;
            set => 
                (this.attribute_type = value);
        }

        public bool HasFields =>
            !this.fields.IsNullOrEmpty<CustomAttributeNamedArgument>();

        public Collection<CustomAttributeNamedArgument> Fields
        {
            get
            {
                Collection<CustomAttributeNamedArgument> fields = this.fields;
                if (this.fields == null)
                {
                    Collection<CustomAttributeNamedArgument> local1 = this.fields;
                    fields = this.fields = new Collection<CustomAttributeNamedArgument>();
                }
                return fields;
            }
        }

        public bool HasProperties =>
            !this.properties.IsNullOrEmpty<CustomAttributeNamedArgument>();

        public Collection<CustomAttributeNamedArgument> Properties
        {
            get
            {
                Collection<CustomAttributeNamedArgument> properties = this.properties;
                if (this.properties == null)
                {
                    Collection<CustomAttributeNamedArgument> local1 = this.properties;
                    properties = this.properties = new Collection<CustomAttributeNamedArgument>();
                }
                return properties;
            }
        }
    }
}

