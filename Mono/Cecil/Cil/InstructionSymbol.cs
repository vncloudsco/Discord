namespace Mono.Cecil.Cil
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct InstructionSymbol
    {
        public readonly int Offset;
        public readonly Mono.Cecil.Cil.SequencePoint SequencePoint;
        public InstructionSymbol(int offset, Mono.Cecil.Cil.SequencePoint sequencePoint)
        {
            this.Offset = offset;
            this.SequencePoint = sequencePoint;
        }
    }
}

