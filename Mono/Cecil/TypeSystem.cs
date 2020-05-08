namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using Mono.Collections.Generic;
    using System;

    internal abstract class TypeSystem
    {
        private readonly ModuleDefinition module;
        private TypeReference type_object;
        private TypeReference type_void;
        private TypeReference type_bool;
        private TypeReference type_char;
        private TypeReference type_sbyte;
        private TypeReference type_byte;
        private TypeReference type_int16;
        private TypeReference type_uint16;
        private TypeReference type_int32;
        private TypeReference type_uint32;
        private TypeReference type_int64;
        private TypeReference type_uint64;
        private TypeReference type_single;
        private TypeReference type_double;
        private TypeReference type_intptr;
        private TypeReference type_uintptr;
        private TypeReference type_string;
        private TypeReference type_typedref;

        private TypeSystem(ModuleDefinition module)
        {
            this.module = module;
        }

        internal static TypeSystem CreateTypeSystem(ModuleDefinition module) => 
            (!module.IsCorlib() ? ((TypeSystem) new CommonTypeSystem(module)) : ((TypeSystem) new CoreTypeSystem(module)));

        private TypeReference LookupSystemType(ref TypeReference reference, string name, ElementType element_type)
        {
            TypeReference reference3;
            lock (this.module.SyncRoot)
            {
                if (reference != null)
                {
                    reference3 = reference;
                }
                else
                {
                    TypeReference type = this.LookupType("System", name);
                    type.etype = element_type;
                    reference3 = reference = type;
                }
            }
            return reference3;
        }

        private TypeReference LookupSystemValueType(ref TypeReference typeRef, string name, ElementType element_type)
        {
            TypeReference reference2;
            lock (this.module.SyncRoot)
            {
                if (typeRef != null)
                {
                    reference2 = typeRef;
                }
                else
                {
                    TypeReference type = this.LookupType("System", name);
                    type.etype = element_type;
                    type.IsValueType = true;
                    reference2 = typeRef = type;
                }
            }
            return reference2;
        }

        internal abstract TypeReference LookupType(string @namespace, string name);

        public IMetadataScope Corlib
        {
            get
            {
                CommonTypeSystem system = this as CommonTypeSystem;
                return ((system != null) ? ((IMetadataScope) system.GetCorlibReference()) : ((IMetadataScope) this.module));
            }
        }

        public TypeReference Object =>
            (this.type_object ?? this.LookupSystemType(ref this.type_object, "Object", ElementType.Object));

        public TypeReference Void =>
            (this.type_void ?? this.LookupSystemType(ref this.type_void, "Void", ElementType.Void));

        public TypeReference Boolean =>
            (this.type_bool ?? this.LookupSystemValueType(ref this.type_bool, "Boolean", ElementType.Boolean));

        public TypeReference Char =>
            (this.type_char ?? this.LookupSystemValueType(ref this.type_char, "Char", ElementType.Char));

        public TypeReference SByte =>
            (this.type_sbyte ?? this.LookupSystemValueType(ref this.type_sbyte, "SByte", ElementType.I1));

        public TypeReference Byte =>
            (this.type_byte ?? this.LookupSystemValueType(ref this.type_byte, "Byte", ElementType.U1));

        public TypeReference Int16 =>
            (this.type_int16 ?? this.LookupSystemValueType(ref this.type_int16, "Int16", ElementType.I2));

        public TypeReference UInt16 =>
            (this.type_uint16 ?? this.LookupSystemValueType(ref this.type_uint16, "UInt16", ElementType.U2));

        public TypeReference Int32 =>
            (this.type_int32 ?? this.LookupSystemValueType(ref this.type_int32, "Int32", ElementType.I4));

        public TypeReference UInt32 =>
            (this.type_uint32 ?? this.LookupSystemValueType(ref this.type_uint32, "UInt32", ElementType.U4));

        public TypeReference Int64 =>
            (this.type_int64 ?? this.LookupSystemValueType(ref this.type_int64, "Int64", ElementType.I8));

        public TypeReference UInt64 =>
            (this.type_uint64 ?? this.LookupSystemValueType(ref this.type_uint64, "UInt64", ElementType.U8));

        public TypeReference Single =>
            (this.type_single ?? this.LookupSystemValueType(ref this.type_single, "Single", ElementType.R4));

        public TypeReference Double =>
            (this.type_double ?? this.LookupSystemValueType(ref this.type_double, "Double", ElementType.R8));

        public TypeReference IntPtr =>
            (this.type_intptr ?? this.LookupSystemValueType(ref this.type_intptr, "IntPtr", ElementType.I));

        public TypeReference UIntPtr =>
            (this.type_uintptr ?? this.LookupSystemValueType(ref this.type_uintptr, "UIntPtr", ElementType.U));

        public TypeReference String =>
            (this.type_string ?? this.LookupSystemType(ref this.type_string, "String", ElementType.String));

        public TypeReference TypedReference =>
            (this.type_typedref ?? this.LookupSystemValueType(ref this.type_typedref, "TypedReference", ElementType.TypedByRef));

        private sealed class CommonTypeSystem : TypeSystem
        {
            private AssemblyNameReference corlib;

            public CommonTypeSystem(ModuleDefinition module) : base(module)
            {
            }

            private TypeReference CreateTypeReference(string @namespace, string name) => 
                new TypeReference(@namespace, name, base.module, this.GetCorlibReference());

            public AssemblyNameReference GetCorlibReference()
            {
                if (this.corlib == null)
                {
                    Collection<AssemblyNameReference> assemblyReferences = base.module.AssemblyReferences;
                    for (int i = 0; i < assemblyReferences.Count; i++)
                    {
                        AssemblyNameReference reference = assemblyReferences[i];
                        if (reference.Name == "mscorlib")
                        {
                            AssemblyNameReference reference3;
                            this.corlib = reference3 = reference;
                            return reference3;
                        }
                    }
                    AssemblyNameReference reference2 = new AssemblyNameReference {
                        Name = "mscorlib",
                        Version = this.GetCorlibVersion()
                    };
                    reference2.PublicKeyToken = new byte[] { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 };
                    this.corlib = reference2;
                    assemblyReferences.Add(this.corlib);
                }
                return this.corlib;
            }

            private Version GetCorlibVersion()
            {
                switch (base.module.Runtime)
                {
                    case TargetRuntime.Net_1_0:
                    case TargetRuntime.Net_1_1:
                        return new Version(1, 0, 0, 0);

                    case TargetRuntime.Net_2_0:
                        return new Version(2, 0, 0, 0);

                    case TargetRuntime.Net_4_0:
                        return new Version(4, 0, 0, 0);
                }
                throw new NotSupportedException();
            }

            internal override TypeReference LookupType(string @namespace, string name) => 
                this.CreateTypeReference(@namespace, name);
        }

        private sealed class CoreTypeSystem : TypeSystem
        {
            public CoreTypeSystem(ModuleDefinition module) : base(module)
            {
            }

            private static void Initialize(object obj)
            {
            }

            internal override TypeReference LookupType(string @namespace, string name)
            {
                TypeReference reference = this.LookupTypeDefinition(@namespace, name) ?? this.LookupTypeForwarded(@namespace, name);
                if (reference == null)
                {
                    throw new NotSupportedException();
                }
                return reference;
            }

            private TypeReference LookupTypeDefinition(string @namespace, string name)
            {
                if (base.module.MetadataSystem.Types == null)
                {
                    Initialize(base.module.Types);
                }
                return base.module.Read<Row<string, string>, TypeDefinition>(new Row<string, string>(@namespace, name), delegate (Row<string, string> row, MetadataReader reader) {
                    foreach (TypeDefinition definition in reader.metadata.Types)
                    {
                        TypeDefinition[] definitionArray;
                        int num;
                        if (definitionArray[num] == null)
                        {
                            definitionArray[num] = reader.GetTypeDefinition((uint) (num + 1));
                        }
                        if ((definition.Name == row.Col2) && (definition.Namespace == row.Col1))
                        {
                            return definition;
                        }
                    }
                    return null;
                });
            }

            private TypeReference LookupTypeForwarded(string @namespace, string name)
            {
                if (base.module.HasExportedTypes)
                {
                    Collection<ExportedType> exportedTypes = base.module.ExportedTypes;
                    for (int i = 0; i < exportedTypes.Count; i++)
                    {
                        ExportedType type = exportedTypes[i];
                        if ((type.Name == name) && (type.Namespace == @namespace))
                        {
                            return type.CreateReference();
                        }
                    }
                }
                return null;
            }
        }
    }
}

