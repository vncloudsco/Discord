namespace Mono.Cecil.Cil
{
    using System;

    internal interface ISymbolReader : IDisposable
    {
        bool ProcessDebugHeader(ImageDebugDirectory directory, byte[] header);
        void Read(MethodSymbols symbols);
        void Read(MethodBody body, InstructionMapper mapper);
    }
}

