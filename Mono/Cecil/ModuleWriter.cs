namespace Mono.Cecil
{
    using Mono.Cecil.Cil;
    using Mono.Cecil.PE;
    using System;
    using System.IO;

    internal static class ModuleWriter
    {
        private static void BuildMetadata(ModuleDefinition module, MetadataBuilder metadata)
        {
            if (!module.HasImage)
            {
                metadata.BuildMetadata();
            }
            else
            {
                module.Read<MetadataBuilder, MetadataBuilder>(metadata, delegate (MetadataBuilder builder, MetadataReader _) {
                    builder.BuildMetadata();
                    return builder;
                });
            }
        }

        private static ISymbolWriter GetSymbolWriter(ModuleDefinition module, string fq_name, ISymbolWriterProvider symbol_writer_provider) => 
            symbol_writer_provider?.GetSymbolWriter(module, fq_name);

        public static void WriteModuleTo(ModuleDefinition module, Stream stream, WriterParameters parameters)
        {
            if ((module.Attributes & ModuleAttributes.ILOnly) == 0)
            {
                throw new NotSupportedException("Writing mixed-mode assemblies is not supported");
            }
            if (module.HasImage && (module.ReadingMode == ReadingMode.Deferred))
            {
                ImmediateModuleReader.ReadModule(module);
            }
            module.MetadataSystem.Clear();
            AssemblyNameDefinition name = module.assembly?.Name;
            string fullyQualifiedName = stream.GetFullyQualifiedName();
            ISymbolWriterProvider symbolWriterProvider = parameters.SymbolWriterProvider;
            if ((symbolWriterProvider == null) && parameters.WriteSymbols)
            {
                symbolWriterProvider = SymbolProvider.GetPlatformWriterProvider();
            }
            ISymbolWriter writer = GetSymbolWriter(module, fullyQualifiedName, symbolWriterProvider);
            if ((parameters.StrongNameKeyPair != null) && (name != null))
            {
                name.PublicKey = parameters.StrongNameKeyPair.PublicKey;
                module.Attributes |= ModuleAttributes.StrongNameSigned;
            }
            MetadataBuilder metadata = new MetadataBuilder(module, fullyQualifiedName, symbolWriterProvider, writer);
            BuildMetadata(module, metadata);
            if (module.symbol_reader != null)
            {
                module.symbol_reader.Dispose();
            }
            ImageWriter writer2 = ImageWriter.CreateWriter(module, metadata, stream);
            writer2.WriteImage();
            if (parameters.StrongNameKeyPair != null)
            {
                CryptoService.StrongName(stream, writer2, parameters.StrongNameKeyPair);
            }
            if (writer != null)
            {
                writer.Dispose();
            }
        }
    }
}

