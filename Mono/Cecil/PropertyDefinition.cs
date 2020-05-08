namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;
    using System.Text;

    internal sealed class PropertyDefinition : PropertyReference, IMemberDefinition, ICustomAttributeProvider, IConstantProvider, IMetadataTokenProvider
    {
        private bool? has_this;
        private ushort attributes;
        private Collection<CustomAttribute> custom_attributes;
        internal MethodDefinition get_method;
        internal MethodDefinition set_method;
        internal Collection<MethodDefinition> other_methods;
        private object constant;

        public PropertyDefinition(string name, PropertyAttributes attributes, TypeReference propertyType) : base(name, propertyType)
        {
            this.constant = Mixin.NotResolved;
            this.attributes = (ushort) attributes;
            base.token = new MetadataToken(TokenType.Property);
        }

        private void InitializeMethods()
        {
            ModuleDefinition module = this.Module;
            if (module != null)
            {
                lock (module.SyncRoot)
                {
                    if (((this.get_method == null) && (this.set_method == null)) && module.HasImage())
                    {
                        module.Read<PropertyDefinition, PropertyDefinition>(this, (property, reader) => reader.ReadMethods(property));
                    }
                }
            }
        }

        private static Collection<ParameterDefinition> MirrorParameters(MethodDefinition method, int bound)
        {
            Collection<ParameterDefinition> collection = new Collection<ParameterDefinition>();
            if (method.HasParameters)
            {
                Collection<ParameterDefinition> parameters = method.Parameters;
                int num = parameters.Count - bound;
                for (int i = 0; i < num; i++)
                {
                    collection.Add(parameters[i]);
                }
            }
            return collection;
        }

        public override PropertyDefinition Resolve() => 
            this;

        public PropertyAttributes Attributes
        {
            get => 
                ((PropertyAttributes) this.attributes);
            set => 
                (this.attributes = (ushort) value);
        }

        public bool HasThis
        {
            get => 
                ((this.has_this == null) ? ((this.GetMethod == null) ? ((this.SetMethod != null) && this.set_method.HasThis) : this.get_method.HasThis) : this.has_this.Value);
            set => 
                (this.has_this = new bool?(value));
        }

        public bool HasCustomAttributes =>
            ((this.custom_attributes == null) ? this.GetHasCustomAttributes(this.Module) : (this.custom_attributes.Count > 0));

        public Collection<CustomAttribute> CustomAttributes =>
            (this.custom_attributes ?? this.GetCustomAttributes(ref this.custom_attributes, this.Module));

        public MethodDefinition GetMethod
        {
            get
            {
                if (this.get_method == null)
                {
                    this.InitializeMethods();
                }
                return this.get_method;
            }
            set => 
                (this.get_method = value);
        }

        public MethodDefinition SetMethod
        {
            get
            {
                if (this.set_method == null)
                {
                    this.InitializeMethods();
                }
                return this.set_method;
            }
            set => 
                (this.set_method = value);
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

        public bool HasParameters
        {
            get
            {
                this.InitializeMethods();
                return ((this.get_method == null) ? ((this.set_method != null) && (this.set_method.HasParameters && (this.set_method.Parameters.Count > 1))) : this.get_method.HasParameters);
            }
        }

        public override Collection<ParameterDefinition> Parameters
        {
            get
            {
                this.InitializeMethods();
                return ((this.get_method == null) ? ((this.set_method == null) ? new Collection<ParameterDefinition>() : MirrorParameters(this.set_method, 1)) : MirrorParameters(this.get_method, 0));
            }
        }

        public bool HasConstant
        {
            get
            {
                this.ResolveConstant(ref this.constant, this.Module);
                return (this.constant != Mixin.NoValue);
            }
            set
            {
                if (!value)
                {
                    this.constant = Mixin.NoValue;
                }
            }
        }

        public object Constant
        {
            get => 
                (this.HasConstant ? this.constant : null);
            set => 
                (this.constant = value);
        }

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

        public bool HasDefault
        {
            get => 
                this.attributes.GetAttributes(0x1000);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x1000, value));
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

        public override string FullName
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(base.PropertyType.ToString());
                builder.Append(' ');
                builder.Append(base.MemberFullName());
                builder.Append('(');
                if (this.HasParameters)
                {
                    Collection<ParameterDefinition> parameters = this.Parameters;
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        if (i > 0)
                        {
                            builder.Append(',');
                        }
                        builder.Append(parameters[i].ParameterType.FullName);
                    }
                }
                builder.Append(')');
                return builder.ToString();
            }
        }
    }
}

