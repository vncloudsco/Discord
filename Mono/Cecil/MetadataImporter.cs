namespace Mono.Cecil
{
    using Mono;
    using Mono.Cecil.Metadata;
    using Mono.Collections.Generic;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class MetadataImporter
    {
        private readonly ModuleDefinition module;
        private static readonly Dictionary<Type, ElementType> type_etype_mapping;

        static MetadataImporter()
        {
            Dictionary<Type, ElementType> dictionary = new Dictionary<Type, ElementType>(0x12) {
                { 
                    typeof(void),
                    ElementType.Void
                },
                { 
                    typeof(bool),
                    ElementType.Boolean
                },
                { 
                    typeof(char),
                    ElementType.Char
                },
                { 
                    typeof(sbyte),
                    ElementType.I1
                },
                { 
                    typeof(byte),
                    ElementType.U1
                },
                { 
                    typeof(short),
                    ElementType.I2
                },
                { 
                    typeof(ushort),
                    ElementType.U2
                },
                { 
                    typeof(int),
                    ElementType.I4
                },
                { 
                    typeof(uint),
                    ElementType.U4
                },
                { 
                    typeof(long),
                    ElementType.I8
                },
                { 
                    typeof(ulong),
                    ElementType.U8
                },
                { 
                    typeof(float),
                    ElementType.R4
                },
                { 
                    typeof(double),
                    ElementType.R8
                },
                { 
                    typeof(string),
                    ElementType.String
                },
                { 
                    typeof(TypedReference),
                    ElementType.TypedByRef
                },
                { 
                    typeof(IntPtr),
                    ElementType.I
                },
                { 
                    typeof(UIntPtr),
                    ElementType.U
                },
                { 
                    typeof(object),
                    ElementType.Object
                }
            };
            type_etype_mapping = dictionary;
        }

        public MetadataImporter(ModuleDefinition module)
        {
            this.module = module;
        }

        private static bool HasCallingConvention(MethodBase method, CallingConventions conventions) => 
            ((method.CallingConvention & conventions) != 0);

        private AssemblyNameReference ImportAssemblyName(AssemblyNameReference name)
        {
            AssemblyNameReference reference;
            if (!this.TryGetAssemblyNameReference(name, out reference))
            {
                reference = new AssemblyNameReference(name.Name, name.Version) {
                    Culture = name.Culture,
                    HashAlgorithm = name.HashAlgorithm,
                    IsRetargetable = name.IsRetargetable
                };
                byte[] dst = !name.PublicKeyToken.IsNullOrEmpty<byte>() ? new byte[name.PublicKeyToken.Length] : Empty<byte>.Array;
                if (dst.Length > 0)
                {
                    Buffer.BlockCopy(name.PublicKeyToken, 0, dst, 0, dst.Length);
                }
                reference.PublicKeyToken = dst;
                this.module.AssemblyReferences.Add(reference);
            }
            return reference;
        }

        private static ElementType ImportElementType(Type type)
        {
            ElementType type2;
            return (type_etype_mapping.TryGetValue(type, out type2) ? type2 : ElementType.None);
        }

        public FieldReference ImportField(FieldReference field, ImportGenericContext context)
        {
            FieldReference reference3;
            TypeReference provider = this.ImportType(field.DeclaringType, context);
            context.Push(provider);
            try
            {
                reference3 = new FieldReference {
                    Name = field.Name,
                    DeclaringType = provider,
                    FieldType = this.ImportType(field.FieldType, context)
                };
            }
            finally
            {
                context.Pop();
            }
            return reference3;
        }

        public FieldReference ImportField(FieldInfo field, ImportGenericContext context)
        {
            FieldReference reference3;
            TypeReference provider = this.ImportType(field.DeclaringType, context);
            if (IsGenericInstance(field.DeclaringType))
            {
                field = ResolveFieldDefinition(field);
            }
            context.Push(provider);
            try
            {
                reference3 = new FieldReference {
                    Name = field.Name,
                    DeclaringType = provider,
                    FieldType = this.ImportType(field.FieldType, context)
                };
            }
            finally
            {
                context.Pop();
            }
            return reference3;
        }

        private TypeReference ImportGenericInstance(Type type, ImportGenericContext context)
        {
            TypeReference reference2;
            TypeReference reference = this.ImportType(type.GetGenericTypeDefinition(), context, ImportGenericKind.Definition);
            GenericInstanceType type2 = new GenericInstanceType(reference);
            Type[] genericArguments = type.GetGenericArguments();
            Collection<TypeReference> collection = type2.GenericArguments;
            context.Push(reference);
            try
            {
                int index = 0;
                while (true)
                {
                    if (index >= genericArguments.Length)
                    {
                        reference2 = type2;
                        break;
                    }
                    collection.Add(this.ImportType(genericArguments[index], context));
                    index++;
                }
            }
            finally
            {
                context.Pop();
            }
            return reference2;
        }

        private static TypeReference ImportGenericParameter(Type type, ImportGenericContext context)
        {
            if (context.IsEmpty)
            {
                throw new InvalidOperationException();
            }
            if (type.DeclaringMethod != null)
            {
                return context.MethodParameter(type.DeclaringMethod.Name, type.GenericParameterPosition);
            }
            if (type.DeclaringType == null)
            {
                throw new InvalidOperationException();
            }
            return context.TypeParameter(NormalizedFullName(type.DeclaringType), type.GenericParameterPosition);
        }

        private static void ImportGenericParameters(IGenericParameterProvider provider, Type[] arguments)
        {
            Collection<GenericParameter> genericParameters = provider.GenericParameters;
            for (int i = 0; i < arguments.Length; i++)
            {
                genericParameters.Add(new GenericParameter(arguments[i].Name, provider));
            }
        }

        private static void ImportGenericParameters(IGenericParameterProvider imported, IGenericParameterProvider original)
        {
            Collection<GenericParameter> genericParameters = original.GenericParameters;
            Collection<GenericParameter> collection2 = imported.GenericParameters;
            for (int i = 0; i < genericParameters.Count; i++)
            {
                collection2.Add(new GenericParameter(genericParameters[i].Name, imported));
            }
        }

        public MethodReference ImportMethod(MethodReference method, ImportGenericContext context)
        {
            MethodReference reference4;
            if (method.IsGenericInstance)
            {
                return this.ImportMethodSpecification(method, context);
            }
            MethodReference imported = new MethodReference {
                Name = method.Name,
                HasThis = method.HasThis,
                ExplicitThis = method.ExplicitThis,
                DeclaringType = this.ImportType(method.DeclaringType, context),
                CallingConvention = method.CallingConvention
            };
            if (method.HasGenericParameters)
            {
                ImportGenericParameters(imported, method);
            }
            context.Push(imported);
            try
            {
                imported.ReturnType = this.ImportType(method.ReturnType, context);
                if (!method.HasParameters)
                {
                    reference4 = imported;
                }
                else
                {
                    Collection<ParameterDefinition> parameters = imported.Parameters;
                    Collection<ParameterDefinition> collection2 = method.Parameters;
                    int num = 0;
                    while (true)
                    {
                        if (num >= collection2.Count)
                        {
                            reference4 = imported;
                            break;
                        }
                        parameters.Add(new ParameterDefinition(this.ImportType(collection2[num].ParameterType, context)));
                        num++;
                    }
                }
            }
            finally
            {
                context.Pop();
            }
            return reference4;
        }

        public MethodReference ImportMethod(MethodBase method, ImportGenericContext context, ImportGenericKind import_kind)
        {
            MethodReference reference4;
            if (IsMethodSpecification(method) || ImportOpenGenericMethod(method, import_kind))
            {
                return this.ImportMethodSpecification(method, context);
            }
            TypeReference reference = this.ImportType(method.DeclaringType, context);
            if (IsGenericInstance(method.DeclaringType))
            {
                method = method.Module.ResolveMethod(method.MetadataToken);
            }
            MethodReference provider = new MethodReference {
                Name = method.Name,
                HasThis = HasCallingConvention(method, CallingConventions.HasThis),
                ExplicitThis = HasCallingConvention(method, CallingConventions.ExplicitThis),
                DeclaringType = this.ImportType(method.DeclaringType, context, ImportGenericKind.Definition)
            };
            if (HasCallingConvention(method, CallingConventions.VarArgs))
            {
                provider.CallingConvention = (MethodCallingConvention) ((byte) (provider.CallingConvention & MethodCallingConvention.VarArg));
            }
            if (method.IsGenericMethod)
            {
                ImportGenericParameters(provider, method.GetGenericArguments());
            }
            context.Push(provider);
            try
            {
                TypeReference reference1;
                MethodInfo info = method as MethodInfo;
                if (info != null)
                {
                    reference1 = this.ImportType(info.ReturnType, context);
                }
                else
                {
                    ImportGenericContext context2 = new ImportGenericContext();
                    reference1 = this.ImportType(typeof(void), context2);
                }
                provider.ReturnType = reference1;
                ParameterInfo[] parameters = method.GetParameters();
                Collection<ParameterDefinition> collection = provider.Parameters;
                int index = 0;
                while (true)
                {
                    if (index >= parameters.Length)
                    {
                        provider.DeclaringType = reference;
                        reference4 = provider;
                        break;
                    }
                    collection.Add(new ParameterDefinition(this.ImportType(parameters[index].ParameterType, context)));
                    index++;
                }
            }
            finally
            {
                context.Pop();
            }
            return reference4;
        }

        private MethodSpecification ImportMethodSpecification(MethodReference method, ImportGenericContext context)
        {
            if (!method.IsGenericInstance)
            {
                throw new NotSupportedException();
            }
            GenericInstanceMethod method2 = (GenericInstanceMethod) method;
            GenericInstanceMethod method3 = new GenericInstanceMethod(this.ImportMethod(method2.ElementMethod, context));
            Collection<TypeReference> genericArguments = method2.GenericArguments;
            Collection<TypeReference> collection2 = method3.GenericArguments;
            for (int i = 0; i < genericArguments.Count; i++)
            {
                collection2.Add(this.ImportType(genericArguments[i], context));
            }
            return method3;
        }

        private MethodReference ImportMethodSpecification(MethodBase method, ImportGenericContext context)
        {
            MethodReference reference2;
            MethodInfo info = method as MethodInfo;
            if (info == null)
            {
                throw new InvalidOperationException();
            }
            MethodReference reference = this.ImportMethod(info.GetGenericMethodDefinition(), context, ImportGenericKind.Definition);
            GenericInstanceMethod method2 = new GenericInstanceMethod(reference);
            Type[] genericArguments = method.GetGenericArguments();
            Collection<TypeReference> collection = method2.GenericArguments;
            context.Push(reference);
            try
            {
                int index = 0;
                while (true)
                {
                    if (index >= genericArguments.Length)
                    {
                        reference2 = method2;
                        break;
                    }
                    collection.Add(this.ImportType(genericArguments[index], context));
                    index++;
                }
            }
            finally
            {
                context.Pop();
            }
            return reference2;
        }

        private static bool ImportOpenGenericMethod(MethodBase method, ImportGenericKind import_kind) => 
            (method.IsGenericMethod && (method.IsGenericMethodDefinition && (import_kind == ImportGenericKind.Open)));

        private static bool ImportOpenGenericType(Type type, ImportGenericKind import_kind) => 
            (type.IsGenericType && (type.IsGenericTypeDefinition && (import_kind == ImportGenericKind.Open)));

        private IMetadataScope ImportScope(IMetadataScope scope)
        {
            switch (scope.MetadataScopeType)
            {
                case MetadataScopeType.AssemblyNameReference:
                    return this.ImportAssemblyName((AssemblyNameReference) scope);

                case MetadataScopeType.ModuleReference:
                    throw new NotImplementedException();

                case MetadataScopeType.ModuleDefinition:
                    return (!ReferenceEquals(scope, this.module) ? this.ImportAssemblyName(((ModuleDefinition) scope).Assembly.Name) : scope);
            }
            throw new NotSupportedException();
        }

        private AssemblyNameReference ImportScope(Assembly assembly)
        {
            AssemblyNameReference reference;
            AssemblyName name = assembly.GetName();
            if (!this.TryGetAssemblyNameReference(name, out reference))
            {
                reference = new AssemblyNameReference(name.Name, name.Version) {
                    Culture = name.CultureInfo.Name,
                    PublicKeyToken = name.GetPublicKeyToken(),
                    HashAlgorithm = (AssemblyHashAlgorithm) name.HashAlgorithm
                };
                this.module.AssemblyReferences.Add(reference);
            }
            return reference;
        }

        public TypeReference ImportType(TypeReference type, ImportGenericContext context)
        {
            if (type.IsTypeSpecification())
            {
                return this.ImportTypeSpecification(type, context);
            }
            TypeReference reference = new TypeReference(type.Namespace, type.Name, this.module, this.ImportScope(type.Scope), type.IsValueType);
            MetadataSystem.TryProcessPrimitiveTypeReference(reference);
            if (type.IsNested)
            {
                reference.DeclaringType = this.ImportType(type.DeclaringType, context);
            }
            if (type.HasGenericParameters)
            {
                ImportGenericParameters(reference, type);
            }
            return reference;
        }

        public TypeReference ImportType(Type type, ImportGenericContext context) => 
            this.ImportType(type, context, ImportGenericKind.Open);

        public TypeReference ImportType(Type type, ImportGenericContext context, ImportGenericKind import_kind)
        {
            if (IsTypeSpecification(type) || ImportOpenGenericType(type, import_kind))
            {
                return this.ImportTypeSpecification(type, context);
            }
            TypeReference provider = new TypeReference(string.Empty, type.Name, this.module, this.ImportScope(type.Assembly), type.IsValueType) {
                etype = ImportElementType(type)
            };
            if (IsNestedType(type))
            {
                provider.DeclaringType = this.ImportType(type.DeclaringType, context, import_kind);
            }
            else
            {
                provider.Namespace = type.Namespace ?? string.Empty;
            }
            if (type.IsGenericType)
            {
                ImportGenericParameters(provider, type.GetGenericArguments());
            }
            return provider;
        }

        private TypeReference ImportTypeSpecification(TypeReference type, ImportGenericContext context)
        {
            ElementType etype = type.etype;
            if (etype > ElementType.CModOpt)
            {
                if (etype == ElementType.Sentinel)
                {
                    SentinelType type6 = (SentinelType) type;
                    return new SentinelType(this.ImportType(type6.ElementType, context));
                }
                if (etype == ElementType.Pinned)
                {
                    PinnedType type5 = (PinnedType) type;
                    return new PinnedType(this.ImportType(type5.ElementType, context));
                }
            }
            else
            {
                switch (etype)
                {
                    case ElementType.Ptr:
                    {
                        PointerType type3 = (PointerType) type;
                        return new PointerType(this.ImportType(type3.ElementType, context));
                    }
                    case ElementType.ByRef:
                    {
                        ByReferenceType type4 = (ByReferenceType) type;
                        return new ByReferenceType(this.ImportType(type4.ElementType, context));
                    }
                    case ElementType.ValueType:
                    case ElementType.Class:
                        break;

                    case ElementType.Var:
                    {
                        GenericParameter parameter = (GenericParameter) type;
                        if (parameter.DeclaringType == null)
                        {
                            throw new InvalidOperationException();
                        }
                        return context.TypeParameter(parameter.DeclaringType.FullName, parameter.Position);
                    }
                    case ElementType.Array:
                    {
                        ArrayType type9 = (ArrayType) type;
                        ArrayType type10 = new ArrayType(this.ImportType(type9.ElementType, context));
                        if (!type9.IsVector)
                        {
                            Collection<ArrayDimension> dimensions = type9.Dimensions;
                            Collection<ArrayDimension> collection2 = type10.Dimensions;
                            collection2.Clear();
                            for (int i = 0; i < dimensions.Count; i++)
                            {
                                ArrayDimension dimension = dimensions[i];
                                collection2.Add(new ArrayDimension(dimension.LowerBound, dimension.UpperBound));
                            }
                        }
                        return type10;
                    }
                    case ElementType.GenericInst:
                    {
                        GenericInstanceType type11 = (GenericInstanceType) type;
                        GenericInstanceType type12 = new GenericInstanceType(this.ImportType(type11.ElementType, context));
                        Collection<TypeReference> genericArguments = type11.GenericArguments;
                        Collection<TypeReference> collection4 = type12.GenericArguments;
                        for (int i = 0; i < genericArguments.Count; i++)
                        {
                            collection4.Add(this.ImportType(genericArguments[i], context));
                        }
                        return type12;
                    }
                    default:
                        switch (etype)
                        {
                            case ElementType.SzArray:
                            {
                                ArrayType type2 = (ArrayType) type;
                                return new ArrayType(this.ImportType(type2.ElementType, context));
                            }
                            case ElementType.MVar:
                            {
                                GenericParameter parameter2 = (GenericParameter) type;
                                if (parameter2.DeclaringMethod == null)
                                {
                                    throw new InvalidOperationException();
                                }
                                return context.MethodParameter(parameter2.DeclaringMethod.Name, parameter2.Position);
                            }
                            case ElementType.CModReqD:
                            {
                                RequiredModifierType type8 = (RequiredModifierType) type;
                                return new RequiredModifierType(this.ImportType(type8.ModifierType, context), this.ImportType(type8.ElementType, context));
                            }
                            case ElementType.CModOpt:
                            {
                                OptionalModifierType type7 = (OptionalModifierType) type;
                                return new OptionalModifierType(this.ImportType(type7.ModifierType, context), this.ImportType(type7.ElementType, context));
                            }
                            default:
                                break;
                        }
                        break;
                }
            }
            throw new NotSupportedException(type.etype.ToString());
        }

        private TypeReference ImportTypeSpecification(Type type, ImportGenericContext context)
        {
            if (type.IsByRef)
            {
                return new ByReferenceType(this.ImportType(type.GetElementType(), context));
            }
            if (type.IsPointer)
            {
                return new PointerType(this.ImportType(type.GetElementType(), context));
            }
            if (type.IsArray)
            {
                return new ArrayType(this.ImportType(type.GetElementType(), context), type.GetArrayRank());
            }
            if (type.IsGenericType)
            {
                return this.ImportGenericInstance(type, context);
            }
            if (!type.IsGenericParameter)
            {
                throw new NotSupportedException(type.FullName);
            }
            return ImportGenericParameter(type, context);
        }

        private static bool IsGenericInstance(Type type) => 
            (type.IsGenericType && !type.IsGenericTypeDefinition);

        private static bool IsMethodSpecification(MethodBase method) => 
            (method.IsGenericMethod && !method.IsGenericMethodDefinition);

        private static bool IsNestedType(Type type) => 
            type.IsNested;

        private static bool IsTypeSpecification(Type type) => 
            (type.HasElementType || (IsGenericInstance(type) || type.IsGenericParameter));

        private static string NormalizedFullName(Type type) => 
            (!IsNestedType(type) ? type.FullName : (NormalizedFullName(type.DeclaringType) + "/" + type.Name));

        private static FieldInfo ResolveFieldDefinition(FieldInfo field) => 
            field.Module.ResolveField(field.MetadataToken);

        private bool TryGetAssemblyNameReference(AssemblyNameReference name_reference, out AssemblyNameReference assembly_reference)
        {
            Collection<AssemblyNameReference> assemblyReferences = this.module.AssemblyReferences;
            for (int i = 0; i < assemblyReferences.Count; i++)
            {
                AssemblyNameReference reference = assemblyReferences[i];
                if (name_reference.FullName == reference.FullName)
                {
                    assembly_reference = reference;
                    return true;
                }
            }
            assembly_reference = null;
            return false;
        }

        private bool TryGetAssemblyNameReference(AssemblyName name, out AssemblyNameReference assembly_reference)
        {
            Collection<AssemblyNameReference> assemblyReferences = this.module.AssemblyReferences;
            for (int i = 0; i < assemblyReferences.Count; i++)
            {
                AssemblyNameReference reference = assemblyReferences[i];
                if (name.FullName == reference.FullName)
                {
                    assembly_reference = reference;
                    return true;
                }
            }
            assembly_reference = null;
            return false;
        }
    }
}

