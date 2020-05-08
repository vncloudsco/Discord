namespace Mono.Cecil
{
    using Mono.Cecil.Cil;
    using Mono.Cecil.PE;
    using System;

    internal abstract class ModuleReader
    {
        protected readonly Image image;
        protected readonly ModuleDefinition module;

        protected ModuleReader(Image image, ReadingMode mode)
        {
            this.image = image;
            this.module = new ModuleDefinition(image);
            this.module.ReadingMode = mode;
        }

        public static ModuleDefinition CreateModuleFrom(Image image, ReaderParameters parameters)
        {
            ModuleReader reader = CreateModuleReader(image, parameters.ReadingMode);
            ModuleDefinition module = reader.module;
            if (parameters.AssemblyResolver != null)
            {
                module.assembly_resolver = parameters.AssemblyResolver;
            }
            if (parameters.MetadataResolver != null)
            {
                module.metadata_resolver = parameters.MetadataResolver;
            }
            reader.ReadModule();
            ReadSymbols(module, parameters);
            return module;
        }

        private static ModuleReader CreateModuleReader(Image image, ReadingMode mode)
        {
            switch (mode)
            {
                case ReadingMode.Immediate:
                    return new ImmediateModuleReader(image);

                case ReadingMode.Deferred:
                    return new DeferredModuleReader(image);
            }
            throw new ArgumentException();
        }

        private void ReadAssembly(MetadataReader reader)
        {
            AssemblyNameDefinition definition = reader.ReadAssemblyNameDefinition();
            if (definition == null)
            {
                this.module.kind = ModuleKind.NetModule;
            }
            else
            {
                AssemblyDefinition definition2 = new AssemblyDefinition {
                    Name = definition
                };
                this.module.assembly = definition2;
                definition2.main_module = this.module;
            }
        }

        protected abstract void ReadModule();
        protected void ReadModuleManifest(MetadataReader reader)
        {
            reader.Populate(this.module);
            this.ReadAssembly(reader);
        }

        private static void ReadSymbols(ModuleDefinition module, ReaderParameters parameters)
        {
            ISymbolReaderProvider symbolReaderProvider = parameters.SymbolReaderProvider;
            if ((symbolReaderProvider == null) && parameters.ReadSymbols)
            {
                symbolReaderProvider = SymbolProvider.GetPlatformReaderProvider();
            }
            if (symbolReaderProvider != null)
            {
                module.SymbolReaderProvider = symbolReaderProvider;
                ISymbolReader reader = (parameters.SymbolStream != null) ? symbolReaderProvider.GetSymbolReader(module, parameters.SymbolStream) : symbolReaderProvider.GetSymbolReader(module, module.FullyQualifiedName);
                module.ReadSymbols(reader);
            }
        }
    }
}

