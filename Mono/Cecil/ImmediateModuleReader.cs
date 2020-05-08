namespace Mono.Cecil
{
    using Mono.Cecil.PE;
    using Mono.Collections.Generic;
    using System;

    internal sealed class ImmediateModuleReader : ModuleReader
    {
        public ImmediateModuleReader(Image image) : base(image, ReadingMode.Immediate)
        {
        }

        private static void Read(object collection)
        {
        }

        private static void ReadCustomAttributes(ICustomAttributeProvider provider)
        {
            if (provider.HasCustomAttributes)
            {
                Collection<CustomAttribute> customAttributes = provider.CustomAttributes;
                for (int i = 0; i < customAttributes.Count; i++)
                {
                    CustomAttribute attribute = customAttributes[i];
                    Read(attribute.ConstructorArguments);
                }
            }
        }

        private static void ReadEvents(TypeDefinition type)
        {
            Collection<EventDefinition> events = type.Events;
            for (int i = 0; i < events.Count; i++)
            {
                EventDefinition provider = events[i];
                Read(provider.AddMethod);
                ReadCustomAttributes(provider);
            }
        }

        private static void ReadFields(TypeDefinition type)
        {
            Collection<FieldDefinition> fields = type.Fields;
            for (int i = 0; i < fields.Count; i++)
            {
                FieldDefinition provider = fields[i];
                if (provider.HasConstant)
                {
                    Read(provider.Constant);
                }
                if (provider.HasLayoutInfo)
                {
                    Read(provider.Offset);
                }
                if (provider.RVA > 0)
                {
                    Read(provider.InitialValue);
                }
                if (provider.HasMarshalInfo)
                {
                    Read(provider.MarshalInfo);
                }
                ReadCustomAttributes(provider);
            }
        }

        private static void ReadGenericParameters(IGenericParameterProvider provider)
        {
            if (provider.HasGenericParameters)
            {
                Collection<GenericParameter> genericParameters = provider.GenericParameters;
                for (int i = 0; i < genericParameters.Count; i++)
                {
                    GenericParameter parameter = genericParameters[i];
                    if (parameter.HasConstraints)
                    {
                        Read(parameter.Constraints);
                    }
                    ReadCustomAttributes(parameter);
                }
            }
        }

        private static void ReadMethods(TypeDefinition type)
        {
            Collection<MethodDefinition> methods = type.Methods;
            for (int i = 0; i < methods.Count; i++)
            {
                MethodDefinition provider = methods[i];
                ReadGenericParameters(provider);
                if (provider.HasParameters)
                {
                    ReadParameters(provider);
                }
                if (provider.HasOverrides)
                {
                    Read(provider.Overrides);
                }
                if (provider.IsPInvokeImpl)
                {
                    Read(provider.PInvokeInfo);
                }
                ReadSecurityDeclarations(provider);
                ReadCustomAttributes(provider);
                MethodReturnType methodReturnType = provider.MethodReturnType;
                if (methodReturnType.HasConstant)
                {
                    Read(methodReturnType.Constant);
                }
                if (methodReturnType.HasMarshalInfo)
                {
                    Read(methodReturnType.MarshalInfo);
                }
                ReadCustomAttributes(methodReturnType);
            }
        }

        protected override void ReadModule()
        {
            base.module.Read<ModuleDefinition, ModuleDefinition>(base.module, delegate (ModuleDefinition module, MetadataReader reader) {
                base.ReadModuleManifest(reader);
                ReadModule(module);
                return module;
            });
        }

        public static void ReadModule(ModuleDefinition module)
        {
            if (module.HasAssemblyReferences)
            {
                Read(module.AssemblyReferences);
            }
            if (module.HasResources)
            {
                Read(module.Resources);
            }
            if (module.HasModuleReferences)
            {
                Read(module.ModuleReferences);
            }
            if (module.HasTypes)
            {
                ReadTypes(module.Types);
            }
            if (module.HasExportedTypes)
            {
                Read(module.ExportedTypes);
            }
            if (module.HasCustomAttributes)
            {
                Read(module.CustomAttributes);
            }
            AssemblyDefinition assembly = module.Assembly;
            if (assembly != null)
            {
                if (assembly.HasCustomAttributes)
                {
                    ReadCustomAttributes(assembly);
                }
                if (assembly.HasSecurityDeclarations)
                {
                    Read(assembly.SecurityDeclarations);
                }
            }
        }

        private static void ReadParameters(MethodDefinition method)
        {
            Collection<ParameterDefinition> parameters = method.Parameters;
            for (int i = 0; i < parameters.Count; i++)
            {
                ParameterDefinition provider = parameters[i];
                if (provider.HasConstant)
                {
                    Read(provider.Constant);
                }
                if (provider.HasMarshalInfo)
                {
                    Read(provider.MarshalInfo);
                }
                ReadCustomAttributes(provider);
            }
        }

        private static void ReadProperties(TypeDefinition type)
        {
            Collection<PropertyDefinition> properties = type.Properties;
            for (int i = 0; i < properties.Count; i++)
            {
                PropertyDefinition provider = properties[i];
                Read(provider.GetMethod);
                if (provider.HasConstant)
                {
                    Read(provider.Constant);
                }
                ReadCustomAttributes(provider);
            }
        }

        private static void ReadSecurityDeclarations(ISecurityDeclarationProvider provider)
        {
            if (provider.HasSecurityDeclarations)
            {
                Collection<SecurityDeclaration> securityDeclarations = provider.SecurityDeclarations;
                for (int i = 0; i < securityDeclarations.Count; i++)
                {
                    SecurityDeclaration declaration = securityDeclarations[i];
                    Read(declaration.SecurityAttributes);
                }
            }
        }

        private static void ReadType(TypeDefinition type)
        {
            ReadGenericParameters(type);
            if (type.HasInterfaces)
            {
                Read(type.Interfaces);
            }
            if (type.HasNestedTypes)
            {
                ReadTypes(type.NestedTypes);
            }
            if (type.HasLayoutInfo)
            {
                Read(type.ClassSize);
            }
            if (type.HasFields)
            {
                ReadFields(type);
            }
            if (type.HasMethods)
            {
                ReadMethods(type);
            }
            if (type.HasProperties)
            {
                ReadProperties(type);
            }
            if (type.HasEvents)
            {
                ReadEvents(type);
            }
            ReadSecurityDeclarations(type);
            ReadCustomAttributes(type);
        }

        private static void ReadTypes(Collection<TypeDefinition> types)
        {
            for (int i = 0; i < types.Count; i++)
            {
                ReadType(types[i]);
            }
        }
    }
}

