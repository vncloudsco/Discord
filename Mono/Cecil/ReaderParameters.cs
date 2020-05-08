namespace Mono.Cecil
{
    using Mono.Cecil.Cil;
    using System;
    using System.IO;

    internal sealed class ReaderParameters
    {
        private Mono.Cecil.ReadingMode reading_mode;
        private IAssemblyResolver assembly_resolver;
        private IMetadataResolver metadata_resolver;
        private Stream symbol_stream;
        private ISymbolReaderProvider symbol_reader_provider;
        private bool read_symbols;

        public ReaderParameters() : this(Mono.Cecil.ReadingMode.Deferred)
        {
        }

        public ReaderParameters(Mono.Cecil.ReadingMode readingMode)
        {
            this.reading_mode = readingMode;
        }

        public Mono.Cecil.ReadingMode ReadingMode
        {
            get => 
                this.reading_mode;
            set => 
                (this.reading_mode = value);
        }

        public IAssemblyResolver AssemblyResolver
        {
            get => 
                this.assembly_resolver;
            set => 
                (this.assembly_resolver = value);
        }

        public IMetadataResolver MetadataResolver
        {
            get => 
                this.metadata_resolver;
            set => 
                (this.metadata_resolver = value);
        }

        public Stream SymbolStream
        {
            get => 
                this.symbol_stream;
            set => 
                (this.symbol_stream = value);
        }

        public ISymbolReaderProvider SymbolReaderProvider
        {
            get => 
                this.symbol_reader_provider;
            set => 
                (this.symbol_reader_provider = value);
        }

        public bool ReadSymbols
        {
            get => 
                this.read_symbols;
            set => 
                (this.read_symbols = value);
        }
    }
}

