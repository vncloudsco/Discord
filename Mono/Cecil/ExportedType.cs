namespace Mono.Cecil
{
    using System;

    internal class ExportedType : IMetadataTokenProvider
    {
        private string @namespace;
        private string name;
        private uint attributes;
        private IMetadataScope scope;
        private ModuleDefinition module;
        private int identifier;
        private ExportedType declaring_type;
        internal Mono.Cecil.MetadataToken token;

        public ExportedType(string @namespace, string name, ModuleDefinition module, IMetadataScope scope)
        {
            this.@namespace = @namespace;
            this.name = name;
            this.scope = scope;
            this.module = module;
        }

        internal TypeReference CreateReference() => 
            new TypeReference(this.@namespace, this.name, this.module, this.scope) { DeclaringType = this.declaring_type?.CreateReference() };

        public TypeDefinition Resolve() => 
            this.module.Resolve(this.CreateReference());

        public override string ToString() => 
            this.FullName;

        public string Namespace
        {
            get => 
                this.@namespace;
            set => 
                (this.@namespace = value);
        }

        public string Name
        {
            get => 
                this.name;
            set => 
                (this.name = value);
        }

        public TypeAttributes Attributes
        {
            get => 
                ((TypeAttributes) this.attributes);
            set => 
                (this.attributes = (uint) value);
        }

        public IMetadataScope Scope =>
            ((this.declaring_type == null) ? this.scope : this.declaring_type.Scope);

        public ExportedType DeclaringType
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

        public int Identifier
        {
            get => 
                this.identifier;
            set => 
                (this.identifier = value);
        }

        public bool IsNotPublic
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 0);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 0, value));
        }

        public bool IsPublic
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 1);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 1, value));
        }

        public bool IsNestedPublic
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 2);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 2, value));
        }

        public bool IsNestedPrivate
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 3);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 3, value));
        }

        public bool IsNestedFamily
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 4);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 4, value));
        }

        public bool IsNestedAssembly
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 5);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 5, value));
        }

        public bool IsNestedFamilyAndAssembly
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 6);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 6, value));
        }

        public bool IsNestedFamilyOrAssembly
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 7);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 7, value));
        }

        public bool IsAutoLayout
        {
            get => 
                this.attributes.GetMaskedAttributes(0x18, 0);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x18, 0, value));
        }

        public bool IsSequentialLayout
        {
            get => 
                this.attributes.GetMaskedAttributes(0x18, 8);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x18, 8, value));
        }

        public bool IsExplicitLayout
        {
            get => 
                this.attributes.GetMaskedAttributes(0x18, 0x10);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x18, 0x10, value));
        }

        public bool IsClass
        {
            get => 
                this.attributes.GetMaskedAttributes(0x20, 0);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x20, 0, value));
        }

        public bool IsInterface
        {
            get => 
                this.attributes.GetMaskedAttributes(0x20, 0x20);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x20, 0x20, value));
        }

        public bool IsAbstract
        {
            get => 
                this.attributes.GetAttributes(0x80);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x80, value));
        }

        public bool IsSealed
        {
            get => 
                this.attributes.GetAttributes(0x100);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x100, value));
        }

        public bool IsSpecialName
        {
            get => 
                this.attributes.GetAttributes(0x400);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x400, value));
        }

        public bool IsImport
        {
            get => 
                this.attributes.GetAttributes(0x1000);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x1000, value));
        }

        public bool IsSerializable
        {
            get => 
                this.attributes.GetAttributes(0x2000);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x2000, value));
        }

        public bool IsAnsiClass
        {
            get => 
                this.attributes.GetMaskedAttributes(0x30000, 0);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x30000, 0, value));
        }

        public bool IsUnicodeClass
        {
            get => 
                this.attributes.GetMaskedAttributes(0x30000, 0x10000);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x30000, 0x10000, value));
        }

        public bool IsAutoClass
        {
            get => 
                this.attributes.GetMaskedAttributes(0x30000, 0x20000);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x30000, 0x20000, value));
        }

        public bool IsBeforeFieldInit
        {
            get => 
                this.attributes.GetAttributes(0x100000);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x100000, value));
        }

        public bool IsRuntimeSpecialName
        {
            get => 
                this.attributes.GetAttributes(0x800);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x800, value));
        }

        public bool HasSecurity
        {
            get => 
                this.attributes.GetAttributes(0x40000);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x40000, value));
        }

        public bool IsForwarder
        {
            get => 
                this.attributes.GetAttributes(0x200000);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x200000, value));
        }

        public string FullName
        {
            get
            {
                string str = string.IsNullOrEmpty(this.@namespace) ? this.name : (this.@namespace + '.' + this.name);
                return ((this.declaring_type == null) ? str : (this.declaring_type.FullName + "/" + str));
            }
        }
    }
}

