namespace Mono.Cecil.Cil
{
    using Mono.Cecil;
    using Mono.Cecil.PE;
    using Mono.Collections.Generic;
    using System;
    using System.Runtime.InteropServices;

    internal sealed class CodeReader : ByteBuffer
    {
        internal readonly MetadataReader reader;
        private int start;
        private Section code_section;
        private MethodDefinition method;
        private MethodBody body;

        public CodeReader(Section section, MetadataReader reader) : base(section.Data)
        {
            this.code_section = section;
            this.reader = reader;
        }

        private void Align(int align)
        {
            align--;
            base.Advance(((base.position + align) & ~align) - base.position);
        }

        public CallSite GetCallSite(MetadataToken token) => 
            this.reader.ReadCallSite(token);

        private Instruction GetInstruction(int offset) => 
            GetInstruction(this.body.Instructions, offset);

        private static Instruction GetInstruction(Collection<Instruction> instructions, int offset)
        {
            int size = instructions.size;
            Instruction[] items = instructions.items;
            if ((offset >= 0) && (offset <= items[size - 1].offset))
            {
                int num2 = 0;
                int num3 = size - 1;
                while (num2 <= num3)
                {
                    int index = num2 + ((num3 - num2) / 2);
                    Instruction instruction = items[index];
                    int num5 = instruction.offset;
                    if (offset == num5)
                    {
                        return instruction;
                    }
                    if (offset < num5)
                    {
                        num3 = index - 1;
                        continue;
                    }
                    num2 = index + 1;
                }
            }
            return null;
        }

        private static MetadataToken GetOriginalToken(MetadataBuilder metadata, MethodDefinition method)
        {
            MetadataToken token;
            return (!metadata.TryGetOriginalMethodToken(method.token, out token) ? MetadataToken.Zero : token);
        }

        public ParameterDefinition GetParameter(int index) => 
            this.body.GetParameter(index);

        public string GetString(MetadataToken token) => 
            this.reader.image.UserStringHeap.Read(token.RID);

        public VariableDefinition GetVariable(int index) => 
            this.body.GetVariable(index);

        private bool IsInSection(int rva) => 
            ((this.code_section.VirtualAddress <= rva) && (rva < (this.code_section.VirtualAddress + this.code_section.SizeOfRawData)));

        public void MoveTo(int rva)
        {
            if (!this.IsInSection(rva))
            {
                this.code_section = this.reader.image.GetSectionAtVirtualAddress((uint) rva);
                base.Reset(this.code_section.Data);
            }
            base.position = rva - ((int) this.code_section.VirtualAddress);
        }

        private void PatchRawCode(ByteBuffer buffer, int code_size, CodeWriter writer)
        {
            MetadataBuilder metadata = writer.metadata;
            buffer.WriteBytes(base.ReadBytes(code_size));
            int position = buffer.position;
            buffer.position -= code_size;
            while (buffer.position < position)
            {
                OpCode code;
                byte index = buffer.ReadByte();
                if (index != 0xfe)
                {
                    code = OpCodes.OneByteOpCode[index];
                }
                else
                {
                    byte num3 = buffer.ReadByte();
                    code = OpCodes.TwoBytesOpCode[num3];
                }
                switch (code.OperandType)
                {
                    case OperandType.InlineBrTarget:
                    case OperandType.InlineI:
                    case OperandType.ShortInlineR:
                        buffer.position += 4;
                        break;

                    case OperandType.InlineField:
                    case OperandType.InlineMethod:
                    case OperandType.InlineTok:
                    case OperandType.InlineType:
                        buffer.position -= 4;
                        buffer.WriteUInt32(metadata.LookupToken(this.reader.LookupToken(new MetadataToken(buffer.ReadUInt32()))).ToUInt32());
                        break;

                    case OperandType.InlineI8:
                    case OperandType.InlineR:
                        buffer.position += 8;
                        break;

                    case OperandType.InlineSig:
                    {
                        CallSite callSite = this.GetCallSite(new MetadataToken(buffer.ReadUInt32()));
                        buffer.position -= 4;
                        buffer.WriteUInt32(writer.GetStandAloneSignature(callSite).ToUInt32());
                        break;
                    }
                    case OperandType.InlineString:
                    {
                        string str = this.GetString(new MetadataToken(buffer.ReadUInt32()));
                        buffer.position -= 4;
                        buffer.WriteUInt32(new MetadataToken(TokenType.String, metadata.user_string_heap.GetStringIndex(str)).ToUInt32());
                        break;
                    }
                    case OperandType.InlineSwitch:
                    {
                        int num4 = buffer.ReadInt32();
                        buffer.position += num4 * 4;
                        break;
                    }
                    case OperandType.InlineVar:
                    case OperandType.InlineArg:
                        buffer.position += 2;
                        break;

                    case OperandType.ShortInlineBrTarget:
                    case OperandType.ShortInlineI:
                    case OperandType.ShortInlineVar:
                    case OperandType.ShortInlineArg:
                        buffer.position++;
                        break;

                    default:
                        break;
                }
            }
        }

