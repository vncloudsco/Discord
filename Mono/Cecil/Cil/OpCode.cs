namespace Mono.Cecil.Cil
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct OpCode
    {
        private readonly byte op1;
        private readonly byte op2;
        private readonly byte code;
        private readonly byte flow_control;
        private readonly byte opcode_type;
        private readonly byte operand_type;
        private readonly byte stack_behavior_pop;
        private readonly byte stack_behavior_push;
        public string Name =>
            OpCodeNames.names[(int) this.Code];
        public int Size =>
            ((this.op1 == 0xff) ? 1 : 2);
        public byte Op1 =>
            this.op1;
        public byte Op2 =>
            this.op2;
        public short Value =>
            ((this.op1 == 0xff) ? this.op2 : ((short) ((this.op1 << 8) | this.op2)));
        public Mono.Cecil.Cil.Code Code =>
            ((Mono.Cecil.Cil.Code) this.code);
        public Mono.Cecil.Cil.FlowControl FlowControl =>
            ((Mono.Cecil.Cil.FlowControl) this.flow_control);
        public Mono.Cecil.Cil.OpCodeType OpCodeType =>
            ((Mono.Cecil.Cil.OpCodeType) this.opcode_type);
        public Mono.Cecil.Cil.OperandType OperandType =>
            ((Mono.Cecil.Cil.OperandType) this.operand_type);
        public StackBehaviour StackBehaviourPop =>
            ((StackBehaviour) this.stack_behavior_pop);
        public StackBehaviour StackBehaviourPush =>
            ((StackBehaviour) this.stack_behavior_push);
        internal OpCode(int x, int y)
        {
            this.op1 = (byte) (x & 0xff);
            this.op2 = (byte) ((x >> 8) & 0xff);
            this.code = (byte) ((x >> 0x10) & 0xff);
            this.flow_control = (byte) ((x >> 0x18) & 0xff);
            this.opcode_type = (byte) (y & 0xff);
            this.operand_type = (byte) ((y >> 8) & 0xff);
            this.stack_behavior_pop = (byte) ((y >> 0x10) & 0xff);
            this.stack_behavior_push = (byte) ((y >> 0x18) & 0xff);
            if (this.op1 == 0xff)
            {
                OpCodes.OneByteOpCode[this.op2] = this;
            }
            else
            {
                OpCodes.TwoBytesOpCode[this.op2] = this;
            }
        }

        public override int GetHashCode() => 
            this.Value;

        public override bool Equals(object obj)
        {
            if (!(obj is OpCode))
            {
                return false;
            }
            OpCode code = (OpCode) obj;
            return ((this.op1 == code.op1) && (this.op2 == code.op2));
        }

        public bool Equals(OpCode opcode) => 
            ((this.op1 == opcode.op1) && (this.op2 == opcode.op2));

        public static bool operator ==(OpCode one, OpCode other) => 
            ((one.op1 == other.op1) && (one.op2 == other.op2));

        public static bool operator !=(OpCode one, OpCode other) => 
            ((one.op1 != other.op1) || (one.op2 != other.op2));

        public override string ToString() => 
            this.Name;
    }
}

