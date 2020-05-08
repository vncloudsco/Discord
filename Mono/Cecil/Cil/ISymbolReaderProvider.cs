namespace Mono.Cecil.Cil
{
    using Mono.Cecil;
    using System;
    using System.IO;

    internal interface ISymbolReaderProvider
    {
        ISymbolReader GetSymbolReader(ModuleDefinition module, Stream symbolStream);
        ISymbolReader GetSymbolReader(ModuleDefinition module, string fileName);
    }
}