        private void PatchRawExceptionHandlers(ByteBuffer buffer, MetadataBuilder metadata, int count, bool fat_entry)
        {
            for (int i = 0; i < count; i++)
            {
                ExceptionHandlerType type;
                if (fat_entry)
                {
                    uint num2 = base.ReadUInt32();
                    type = ((ExceptionHandlerType) num2) & (ExceptionHandlerType.Fault | ExceptionHandlerType.Finally | ExceptionHandlerType.Filter);
                    buffer.WriteUInt32(num2);
                }
                else
                {
                    ushort num3 = base.ReadUInt16();
                    type = ((ExceptionHandlerType) num3) & (ExceptionHandlerType.Fault | ExceptionHandlerType.Finally | ExceptionHandlerType.Filter);
                    buffer.WriteUInt16(num3);
                }
                buffer.WriteBytes(this.ReadBytes(fat_entry ? 0x10 : 6));
                if (type != ExceptionHandlerType.Catch)
                {
                    buffer.WriteUInt32(base.ReadUInt32());
                }
                else
                {
                    IMetadataTokenProvider token = this.reader.LookupToken(this.ReadToken());
                    buffer.WriteUInt32(metadata.LookupToken(token).ToUInt32());
                }
            }
        }

        private void PatchRawFatMethod(ByteBuffer buffer, MethodSymbols symbols, CodeWriter writer, out MetadataToken local_var_token)
        {
            ushort num = base.ReadUInt16();
            buffer.WriteUInt16(num);
            buffer.WriteUInt16(base.ReadUInt16());
            symbols.code_size = base.ReadInt32();
            buffer.WriteInt32(symbols.code_size);
            local_var_token = this.ReadToken();
            if (local_var_token.RID <= 0)
            {
                buffer.WriteUInt32(0);
            }
            else
            {
                Collection<VariableDefinition> collection = symbols.variables = this.ReadVariables(local_var_token);
                buffer.WriteUInt32((collection != null) ? writer.GetStandAloneSignature(symbols.variables).ToUInt32() : 0);
            }
            this.PatchRawCode(buffer, symbols.code_size, writer);
            if ((num & 8) != 0)
            {
                this.PatchRawSection(buffer, writer.metadata);
            }
        }

        private void PatchRawFatSection(ByteBuffer buffer, MetadataBuilder metadata)
        {
            base.position--;
            int num = base.ReadInt32();
            buffer.WriteInt32(num);
            int count = (num >> 8) / 0x18;
            this.PatchRawExceptionHandlers(buffer, metadata, count, true);
        }

        public ByteBuffer PatchRawMethodBody(MethodDefinition method, CodeWriter writer, out MethodSymbols symbols)
        {
            MetadataToken zero;
            ByteBuffer buffer = new ByteBuffer();
            symbols = new MethodSymbols(method.Name);
            this.method = method;
            this.reader.context = method;
            this.MoveTo(method.RVA);
            byte num = base.ReadByte();
            switch ((num & 3))
            {
                case 2:
                    buffer.WriteByte(num);
                    zero = MetadataToken.Zero;
                    symbols.code_size = num >> 2;
                    this.PatchRawCode(buffer, symbols.code_size, writer);
                    break;

                case 3:
                    base.position--;
                    this.PatchRawFatMethod(buffer, symbols, writer, out zero);
                    break;

                default:
                    throw new NotSupportedException();
            }
            ISymbolReader reader = this.reader.module.symbol_reader;
            if ((reader != null) && writer.metadata.write_symbols)
            {
                symbols.method_token = GetOriginalToken(writer.metadata, method);
                symbols.local_var_token = zero;
                reader.Read(symbols);
            }
            return buffer;
        }

