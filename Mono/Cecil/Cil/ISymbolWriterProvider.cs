namespace Mono.Cecil.Cil
{
    using Mono.Cecil;
    using System;
    using System.IO;

    internal interface ISymbolWriterProvider
    {
        ISymbolWriter GetSymbolWriter(ModuleDefinition module, Stream symbolStream);
        ISymbolWriter GetSymbolWriter(ModuleDefinition module, string fileName);
    }
}

