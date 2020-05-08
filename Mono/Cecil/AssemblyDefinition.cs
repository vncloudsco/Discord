namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;
    using System.IO;

    internal sealed class AssemblyDefinition : ICustomAttributeProvider, ISecurityDeclarationProvider, IMetadataTokenProvider
    {
        private AssemblyNameDefinition name;
        internal ModuleDefinition main_module;
        private Collection<ModuleDefinition> modules;
        private Collection<CustomAttribute> custom_attributes;
        private Collection<SecurityDeclaration> security_declarations;

        internal AssemblyDefinition()
        {
        }

        public static AssemblyDefinition CreateAssembly(AssemblyNameDefinition assemblyName, string moduleName, ModuleKind kind)
        {
            ModuleParameters parameters = new ModuleParameters {
                Kind = kind
            };
            return CreateAssembly(assemblyName, moduleName, parameters);
        }

        public static AssemblyDefinition CreateAssembly(AssemblyNameDefinition assemblyName, string moduleName, ModuleParameters parameters)
        {
            if (assemblyName == null)
            {
                throw new ArgumentNullException("assemblyName");
            }
            if (moduleName == null)
            {
                throw new ArgumentNullException("moduleName");
            }
            Mixin.CheckParameters(parameters);
            if (parameters.Kind == ModuleKind.NetModule)
            {
                throw new ArgumentException("kind");
            }
            AssemblyDefinition assembly = ModuleDefinition.CreateModule(moduleName, parameters).Assembly;
            assembly.Name = assemblyName;
            return assembly;
        }

        private static AssemblyDefinition ReadAssembly(ModuleDefinition module)
        {
            AssemblyDefinition assembly = module.Assembly;
            if (assembly == null)
            {
                throw new ArgumentException();
            }
            return assembly;
        }

        public static AssemblyDefinition ReadAssembly(Stream stream) => 
            ReadAssembly(ModuleDefinition.ReadModule(stream));

        public static AssemblyDefinition ReadAssembly(string fileName) => 
            ReadAssembly(ModuleDefinition.ReadModule(fileName));

        public static AssemblyDefinition ReadAssembly(Stream stream, ReaderParameters parameters) => 
            ReadAssembly(ModuleDefinition.ReadModule(stream, parameters));

        public static AssemblyDefinition ReadAssembly(string fileName, ReaderParameters parameters) => 
            ReadAssembly(ModuleDefinition.ReadModule(fileName, parameters));

        public override string ToString() => 
            this.FullName;

        public void Write(Stream stream)
        {
            this.Write(stream, new WriterParameters());
        }

        public void Write(string fileName)
        {
            this.Write(fileName, new WriterParameters());
        }

        public void Write(Stream stream, WriterParameters parameters)
        {
            this.main_module.Write(stream, parameters);
        }

        public void Write(string fileName, WriterParameters parameters)
        {
            this.main_module.Write(fileName, parameters);
        }

        public AssemblyNameDefinition Name
        {
            get => 
                this.name;
            set => 
                (this.name = value);
        }

        public string FullName =>
            ((this.name != null) ? this.name.FullName : string.Empty);

        public Mono.Cecil.MetadataToken MetadataToken
        {
            get => 
                new Mono.Cecil.MetadataToken(TokenType.Assembly, 1);
            set
            {
            }
        }

        public Collection<ModuleDefinition> Modules
        {
            get
            {
                if (this.modules != null)
                {
                    return this.modules;
                }
                if (!this.main_module.HasImage)
                {
                    Collection<ModuleDefinition> collection2;
                    Collection<ModuleDefinition> collection = new Collection<ModuleDefinition>(1) {
                        this.main_module
                    };
                    this.modules = collection2 = collection;
                    return collection2;
                }
                return this.main_module.Read<AssemblyDefinition, Collection<ModuleDefinition>>(ref this.modules, this, (_, reader) => reader.ReadModules());
            }
        }

        public ModuleDefinition MainModule =>
            this.main_module;

        public MethodDefinition EntryPoint
        {
            get => 
                this.main_module.EntryPoint;
            set => 
                (this.main_module.EntryPoint = value);
        }

        public bool HasCustomAttributes =>
            ((this.custom_attributes == null) ? this.GetHasCustomAttributes(this.main_module) : (this.custom_attributes.Count > 0));

        public Collection<CustomAttribute> CustomAttributes =>
            (this.custom_attributes ?? this.GetCustomAttributes(ref this.custom_attributes, this.main_module));

        public bool HasSecurityDeclarations =>
            ((this.security_declarations == null) ? this.GetHasSecurityDeclarations(this.main_module) : (this.security_declarations.Count > 0));

        public Collection<SecurityDeclaration> SecurityDeclarations =>
            (this.security_declarations ?? this.GetSecurityDeclarations(ref this.security_declarations, this.main_module));
    }
}