        private void PatchRawSection(ByteBuffer buffer, MetadataBuilder metadata)
        {
            int position = base.position;
            this.Align(4);
            buffer.WriteBytes((int) (base.position - position));
            byte num2 = base.ReadByte();
            if ((num2 & 0x40) != 0)
            {
                this.PatchRawFatSection(buffer, metadata);
            }
            else
            {
                buffer.WriteByte(num2);
                this.PatchRawSmallSection(buffer, metadata);
            }
            if ((num2 & 0x80) != 0)
            {
                this.PatchRawSection(buffer, metadata);
            }
        }

        private void PatchRawSmallSection(ByteBuffer buffer, MetadataBuilder metadata)
        {
            byte num = base.ReadByte();
            buffer.WriteByte(num);
            base.Advance(2);
            buffer.WriteUInt16(0);
            int count = num / 12;
            this.PatchRawExceptionHandlers(buffer, metadata, count, false);
        }

        private void ReadCode()
        {
            this.start = base.position;
            int num = this.body.code_size;
            if ((num < 0) || (base.buffer.Length <= ((ulong) (num + base.position))))
            {
                num = 0;
            }
            int num2 = this.start + num;
            Collection<Instruction> instructions = this.body.instructions = new InstructionCollection((num + 1) / 2);
            while (base.position < num2)
            {
                int offset = base.position - this.start;
                OpCode opCode = this.ReadOpCode();
                Instruction instruction = new Instruction(offset, opCode);
                if (opCode.OperandType != OperandType.InlineNone)
                {
                    instruction.operand = this.ReadOperand(instruction);
                }
                instructions.Add(instruction);
            }
            this.ResolveBranches(instructions);
        }

        private void ReadExceptionHandlers(int count, Func<int> read_entry, Func<int> read_length)
        {
            for (int i = 0; i < count; i++)
            {
                ExceptionHandler handler = new ExceptionHandler(read_entry() & 7) {
                    TryStart = this.GetInstruction(read_entry())
                };
                handler.TryEnd = this.GetInstruction(handler.TryStart.Offset + read_length());
                handler.HandlerStart = this.GetInstruction(read_entry());
                handler.HandlerEnd = this.GetInstruction(handler.HandlerStart.Offset + read_length());
                this.ReadExceptionHandlerSpecific(handler);
                this.body.ExceptionHandlers.Add(handler);
            }
        }

        private void ReadExceptionHandlerSpecific(ExceptionHandler handler)
        {
            switch (handler.HandlerType)
            {
                case ExceptionHandlerType.Catch:
                    handler.CatchType = (TypeReference) this.reader.LookupToken(this.ReadToken());
                    return;

                case ExceptionHandlerType.Filter:
                    handler.FilterStart = this.GetInstruction(base.ReadInt32());
                    return;
            }
            base.Advance(4);
        }

        private void ReadFatMethod()
        {
            ushort num = base.ReadUInt16();
            this.body.max_stack_size = base.ReadUInt16();
            this.body.code_size = (int) base.ReadUInt32();
            this.body.local_var_token = new MetadataToken(base.ReadUInt32());
            this.body.init_locals = (num & 0x10) != 0;
            if (this.body.local_var_token.RID != 0)
            {
                this.body.variables = this.ReadVariables(this.body.local_var_token);
            }
            this.ReadCode();
            if ((num & 8) != 0)
            {
                this.ReadSection();
            }
        }

        private void ReadFatSection()
        {
            base.position--;
            int count = (base.ReadInt32() >> 8) / 0x18;
            this.ReadExceptionHandlers(count, new Func<int>(this.ReadInt32), new Func<int>(this.ReadInt32));
        }

        private void ReadMethodBody()
        {
            this.MoveTo(this.method.RVA);
            byte num = base.ReadByte();
            switch ((num & 3))
            {
                case 2:
                    this.body.code_size = num >> 2;
                    this.body.MaxStackSize = 8;
                    this.ReadCode();
                    break;

                case 3:
                    base.position--;
                    this.ReadFatMethod();
                    break;

                default:
                    throw new InvalidOperationException();
            }
            ISymbolReader reader = this.reader.module.symbol_reader;
            if (reader != null)
            {
                Collection<Instruction> instructions = this.body.Instructions;
                reader.Read(this.body, offset => GetInstruction(instructions, offset));
            }
        }

