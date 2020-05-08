namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;

    internal sealed class SecurityDeclaration
    {
        internal readonly uint signature;
        private byte[] blob;
        private readonly ModuleDefinition module;
        internal bool resolved;
        private SecurityAction action;
        internal Collection<SecurityAttribute> security_attributes;

        public SecurityDeclaration(SecurityAction action)
        {
            this.action = action;
            this.resolved = true;
        }

        public SecurityDeclaration(SecurityAction action, byte[] blob)
        {
            this.action = action;
            this.resolved = false;
            this.blob = blob;
        }

        internal SecurityDeclaration(SecurityAction action, uint signature, ModuleDefinition module)
        {
            this.action = action;
            this.signature = signature;
            this.module = module;
        }

        public byte[] GetBlob()
        {
            byte[] buffer;
            if (this.blob != null)
            {
                return this.blob;
            }
            if (!this.HasImage || (this.signature == 0))
            {
                throw new NotSupportedException();
            }
            this.blob = buffer = this.module.Read<SecurityDeclaration, byte[]>(this, (declaration, reader) => reader.ReadSecurityDeclarationBlob(declaration.signature));
            return buffer;
        }

        private void Resolve()
        {
            if (!this.resolved && this.HasImage)
            {
                this.module.Read<SecurityDeclaration, SecurityDeclaration>(this, delegate (SecurityDeclaration declaration, MetadataReader reader) {
                    reader.ReadSecurityDeclarationSignature(declaration);
                    return this;
                });
                this.resolved = true;
            }
        }

        public SecurityAction Action
        {
            get => 
                this.action;
            set => 
                (this.action = value);
        }

        public bool HasSecurityAttributes
        {
            get
            {
                this.Resolve();
                return !this.security_attributes.IsNullOrEmpty<SecurityAttribute>();
            }
        }

        public Collection<SecurityAttribute> SecurityAttributes
        {
            get
            {
                this.Resolve();
                Collection<SecurityAttribute> collection2 = this.security_attributes;
                if (this.security_attributes == null)
                {
                    Collection<SecurityAttribute> local1 = this.security_attributes;
                    collection2 = this.security_attributes = new Collection<SecurityAttribute>();
                }
                return collection2;
            }
        }

        internal bool HasImage =>
            ((this.module != null) && this.module.HasImage);
    }
}

