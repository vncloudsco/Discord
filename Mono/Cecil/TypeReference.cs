namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using Mono.Collections.Generic;
    using System;

    internal class TypeReference : MemberReference, IGenericParameterProvider, IMetadataTokenProvider, IGenericContext
    {
        private string @namespace;
        private bool value_type;
        internal IMetadataScope scope;
        internal ModuleDefinition module;
        internal ElementType etype;
        private string fullname;
        protected Collection<GenericParameter> generic_parameters;

        protected TypeReference(string @namespace, string name) : base(name)
        {
            this.@namespace = @namespace ?? string.Empty;
            base.token = new MetadataToken(TokenType.TypeRef, 0);
        }

        public TypeReference(string @namespace, string name, ModuleDefinition module, IMetadataScope scope) : this(@namespace, name)
        {
            this.module = module;
            this.scope = scope;
        }

        public TypeReference(string @namespace, string name, ModuleDefinition module, IMetadataScope scope, bool valueType) : this(@namespace, name, module, scope)
        {
            this.value_type = valueType;
        }

        public virtual TypeReference GetElementType() => 
            this;

        public virtual TypeDefinition Resolve()
        {
            ModuleDefinition module = this.Module;
            if (module == null)
            {
                throw new NotSupportedException();
            }
            return module.Resolve(this);
        }

        public override string Name
        {
            get => 
                base.Name;
            set
            {
                base.Name = value;
                this.fullname = null;
            }
        }

        public virtual string Namespace
        {
            get => 
                this.@namespace;
            set
            {
                this.@namespace = value;
                this.fullname = null;
            }
        }

        public virtual bool IsValueType
        {
            get => 
                this.value_type;
            set => 
                (this.value_type = value);
        }

        public override ModuleDefinition Module
        {
            get
            {
                if (this.module != null)
                {
                    return this.module;
                }
                TypeReference declaringType = this.DeclaringType;
                return declaringType?.Module;
            }
        }

        IGenericParameterProvider IGenericContext.Type =>
            this;

        IGenericParameterProvider IGenericContext.Method =>
            null;

        GenericParameterType IGenericParameterProvider.GenericParameterType =>
            GenericParameterType.Type;

        public virtual bool HasGenericParameters =>
            !this.generic_parameters.IsNullOrEmpty<GenericParameter>();

        public virtual Collection<GenericParameter> GenericParameters
        {
            get
            {
                Collection<GenericParameter> collection;
                if (this.generic_parameters != null)
                {
                    return this.generic_parameters;
                }
                this.generic_parameters = collection = new GenericParameterCollection(this);
                return collection;
            }
        }

        public virtual IMetadataScope Scope
        {
            get
            {
                TypeReference declaringType = this.DeclaringType;
                return ((declaringType == null) ? this.scope : declaringType.Scope);
            }
            set
            {
                TypeReference declaringType = this.DeclaringType;
                if (declaringType != null)
                {
                    declaringType.Scope = value;
                }
                else
                {
                    this.scope = value;
                }
            }
        }

        public bool IsNested =>
            !ReferenceEquals(this.DeclaringType, null);

        public override TypeReference DeclaringType
        {
            get => 
                base.DeclaringType;
            set
            {
                base.DeclaringType = value;
                this.fullname = null;
            }
        }

        public override string FullName
        {
            get
            {
                if (this.fullname == null)
                {
                    this.fullname = this.TypeFullName();
                    if (this.IsNested)
                    {
                        this.fullname = this.DeclaringType.FullName + "/" + this.fullname;
                    }
                }
                return this.fullname;
            }
        }

        public virtual bool IsByReference =>
            false;

        public virtual bool IsPointer =>
            false;

        public virtual bool IsSentinel =>
            false;

        public virtual bool IsArray =>
            false;

        public virtual bool IsGenericParameter =>
            false;

        public virtual bool IsGenericInstance =>
            false;

        public virtual bool IsRequiredModifier =>
            false;

        public virtual bool IsOptionalModifier =>
            false;

        public virtual bool IsPinned =>
            false;

        public virtual bool IsFunctionPointer =>
            false;

        public virtual bool IsPrimitive =>
            this.etype.IsPrimitive();

        public virtual Mono.Cecil.MetadataType MetadataType =>
            ((this.etype != ElementType.None) ? ((Mono.Cecil.MetadataType) this.etype) : (this.IsValueType ? Mono.Cecil.MetadataType.ValueType : Mono.Cecil.MetadataType.Class));
    }
}

