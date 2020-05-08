namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;

    internal sealed class CustomAttribute : ICustomAttribute
    {
        internal readonly uint signature;
        internal bool resolved;
        private MethodReference constructor;
        private byte[] blob;
        internal Collection<CustomAttributeArgument> arguments;
        internal Collection<CustomAttributeNamedArgument> fields;
        internal Collection<CustomAttributeNamedArgument> properties;

        public CustomAttribute(MethodReference constructor)
        {
            this.constructor = constructor;
            this.resolved = true;
        }

        public CustomAttribute(MethodReference constructor, byte[] blob)
        {
            this.constructor = constructor;
            this.resolved = false;
            this.blob = blob;
        }

        internal CustomAttribute(uint signature, MethodReference constructor)
        {
            this.signature = signature;
            this.constructor = constructor;
            this.resolved = false;
        }

        public byte[] GetBlob()
        {
            if (this.blob != null)
            {
                return this.blob;
            }
            if (!this.HasImage)
            {
                throw new NotSupportedException();
            }
            return this.Module.Read<CustomAttribute, byte[]>(ref this.blob, this, (attribute, reader) => reader.ReadCustomAttributeBlob(attribute.signature));
        }

        private void Resolve()
        {
            if (!this.resolved && this.HasImage)
            {
                this.Module.Read<CustomAttribute, CustomAttribute>(this, delegate (CustomAttribute attribute, MetadataReader reader) {
                    try
                    {
                        reader.ReadCustomAttributeSignature(attribute);
                        this.resolved = true;
                    }
                    catch (ResolutionException)
                    {
                        if (this.arguments != null)
                        {
                            this.arguments.Clear();
                        }
                        if (this.fields != null)
                        {
                            this.fields.Clear();
                        }
                        if (this.properties != null)
                        {
                            this.properties.Clear();
                        }
                        this.resolved = false;
                    }
                    return this;
                });
            }
        }

        public MethodReference Constructor
        {
            get => 
                this.constructor;
            set => 
                (this.constructor = value);
        }

        public TypeReference AttributeType =>
            this.constructor.DeclaringType;

        public bool IsResolved =>
            this.resolved;

        public bool HasConstructorArguments
        {
            get
            {
                this.Resolve();
                return !this.arguments.IsNullOrEmpty<CustomAttributeArgument>();
            }
        }

        public Collection<CustomAttributeArgument> ConstructorArguments
        {
            get
            {
                this.Resolve();
                Collection<CustomAttributeArgument> arguments = this.arguments;
                if (this.arguments == null)
                {
                    Collection<CustomAttributeArgument> local1 = this.arguments;
                    arguments = this.arguments = new Collection<CustomAttributeArgument>();
                }
                return arguments;
            }
        }

        public bool HasFields
        {
            get
            {
                this.Resolve();
                return !this.fields.IsNullOrEmpty<CustomAttributeNamedArgument>();
            }
        }

        public Collection<CustomAttributeNamedArgument> Fields
        {
            get
            {
                this.Resolve();
                Collection<CustomAttributeNamedArgument> fields = this.fields;
                if (this.fields == null)
                {
                    Collection<CustomAttributeNamedArgument> local1 = this.fields;
                    fields = this.fields = new Collection<CustomAttributeNamedArgument>();
                }
                return fields;
            }
        }

        public bool HasProperties
        {
            get
            {
                this.Resolve();
                return !this.properties.IsNullOrEmpty<CustomAttributeNamedArgument>();
            }
        }

        public Collection<CustomAttributeNamedArgument> Properties
        {
            get
            {
                this.Resolve();
                Collection<CustomAttributeNamedArgument> properties = this.properties;
                if (this.properties == null)
                {
                    Collection<CustomAttributeNamedArgument> local1 = this.properties;
                    properties = this.properties = new Collection<CustomAttributeNamedArgument>();
                }
                return properties;
            }
        }

        internal bool HasImage =>
            ((this.constructor != null) && this.constructor.HasImage);

        internal ModuleDefinition Module =>
            this.constructor.Module;
    }
}