        public MethodBody ReadMethodBody(MethodDefinition method)
        {
            this.method = method;
            this.body = new MethodBody(method);
            this.reader.context = method;
            this.ReadMethodBody();
            return this.body;
        }

        private OpCode ReadOpCode()
        {
            byte index = base.ReadByte();
            return ((index != 0xfe) ? OpCodes.OneByteOpCode[index] : OpCodes.TwoBytesOpCode[base.ReadByte()]);
        }

        private object ReadOperand(Instruction instruction)
        {
            switch (instruction.opcode.OperandType)
            {
                case OperandType.InlineBrTarget:
                    return (base.ReadInt32() + this.Offset);

                case OperandType.InlineField:
                case OperandType.InlineMethod:
                case OperandType.InlineTok:
                case OperandType.InlineType:
                    return this.reader.LookupToken(this.ReadToken());

                case OperandType.InlineI:
                    return base.ReadInt32();

                case OperandType.InlineI8:
                    return base.ReadInt64();

                case OperandType.InlineR:
                    return base.ReadDouble();

                case OperandType.InlineSig:
                    return this.GetCallSite(this.ReadToken());

                case OperandType.InlineString:
                    return this.GetString(this.ReadToken());

                case OperandType.InlineSwitch:
                {
                    int num = base.ReadInt32();
                    int num2 = this.Offset + (4 * num);
                    int[] numArray = new int[num];
                    for (int i = 0; i < num; i++)
                    {
                        numArray[i] = num2 + base.ReadInt32();
                    }
                    return numArray;
                }
                case OperandType.InlineVar:
                    return this.GetVariable(base.ReadUInt16());

                case OperandType.InlineArg:
                    return this.GetParameter(base.ReadUInt16());

                case OperandType.ShortInlineBrTarget:
                    return (base.ReadSByte() + this.Offset);

                case OperandType.ShortInlineI:
                    return (!(instruction.opcode == OpCodes.Ldc_I4_S) ? ((object) base.ReadByte()) : ((object) base.ReadSByte()));

                case OperandType.ShortInlineR:
                    return base.ReadSingle();

                case OperandType.ShortInlineVar:
                    return this.GetVariable(base.ReadByte());

                case OperandType.ShortInlineArg:
                    return this.GetParameter(base.ReadByte());
            }
            throw new NotSupportedException();
        }

        private void ReadSection()
        {
            this.Align(4);
            byte num = base.ReadByte();
            if ((num & 0x40) == 0)
            {
                this.ReadSmallSection();
            }
            else
            {
                this.ReadFatSection();
            }
            if ((num & 0x80) != 0)
            {
                this.ReadSection();
            }
        }

        private void ReadSmallSection()
        {
            int count = base.ReadByte() / 12;
            base.Advance(2);
            this.ReadExceptionHandlers(count, () => base.ReadUInt16(), () => base.ReadByte());
        }

        public MetadataToken ReadToken() => 
            new MetadataToken(base.ReadUInt32());

        public VariableDefinitionCollection ReadVariables(MetadataToken local_var_token)
        {
            int position = this.reader.position;
            VariableDefinitionCollection definitions = this.reader.ReadVariables(local_var_token);
            this.reader.position = position;
            return definitions;
        }

        private void ResolveBranches(Collection<Instruction> instructions)
        {
            Instruction[] items = instructions.items;
            int size = instructions.size;
            int index = 0;
            while (true)
            {
                while (true)
                {
                    if (index >= size)
                    {
                        return;
                    }
                    Instruction instruction = items[index];
                    OperandType operandType = instruction.opcode.OperandType;
                    if (operandType != OperandType.InlineBrTarget)
                    {
                        if (operandType == OperandType.InlineSwitch)
                        {
                            int[] operand = (int[]) instruction.operand;
                            Instruction[] instructionArray2 = new Instruction[operand.Length];
                            int num3 = 0;
                            while (true)
                            {
                                if (num3 >= operand.Length)
                                {
                                    instruction.operand = instructionArray2;
                                    break;
                                }
                                instructionArray2[num3] = this.GetInstruction(operand[num3]);
                                num3++;
                            }
                            break;
                        }
                        if (operandType != OperandType.ShortInlineBrTarget)
                        {
                            break;
                        }
                    }
                    instruction.operand = this.GetInstruction((int) instruction.operand);
                    break;
                }
                index++;
            }
        }

        private int Offset =>
            (base.position - this.start);
    }
}

