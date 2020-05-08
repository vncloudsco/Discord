namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;

    internal sealed class EventDefinition : EventReference, IMemberDefinition, ICustomAttributeProvider, IMetadataTokenProvider
    {
        private ushort attributes;
        private Collection<CustomAttribute> custom_attributes;
        internal MethodDefinition add_method;
        internal MethodDefinition invoke_method;
        internal MethodDefinition remove_method;
        internal Collection<MethodDefinition> other_methods;

        public EventDefinition(string name, EventAttributes attributes, TypeReference eventType) : base(name, eventType)
        {
            this.attributes = (ushort) attributes;
            base.token = new MetadataToken(TokenType.Event);
        }

        private void InitializeMethods()
        {
            ModuleDefinition module = this.Module;
            if (module != null)
            {
                lock (module.SyncRoot)
                {
                    if (((this.add_method == null) && ((this.invoke_method == null) && (this.remove_method == null))) && module.HasImage())
                    {
                        module.Read<EventDefinition, EventDefinition>(this, (@event, reader) => reader.ReadMethods(@event));
                    }
                }
            }
        }

        public override EventDefinition Resolve() => 
            this;

        public EventAttributes Attributes
        {
            get => 
                ((EventAttributes) this.attributes);
            set => 
                (this.attributes = (ushort) value);
        }

        public MethodDefinition AddMethod
        {
            get
            {
                if (this.add_method == null)
                {
                    this.InitializeMethods();
                }
                return this.add_method;
            }
            set => 
                (this.add_method = value);
        }

        public MethodDefinition InvokeMethod
        {
            get
            {
                if (this.invoke_method == null)
                {
                    this.InitializeMethods();
                }
                return this.invoke_method;
            }
            set => 
                (this.invoke_method = value);
        }

        public MethodDefinition RemoveMethod
        {
            get
            {
                if (this.remove_method == null)
                {
                    this.InitializeMethods();
                }
                return this.remove_method;
            }
            set => 
                (this.remove_method = value);
        }

        public bool HasOtherMethods
        {
            get
            {
                if (this.other_methods != null)
                {
                    return (this.other_methods.Count > 0);
                }
                this.InitializeMethods();
                return !this.other_methods.IsNullOrEmpty<MethodDefinition>();
            }
        }

        public Collection<MethodDefinition> OtherMethods
        {
            get
            {
                Collection<MethodDefinition> collection;
                if (this.other_methods != null)
                {
                    return this.other_methods;
                }
                this.InitializeMethods();
                if (this.other_methods != null)
                {
                    return this.other_methods;
                }
                this.other_methods = collection = new Collection<MethodDefinition>();
                return collection;
            }
        }

        public bool HasCustomAttributes =>
            ((this.custom_attributes == null) ? this.GetHasCustomAttributes(this.Module) : (this.custom_attributes.Count > 0));

        public Collection<CustomAttribute> CustomAttributes =>
            (this.custom_attributes ?? this.GetCustomAttributes(ref this.custom_attributes, this.Module));

        public bool IsSpecialName
        {
            get => 
                this.attributes.GetAttributes(0x200);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x200, value));
        }

        public bool IsRuntimeSpecialName
        {
            get => 
                this.attributes.GetAttributes(0x400);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x400, value));
        }

        public TypeDefinition DeclaringType
        {
            get => 
                ((TypeDefinition) base.DeclaringType);
            set => 
                (base.DeclaringType = value);
        }

        public override bool IsDefinition =>
            true;
    }
}

