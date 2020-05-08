namespace Mono.Cecil
{
    using Mono.Cecil.Cil;
    using Mono.Collections.Generic;
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class MethodDefinition : MethodReference, IMemberDefinition, ICustomAttributeProvider, ISecurityDeclarationProvider, IMetadataTokenProvider
    {
        private ushort attributes;
        private ushort impl_attributes;
        internal volatile bool sem_attrs_ready;
        internal MethodSemanticsAttributes sem_attrs;
        private Collection<CustomAttribute> custom_attributes;
        private Collection<SecurityDeclaration> security_declarations;
        internal uint rva;
        internal Mono.Cecil.PInvokeInfo pinvoke;
        private Collection<MethodReference> overrides;
        internal MethodBody body;

        internal MethodDefinition()
        {
            base.token = new MetadataToken(TokenType.Method);
        }

        public MethodDefinition(string name, MethodAttributes attributes, TypeReference returnType) : base(name, returnType)
        {
            this.attributes = (ushort) attributes;
            this.HasThis = !this.IsStatic;
            base.token = new MetadataToken(TokenType.Method);
        }

        internal void ReadSemantics()
        {
            if (!this.sem_attrs_ready)
            {
                ModuleDefinition module = this.Module;
                if ((module != null) && module.HasImage)
                {
                    module.Read<MethodDefinition, MethodSemanticsAttributes>(this, (method, reader) => reader.ReadAllSemantics(method));
                }
            }
        }

        public override MethodDefinition Resolve() => 
            this;

        public MethodAttributes Attributes
        {
            get => 
                ((MethodAttributes) this.attributes);
            set => 
                (this.attributes = (ushort) value);
        }

        public MethodImplAttributes ImplAttributes
        {
            get => 
                ((MethodImplAttributes) this.impl_attributes);
            set => 
                (this.impl_attributes = (ushort) value);
        }

        public MethodSemanticsAttributes SemanticsAttributes
        {
            get
            {
                if (!this.sem_attrs_ready)
                {
                    if (base.HasImage)
                    {
                        this.ReadSemantics();
                        return this.sem_attrs;
                    }
                    this.sem_attrs = MethodSemanticsAttributes.None;
                    this.sem_attrs_ready = true;
                }
                return this.sem_attrs;
            }
            set => 
                (this.sem_attrs = value);
        }

        public bool HasSecurityDeclarations =>
            ((this.security_declarations == null) ? this.GetHasSecurityDeclarations(this.Module) : (this.security_declarations.Count > 0));

        public Collection<SecurityDeclaration> SecurityDeclarations =>
            (this.security_declarations ?? this.GetSecurityDeclarations(ref this.security_declarations, this.Module));

        public bool HasCustomAttributes =>
            ((this.custom_attributes == null) ? this.GetHasCustomAttributes(this.Module) : (this.custom_attributes.Count > 0));

        public Collection<CustomAttribute> CustomAttributes =>
            (this.custom_attributes ?? this.GetCustomAttributes(ref this.custom_attributes, this.Module));

        public int RVA =>
            ((int) this.rva);

        public bool HasBody =>
            (((this.attributes & 0x400) == 0) && (((this.attributes & 0x2000) == 0) && (((this.impl_attributes & 0x1000) == 0) && (((this.impl_attributes & 1) == 0) && (((this.impl_attributes & 4) == 0) && ((this.impl_attributes & 3) == 0))))));

        public MethodBody Body
        {
            get
            {
                MethodBody body = this.body;
                if (body != null)
                {
                    return body;
                }
                if (!this.HasBody)
                {
                    return null;
                }
                if (!base.HasImage || (this.rva == 0))
                {
                    MethodBody body2;
                    this.body = body2 = new MethodBody(this);
                    return body2;
                }
                return this.Module.Read<MethodDefinition, MethodBody>(ref this.body, this, (method, reader) => reader.ReadMethodBody(method));
            }
            set
            {
                ModuleDefinition module = this.Module;
                if (module == null)
                {
                    this.body = value;
                }
                else
                {
                    lock (module.SyncRoot)
                    {
                        this.body = value;
                    }
                }
            }
        }

        public bool HasPInvokeInfo =>
            ((this.pinvoke == null) ? this.IsPInvokeImpl : true);

        public Mono.Cecil.PInvokeInfo PInvokeInfo
        {
            get
            {
                if (this.pinvoke != null)
                {
                    return this.pinvoke;
                }
                if (!base.HasImage || !this.IsPInvokeImpl)
                {
                    return null;
                }
                return this.Module.Read<MethodDefinition, Mono.Cecil.PInvokeInfo>(ref this.pinvoke, this, (method, reader) => reader.ReadPInvokeInfo(method));
            }
            set
            {
                this.IsPInvokeImpl = true;
                this.pinvoke = value;
            }
        }

        public bool HasOverrides
        {
            get
            {
                if (this.overrides != null)
                {
                    return (this.overrides.Count > 0);
                }
                if (!base.HasImage)
                {
                    return false;
                }
                return this.Module.Read<MethodDefinition, bool>(this, (method, reader) => reader.HasOverrides(method));
            }
        }

        public Collection<MethodReference> Overrides
        {
            get
            {
                if (this.overrides != null)
                {
                    return this.overrides;
                }
                if (!base.HasImage)
                {
                    Collection<MethodReference> collection;
                    this.overrides = collection = new Collection<MethodReference>();
                    return collection;
                }
                return this.Module.Read<MethodDefinition, Collection<MethodReference>>(ref this.overrides, this, (method, reader) => reader.ReadOverrides(method));
            }
        }

        public override bool HasGenericParameters =>
            ((base.generic_parameters == null) ? this.GetHasGenericParameters(this.Module) : (base.generic_parameters.Count > 0));

        public override Collection<GenericParameter> GenericParameters =>
            (base.generic_parameters ?? this.GetGenericParameters(ref base.generic_parameters, this.Module));

        public bool IsCompilerControlled
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 0);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 0, value));
        }

        public bool IsPrivate
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 1);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 1, value));
        }

        public bool IsFamilyAndAssembly
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 2);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 2, value));
        }

        public bool IsAssembly
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 3);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 3, value));
        }

        public bool IsFamily
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 4);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 4, value));
        }

        public bool IsFamilyOrAssembly
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 5);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 5, value));
        }

        public bool IsPublic
        {
            get => 
                this.attributes.GetMaskedAttributes(7, 6);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(7, 6, value));
        }

        public bool IsStatic
        {
            get => 
                this.attributes.GetAttributes(0x10);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x10, value));
        }

        public bool IsFinal
        {
            get => 
                this.attributes.GetAttributes(0x20);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x20, value));
        }

        public bool IsVirtual
        {
            get => 
                this.attributes.GetAttributes(0x40);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x40, value));
        }

        public bool IsHideBySig
        {
            get => 
                this.attributes.GetAttributes(0x80);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x80, value));
        }

        public bool IsReuseSlot
        {
            get => 
                this.attributes.GetMaskedAttributes(0x100, 0);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x100, 0, value));
        }

        public bool IsNewSlot
        {
            get => 
                this.attributes.GetMaskedAttributes(0x100, 0x100);
            set => 
                (this.attributes = this.attributes.SetMaskedAttributes(0x100, 0x100, value));
        }

        public bool IsCheckAccessOnOverride
        {
            get => 
                this.attributes.GetAttributes(0x200);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x200, value));
        }

        public bool IsAbstract
        {
            get => 
                this.attributes.GetAttributes(0x400);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x400, value));
        }

        public bool IsSpecialName
        {
            get => 
                this.attributes.GetAttributes(0x800);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x800, value));
        }

        public bool IsPInvokeImpl
        {
            get => 
                this.attributes.GetAttributes(0x2000);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x2000, value));
        }

        public bool IsUnmanagedExport
        {
            get => 
                this.attributes.GetAttributes(8);
            set => 
                (this.attributes = this.attributes.SetAttributes(8, value));
        }

        public bool IsRuntimeSpecialName
        {
            get => 
                this.attributes.GetAttributes(0x1000);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x1000, value));
        }

        public bool HasSecurity
        {
            get => 
                this.attributes.GetAttributes(0x4000);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x4000, value));
        }

        public bool IsIL
        {
            get => 
                this.impl_attributes.GetMaskedAttributes(3, 0);
            set => 
                (this.impl_attributes = this.impl_attributes.SetMaskedAttributes(3, 0, value));
        }

        public bool IsNative
        {
            get => 
                this.impl_attributes.GetMaskedAttributes(3, 1);
            set => 
                (this.impl_attributes = this.impl_attributes.SetMaskedAttributes(3, 1, value));
        }

        public bool IsRuntime
        {
            get => 
                this.impl_attributes.GetMaskedAttributes(3, 3);
            set => 
                (this.impl_attributes = this.impl_attributes.SetMaskedAttributes(3, 3, value));
        }

        public bool IsUnmanaged
        {
            get => 
                this.impl_attributes.GetMaskedAttributes(4, 4);
            set => 
                (this.impl_attributes = this.impl_attributes.SetMaskedAttributes(4, 4, value));
        }

        public bool IsManaged
        {
            get => 
                this.impl_attributes.GetMaskedAttributes(4, 0);
            set => 
                (this.impl_attributes = this.impl_attributes.SetMaskedAttributes(4, 0, value));
        }

        public bool IsForwardRef
        {
            get => 
                this.impl_attributes.GetAttributes(0x10);
            set => 
                (this.impl_attributes = this.impl_attributes.SetAttributes(0x10, value));
        }

        public bool IsPreserveSig
        {
            get => 
                this.impl_attributes.GetAttributes(0x80);
            set => 
                (this.impl_attributes = this.impl_attributes.SetAttributes(0x80, value));
        }

        public bool IsInternalCall
        {
            get => 
                this.impl_attributes.GetAttributes(0x1000);
            set => 
                (this.impl_attributes = this.impl_attributes.SetAttributes(0x1000, value));
        }

        public bool IsSynchronized
        {
            get => 
                this.impl_attributes.GetAttributes(0x20);
            set => 
                (this.impl_attributes = this.impl_attributes.SetAttributes(0x20, value));
        }

        public bool NoInlining
        {
            get => 
                this.impl_attributes.GetAttributes(8);
            set => 
                (this.impl_attributes = this.impl_attributes.SetAttributes(8, value));
        }

        public bool NoOptimization
        {
            get => 
                this.impl_attributes.GetAttributes(0x40);
            set => 
                (this.impl_attributes = this.impl_attributes.SetAttributes(0x40, value));
        }

        public bool IsSetter
        {
            get => 
                this.GetSemantics((MethodSemanticsAttributes.None | MethodSemanticsAttributes.Setter));
            set => 
                this.SetSemantics((MethodSemanticsAttributes.None | MethodSemanticsAttributes.Setter), value);
        }

        public bool IsGetter
        {
            get => 
                this.GetSemantics(MethodSemanticsAttributes.Getter);
            set => 
                this.SetSemantics(MethodSemanticsAttributes.Getter, value);
        }

        public bool IsOther
        {
            get => 
                this.GetSemantics((MethodSemanticsAttributes.None | MethodSemanticsAttributes.Other));
            set => 
                this.SetSemantics((MethodSemanticsAttributes.None | MethodSemanticsAttributes.Other), value);
        }

        public bool IsAddOn
        {
            get => 
                this.GetSemantics(MethodSemanticsAttributes.AddOn);
            set => 
                this.SetSemantics(MethodSemanticsAttributes.AddOn, value);
        }

        public bool IsRemoveOn
        {
            get => 
                this.GetSemantics((MethodSemanticsAttributes.None | MethodSemanticsAttributes.RemoveOn));
            set => 
                this.SetSemantics((MethodSemanticsAttributes.None | MethodSemanticsAttributes.RemoveOn), value);
        }

        public bool IsFire
        {
            get => 
                this.GetSemantics(MethodSemanticsAttributes.Fire);
            set => 
                this.SetSemantics(MethodSemanticsAttributes.Fire, value);
        }

        public TypeDefinition DeclaringType
        {
            get => 
                ((TypeDefinition) base.DeclaringType);
            set => 
                (base.DeclaringType = value);
        }

        public bool IsConstructor =>
            (this.IsRuntimeSpecialName && (this.IsSpecialName && ((this.Name == ".cctor") || (this.Name == ".ctor"))));

        public override bool IsDefinition =>
            true;
    }
}

