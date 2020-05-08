namespace Mono.Cecil.Cil
{
    using Mono.Cecil;
    using Mono.Collections.Generic;
    using System;

    internal sealed class ILProcessor
    {
        private readonly MethodBody body;
        private readonly Collection<Instruction> instructions;

        internal ILProcessor(MethodBody body)
        {
            this.body = body;
            this.instructions = body.Instructions;
        }

        public void Append(Instruction instruction)
        {
            if (instruction == null)
            {
                throw new ArgumentNullException("instruction");
            }
            this.instructions.Add(instruction);
        }

        public Instruction Create(OpCode opcode) => 
            Instruction.Create(opcode);

        public Instruction Create(OpCode opcode, CallSite site) => 
            Instruction.Create(opcode, site);

        public Instruction Create(OpCode opcode, Instruction target) => 
            Instruction.Create(opcode, target);

        public Instruction Create(OpCode opcode, VariableDefinition variable) => 
            Instruction.Create(opcode, variable);

        public Instruction Create(OpCode opcode, FieldReference field) => 
            Instruction.Create(opcode, field);

        public Instruction Create(OpCode opcode, MethodReference method) => 
            Instruction.Create(opcode, method);

        public Instruction Create(OpCode opcode, ParameterDefinition parameter) => 
            Instruction.Create(opcode, parameter);

        public Instruction Create(OpCode opcode, TypeReference type) => 
            Instruction.Create(opcode, type);

        public Instruction Create(OpCode opcode, byte value) => 
            ((opcode.OperandType != OperandType.ShortInlineVar) ? ((opcode.OperandType != OperandType.ShortInlineArg) ? Instruction.Create(opcode, value) : Instruction.Create(opcode, this.body.GetParameter(value))) : Instruction.Create(opcode, this.body.Variables[value]));

        public Instruction Create(OpCode opcode, double value) => 
            Instruction.Create(opcode, value);

        public Instruction Create(OpCode opcode, int value) => 
            ((opcode.OperandType != OperandType.InlineVar) ? ((opcode.OperandType != OperandType.InlineArg) ? Instruction.Create(opcode, value) : Instruction.Create(opcode, this.body.GetParameter(value))) : Instruction.Create(opcode, this.body.Variables[value]));

        public Instruction Create(OpCode opcode, long value) => 
            Instruction.Create(opcode, value);

        public Instruction Create(OpCode opcode, sbyte value) => 
            Instruction.Create(opcode, value);

        public Instruction Create(OpCode opcode, float value) => 
            Instruction.Create(opcode, value);

        public Instruction Create(OpCode opcode, string value) => 
            Instruction.Create(opcode, value);

        public Instruction Create(OpCode opcode, Instruction[] targets) => 
            Instruction.Create(opcode, targets);

        public void Emit(OpCode opcode)
        {
            this.Append(this.Create(opcode));
        }

        public void Emit(OpCode opcode, CallSite site)
        {
            this.Append(this.Create(opcode, site));
        }

        public void Emit(OpCode opcode, Instruction target)
        {
            this.Append(this.Create(opcode, target));
        }

        public void Emit(OpCode opcode, VariableDefinition variable)
        {
            this.Append(this.Create(opcode, variable));
        }

        public void Emit(OpCode opcode, FieldReference field)
        {
            this.Append(this.Create(opcode, field));
        }

        public void Emit(OpCode opcode, MethodReference method)
        {
            this.Append(this.Create(opcode, method));
        }

        public void Emit(OpCode opcode, ParameterDefinition parameter)
        {
            this.Append(this.Create(opcode, parameter));
        }

        public void Emit(OpCode opcode, TypeReference type)
        {
            this.Append(this.Create(opcode, type));
        }

        public void Emit(OpCode opcode, byte value)
        {
            this.Append(this.Create(opcode, value));
        }

        public void Emit(OpCode opcode, double value)
        {
            this.Append(this.Create(opcode, value));
        }

        public void Emit(OpCode opcode, int value)
        {
            this.Append(this.Create(opcode, value));
        }

        public void Emit(OpCode opcode, long value)
        {
            this.Append(this.Create(opcode, value));
        }

        public void Emit(OpCode opcode, sbyte value)
        {
            this.Append(this.Create(opcode, value));
        }

        public void Emit(OpCode opcode, float value)
        {
            this.Append(this.Create(opcode, value));
        }

        public void Emit(OpCode opcode, string value)
        {
            this.Append(this.Create(opcode, value));
        }

        public void Emit(OpCode opcode, Instruction[] targets)
        {
            this.Append(this.Create(opcode, targets));
        }

        public void InsertAfter(Instruction target, Instruction instruction)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (instruction == null)
            {
                throw new ArgumentNullException("instruction");
            }
            int index = this.instructions.IndexOf(target);
            if (index == -1)
            {
                throw new ArgumentOutOfRangeException("target");
            }
            this.instructions.Insert(index + 1, instruction);
        }

        public void InsertBefore(Instruction target, Instruction instruction)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (instruction == null)
            {
                throw new ArgumentNullException("instruction");
            }
            int index = this.instructions.IndexOf(target);
            if (index == -1)
            {
                throw new ArgumentOutOfRangeException("target");
            }
            this.instructions.Insert(index, instruction);
        }

        public void Remove(Instruction instruction)
        {
            if (instruction == null)
            {
                throw new ArgumentNullException("instruction");
            }
            if (!this.instructions.Remove(instruction))
            {
                throw new ArgumentOutOfRangeException("instruction");
            }
        }

        public void Replace(Instruction target, Instruction instruction)
        {
            if (target == null)
            {
                throw new ArgumentNullException("target");
            }
            if (instruction == null)
            {
                throw new ArgumentNullException("instruction");
            }
            this.InsertAfter(target, instruction);
            this.Remove(target);
        }

        public MethodBody Body =>
            this.body;
    }
}

