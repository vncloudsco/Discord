namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;

    internal class MetadataResolver : IMetadataResolver
    {
        private readonly IAssemblyResolver assembly_resolver;

        public MetadataResolver(IAssemblyResolver assemblyResolver)
        {
            if (assemblyResolver == null)
            {
                throw new ArgumentNullException("assemblyResolver");
            }
            this.assembly_resolver = assemblyResolver;
        }

        private static bool AreSame(ArrayType a, ArrayType b) => 
            (a.Rank == b.Rank);

        private static bool AreSame(GenericInstanceType a, GenericInstanceType b)
        {
            if (a.GenericArguments.Count != b.GenericArguments.Count)
            {
                return false;
            }
            for (int i = 0; i < a.GenericArguments.Count; i++)
            {
                if (!AreSame(a.GenericArguments[i], b.GenericArguments[i]))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool AreSame(GenericParameter a, GenericParameter b) => 
            (a.Position == b.Position);

        private static bool AreSame(IModifierType a, IModifierType b) => 
            AreSame(a.ModifierType, b.ModifierType);

        private static bool AreSame(TypeReference a, TypeReference b) => 
            (!ReferenceEquals(a, b) ? ((a != null) && ((b != null) && ((a.etype == b.etype) ? (!a.IsGenericParameter ? (!a.IsTypeSpecification() ? ((a.Name == b.Name) && ((a.Namespace == b.Namespace) && AreSame(a.DeclaringType, b.DeclaringType))) : AreSame((TypeSpecification) a, (TypeSpecification) b)) : AreSame((GenericParameter) a, (GenericParameter) b)) : false))) : true);

        private static bool AreSame(TypeSpecification a, TypeSpecification b) => 
            (AreSame(a.ElementType, b.ElementType) ? (!a.IsGenericInstance ? ((a.IsRequiredModifier || a.IsOptionalModifier) ? AreSame((IModifierType) a, (IModifierType) b) : (!a.IsArray || AreSame((ArrayType) a, (ArrayType) b))) : AreSame((GenericInstanceType) a, (GenericInstanceType) b)) : false);

        private static bool AreSame(Collection<ParameterDefinition> a, Collection<ParameterDefinition> b)
        {
            int count = a.Count;
            if (count != b.Count)
            {
                return false;
            }
            if (count != 0)
            {
                for (int i = 0; i < count; i++)
                {
                    if (!AreSame(a[i].ParameterType, b[i].ParameterType))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private FieldDefinition GetField(TypeDefinition type, FieldReference reference)
        {
            while (type != null)
            {
                FieldDefinition field = GetField(type.Fields, reference);
                if (field != null)
                {
                    return field;
                }
                if (type.BaseType == null)
                {
                    return null;
                }
                type = this.Resolve(type.BaseType);
            }
            return null;
        }

        private static FieldDefinition GetField(Collection<FieldDefinition> fields, FieldReference reference)
        {
            for (int i = 0; i < fields.Count; i++)
            {
                FieldDefinition definition = fields[i];
                if ((definition.Name == reference.Name) && AreSame(definition.FieldType, reference.FieldType))
                {
                    return definition;
                }
            }
            return null;
        }

        private MethodDefinition GetMethod(TypeDefinition type, MethodReference reference)
        {
            while (type != null)
            {
                MethodDefinition method = GetMethod(type.Methods, reference);
                if (method != null)
                {
                    return method;
                }
                if (type.BaseType == null)
                {
                    return null;
                }
                type = this.Resolve(type.BaseType);
            }
            return null;
        }

        public static MethodDefinition GetMethod(Collection<MethodDefinition> methods, MethodReference reference)
        {
            for (int i = 0; i < methods.Count; i++)
            {
                MethodDefinition definition = methods[i];
                if ((((definition.Name == reference.Name) && ((definition.HasGenericParameters == reference.HasGenericParameters) && (!definition.HasGenericParameters || (definition.GenericParameters.Count == reference.GenericParameters.Count)))) && AreSame(definition.ReturnType, reference.ReturnType)) && (definition.HasParameters == reference.HasParameters))
                {
                    if (!definition.HasParameters && !reference.HasParameters)
                    {
                        return definition;
                    }
                    if (AreSame(definition.Parameters, reference.Parameters))
                    {
                        return definition;
                    }
                }
            }
            return null;
        }

        private static TypeDefinition GetType(ModuleDefinition module, TypeReference reference)
        {
            TypeDefinition typeDefinition = GetTypeDefinition(module, reference);
            if (typeDefinition != null)
            {
                return typeDefinition;
            }
            if (module.HasExportedTypes)
            {
                Collection<ExportedType> exportedTypes = module.ExportedTypes;
                for (int i = 0; i < exportedTypes.Count; i++)
                {
                    ExportedType type = exportedTypes[i];
                    if ((type.Name == reference.Name) && (type.Namespace == reference.Namespace))
                    {
                        return type.Resolve();
                    }
                }
            }
            return null;
        }

        private static TypeDefinition GetTypeDefinition(ModuleDefinition module, TypeReference type)
        {
            if (!type.IsNested)
            {
                return module.GetType(type.Namespace, type.Name);
            }
            TypeDefinition self = type.DeclaringType.Resolve();
            return ((self != null) ? self.GetNestedType(type.TypeFullName()) : null);
        }

        public virtual FieldDefinition Resolve(FieldReference field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }
            TypeDefinition type = this.Resolve(field.DeclaringType);
            return (type?.HasFields ? this.GetField(type, field) : null);
        }

        public virtual MethodDefinition Resolve(MethodReference method)
        {
            if (method == null)
            {
                throw new ArgumentNullException("method");
            }
            TypeDefinition type = this.Resolve(method.DeclaringType);
            if (type == null)
            {
                return null;
            }
            method = method.GetElementMethod();
            return (type.HasMethods ? this.GetMethod(type, method) : null);
        }

        public virtual TypeDefinition Resolve(TypeReference type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            type = type.GetElementType();
            IMetadataScope scope = type.Scope;
            if (scope == null)
            {
                return null;
            }
            switch (scope.MetadataScopeType)
            {
                case MetadataScopeType.AssemblyNameReference:
                {
                    AssemblyDefinition definition = this.assembly_resolver.Resolve((AssemblyNameReference) scope);
                    return ((definition != null) ? GetType(definition.MainModule, type) : null);
                }
                case MetadataScopeType.ModuleReference:
                {
                    Collection<ModuleDefinition> modules = type.Module.Assembly.Modules;
                    ModuleReference reference = (ModuleReference) scope;
                    for (int i = 0; i < modules.Count; i++)
                    {
                        ModuleDefinition module = modules[i];
                        if (module.Name == reference.Name)
                        {
                            return GetType(module, type);
                        }
                    }
                    break;
                }
                case MetadataScopeType.ModuleDefinition:
                    return GetType((ModuleDefinition) scope, type);

                default:
                    break;
            }
            throw new NotSupportedException();
        }

        public IAssemblyResolver AssemblyResolver =>
            this.assembly_resolver;
    }
}

