namespace Mono.Cecil
{
    using Mono.Cecil.Cil;
    using System;
    using System.IO;
    using System.Reflection;

    internal sealed class WriterParameters
    {
        private Stream symbol_stream;
        private ISymbolWriterProvider symbol_writer_provider;
        private bool write_symbols;
        private System.Reflection.StrongNameKeyPair key_pair;

        public Stream SymbolStream
        {
            get => 
                this.symbol_stream;
            set => 
                (this.symbol_stream = value);
        }

        public ISymbolWriterProvider SymbolWriterProvider
        {
            get => 
                this.symbol_writer_provider;
            set => 
                (this.symbol_writer_provider = value);
        }

        public bool WriteSymbols
        {
            get => 
                this.write_symbols;
            set => 
                (this.write_symbols = value);
        }

        public System.Reflection.StrongNameKeyPair StrongNameKeyPair
        {
            get => 
                this.key_pair;
            set => 
                (this.key_pair = value);
        }
    }
}

