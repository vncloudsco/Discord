namespace Mono.Cecil.Cil
{
    using System;
    using System.Runtime.InteropServices;

    internal interface ISymbolWriter : IDisposable
    {
        bool GetDebugHeader(out ImageDebugDirectory directory, out byte[] header);
        void Write(MethodBody body);
        void Write(MethodSymbols symbols);
    }
}

