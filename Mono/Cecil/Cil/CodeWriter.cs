namespace Mono.Cecil.Cil
{
    using Mono.Cecil;
    using Mono.Cecil.Metadata;
    using Mono.Cecil.PE;
    using Mono.Collections.Generic;
    using System;
    using System.Collections.Generic;

    internal sealed class CodeWriter : ByteBuffer
    {
        private readonly uint code_base;
        internal readonly MetadataBuilder metadata;
        private readonly Dictionary<uint, MetadataToken> standalone_signatures;
        private uint current;
        private MethodBody body;

        public CodeWriter(MetadataBuilder metadata) : base(0)
        {
            this.code_base = metadata.text_map.GetNextRVA(TextSegment.CLIHeader);
            this.current = this.code_base;
            this.metadata = metadata;
            this.standalone_signatures = new Dictionary<uint, MetadataToken>();
        }

        private static void AddExceptionStackSize(Instruction handler_start, ref Dictionary<Instruction, int> stack_sizes)
        {
            if (handler_start != null)
            {
                if (stack_sizes == null)
                {
                    stack_sizes = new Dictionary<Instruction, int>();
                }
                stack_sizes[handler_start] = 1;
            }
        }

        private void Align(int align)
        {
            align--;
            base.WriteBytes((int) (((base.position + align) & ~align) - base.position));
        }

        private uint BeginMethod() => 
            this.current;

        private void ComputeExceptionHandlerStackSize(ref Dictionary<Instruction, int> stack_sizes)
        {
            Collection<ExceptionHandler> exceptionHandlers = this.body.ExceptionHandlers;
            for (int i = 0; i < exceptionHandlers.Count; i++)
            {
                ExceptionHandler handler = exceptionHandlers[i];
                ExceptionHandlerType handlerType = handler.HandlerType;
                switch (handlerType)
                {
                    case ExceptionHandlerType.Catch:
                        AddExceptionStackSize(handler.HandlerStart, ref stack_sizes);
                        break;

                    case ExceptionHandlerType.Filter:
                        AddExceptionStackSize(handler.FilterStart, ref stack_sizes);
                        AddExceptionStackSize(handler.HandlerStart, ref stack_sizes);
                        break;

                    default:
                        break;
                }
            }
        }

        private void ComputeHeader()
        {
            int num = 0;
            Collection<Instruction> instructions = this.body.instructions;
            Instruction[] items = instructions.items;
            int size = instructions.size;
            int num3 = 0;
            int num4 = 0;
            Dictionary<Instruction, int> dictionary = null;
            if (this.body.HasExceptionHandlers)
            {
                this.ComputeExceptionHandlerStackSize(ref dictionary);
            }
            for (int i = 0; i < size; i++)
            {
                Instruction instruction = items[i];
                instruction.offset = num;
                num += instruction.GetSize();
                ComputeStackSize(instruction, ref dictionary, ref num3, ref num4);
            }
            this.body.code_size = num;
            this.body.max_stack_size = num4;
        }

        private static void ComputePopDelta(StackBehaviour pop_behavior, ref int stack_size)
        {
            switch (pop_behavior)
            {
                case StackBehaviour.Pop1:
                case StackBehaviour.Popi:
                case StackBehaviour.Popref:
                    stack_size--;
                    return;

                case StackBehaviour.Pop1_pop1:
                case StackBehaviour.Popi_pop1:
                case StackBehaviour.Popi_popi:
                case StackBehaviour.Popi_popi8:
                case StackBehaviour.Popi_popr4:
                case StackBehaviour.Popi_popr8:
                case StackBehaviour.Popref_pop1:
                case StackBehaviour.Popref_popi:
                    stack_size -= 2;
                    return;

                case StackBehaviour.Popi_popi_popi:
                case StackBehaviour.Popref_popi_popi:
                case StackBehaviour.Popref_popi_popi8:
                case StackBehaviour.Popref_popi_popr4:
                case StackBehaviour.Popref_popi_popr8:
                case StackBehaviour.Popref_popi_popref:
                    stack_size -= 3;
                    return;

                case StackBehaviour.PopAll:
                    stack_size = 0;
                    return;
            }
        }

        private static void ComputePushDelta(StackBehaviour push_behaviour, ref int stack_size)
        {
            switch (push_behaviour)
            {
                case StackBehaviour.Push1:
                case StackBehaviour.Pushi:
                case StackBehaviour.Pushi8:
                case StackBehaviour.Pushr4:
                case StackBehaviour.Pushr8:
                case StackBehaviour.Pushref:
                    stack_size++;
                    return;

                case StackBehaviour.Push1_push1:
                    stack_size += 2;
                    return;
            }
        }

        private static void ComputeStackDelta(Instruction instruction, ref int stack_size)
        {
            if (instruction.opcode.FlowControl != FlowControl.Call)
            {
                ComputePopDelta(instruction.opcode.StackBehaviourPop, ref stack_size);
                ComputePushDelta(instruction.opcode.StackBehaviourPush, ref stack_size);
            }
            else
            {
                IMethodSignature operand = (IMethodSignature) instruction.operand;
                if (operand.HasImplicitThis() && (instruction.opcode.Code != Code.Newobj))
                {
                    stack_size--;
                }
                if (operand.HasParameters)
                {
                    stack_size -= operand.Parameters.Count;
                }
                if (instruction.opcode.Code == Code.Calli)
                {
                    stack_size--;
                }
                if ((operand.ReturnType.etype != ElementType.Void) || (instruction.opcode.Code == Code.Newobj))
                {
                    stack_size++;
                }
            }
        }

        private static void ComputeStackSize(Instruction instruction, ref int stack_size)
        {
            FlowControl flowControl = instruction.opcode.FlowControl;
            switch (flowControl)
            {
                case FlowControl.Branch:
                case FlowControl.Break:
                    break;

                default:
                    switch (flowControl)
                    {
                        case FlowControl.Return:
                        case FlowControl.Throw:
                            break;

                        default:
                            return;
                    }
                    break;
            }
            stack_size = 0;
        }

        private static void ComputeStackSize(Instruction instruction, ref Dictionary<Instruction, int> stack_sizes, ref int stack_size, ref int max_stack)
        {
            int num;
            if ((stack_sizes != null) && stack_sizes.TryGetValue(instruction, out num))
            {
                stack_size = num;
            }
            max_stack = Math.Max(max_stack, stack_size);
            ComputeStackDelta(instruction, ref stack_size);
            max_stack = Math.Max(max_stack, stack_size);
            CopyBranchStackSize(instruction, ref stack_sizes, stack_size);
            ComputeStackSize(instruction, ref stack_size);
        }

        private static void CopyBranchStackSize(Instruction instruction, ref Dictionary<Instruction, int> stack_sizes, int stack_size)
        {
            if (stack_size != 0)
            {
                OperandType operandType = instruction.opcode.OperandType;
                if (operandType != OperandType.InlineBrTarget)
                {
                    if (operandType == OperandType.InlineSwitch)
                    {
                        Instruction[] operand = (Instruction[]) instruction.operand;
                        for (int i = 0; i < operand.Length; i++)
                        {
                            CopyBranchStackSize(ref stack_sizes, operand[i], stack_size);
                        }
                        return;
                    }
                    if (operandType != OperandType.ShortInlineBrTarget)
                    {
                        return;
                    }
                }
                CopyBranchStackSize(ref stack_sizes, (Instruction) instruction.operand, stack_size);
            }
        }

        private static void CopyBranchStackSize(ref Dictionary<Instruction, int> stack_sizes, Instruction target, int stack_size)
        {
            int num2;
            if (stack_sizes == null)
            {
                stack_sizes = new Dictionary<Instruction, int>();
            }
            int num = stack_size;
            if (stack_sizes.TryGetValue(target, out num2))
            {
                num = Math.Max(num, num2);
            }
            stack_sizes[target] = num;
        }

        private void EndMethod()
        {
            this.current = this.code_base + ((uint) base.position);
        }

        private static MetadataToken GetLocalVarToken(ByteBuffer buffer, MethodSymbols symbols)
        {
            if (symbols.variables.IsNullOrEmpty<VariableDefinition>())
            {
                return MetadataToken.Zero;
            }
            buffer.position = 8;
            return new MetadataToken(buffer.ReadUInt32());
        }

        private int GetParameterIndex(ParameterDefinition parameter) => 
            (!this.body.method.HasThis ? parameter.Index : (!ReferenceEquals(parameter, this.body.this_parameter) ? (parameter.Index + 1) : 0));

        public MetadataToken GetStandAloneSignature(CallSite call_site)
        {
            uint callSiteBlobIndex = this.metadata.GetCallSiteBlobIndex(call_site);
            MetadataToken standAloneSignatureToken = this.GetStandAloneSignatureToken(callSiteBlobIndex);
            call_site.MetadataToken = standAloneSignatureToken;
            return standAloneSignatureToken;
        }

        public MetadataToken GetStandAloneSignature(Collection<VariableDefinition> variables)
        {
            uint localVariableBlobIndex = this.metadata.GetLocalVariableBlobIndex(variables);
            return this.GetStandAloneSignatureToken(localVariableBlobIndex);
        }

        private MetadataToken GetStandAloneSignatureToken(uint signature)
        {
            MetadataToken token;
            if (!this.standalone_signatures.TryGetValue(signature, out token))
            {
                token = new MetadataToken(TokenType.Signature, this.metadata.AddStandAloneSignature(signature));
                this.standalone_signatures.Add(signature, token);
            }
            return token;
        }

        private int GetTargetOffset(Instruction instruction)
        {
            if (instruction != null)
            {
                return instruction.offset;
            }
            Instruction instruction2 = this.body.instructions[this.body.instructions.size - 1];
            return (instruction2.offset + instruction2.GetSize());
        }

        private uint GetUserStringIndex(string @string) => 
            ((@string != null) ? this.metadata.user_string_heap.GetStringIndex(@string) : 0);

        private static int GetVariableIndex(VariableDefinition variable) => 
            variable.Index;

        private static bool IsEmptyMethodBody(MethodBody body) => 
            (body.instructions.IsNullOrEmpty<Instruction>() && body.variables.IsNullOrEmpty<VariableDefinition>());

        private static bool IsFatRange(Instruction start, Instruction end)
        {
            if (start == null)
            {
                throw new ArgumentException();
            }
            return ((end != null) ? (((end.Offset - start.Offset) > 0xff) || (start.Offset > 0xffff)) : true);
        }

        private static bool IsUnresolved(MethodDefinition method) => 
            (method.HasBody && (method.HasImage && ReferenceEquals(method.body, null)));

        private bool RequiresFatHeader()
        {
            MethodBody body = this.body;
            return ((body.CodeSize >= 0x40) || (body.InitLocals || (body.HasVariables || (body.HasExceptionHandlers || (body.MaxStackSize > 8)))));
        }

        private static bool RequiresFatSection(Collection<ExceptionHandler> handlers)
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                ExceptionHandler handler = handlers[i];
                if (IsFatRange(handler.TryStart, handler.TryEnd))
                {
                    return true;
                }
                if (IsFatRange(handler.HandlerStart, handler.HandlerEnd))
                {
                    return true;
                }
                if ((handler.HandlerType == ExceptionHandlerType.Filter) && IsFatRange(handler.FilterStart, handler.HandlerStart))
                {
                    return true;
                }
            }
            return false;
        }

        private void WriteExceptionHandlers()
        {
            this.Align(4);
            Collection<ExceptionHandler> exceptionHandlers = this.body.ExceptionHandlers;
            if ((exceptionHandlers.Count < 0x15) && !RequiresFatSection(exceptionHandlers))
            {
                this.WriteSmallSection(exceptionHandlers);
            }
            else
            {
                this.WriteFatSection(exceptionHandlers);
            }
        }

        private void WriteExceptionHandlers(Collection<ExceptionHandler> handlers, Action<int> write_entry, Action<int> write_length)
        {
            for (int i = 0; i < handlers.Count; i++)
            {
                ExceptionHandler handler = handlers[i];
                write_entry((int) handler.HandlerType);
                write_entry(handler.TryStart.Offset);
                write_length(this.GetTargetOffset(handler.TryEnd) - handler.TryStart.Offset);
                write_entry(handler.HandlerStart.Offset);
                write_length(this.GetTargetOffset(handler.HandlerEnd) - handler.HandlerStart.Offset);
                this.WriteExceptionHandlerSpecific(handler);
            }
        }

        private void WriteExceptionHandlerSpecific(ExceptionHandler handler)
        {
            switch (handler.HandlerType)
            {
                case ExceptionHandlerType.Catch:
                    this.WriteMetadataToken(this.metadata.LookupToken(handler.CatchType));
                    return;

                case ExceptionHandlerType.Filter:
                    base.WriteInt32(handler.FilterStart.Offset);
                    return;
            }
            base.WriteInt32(0);
        }

        private void WriteFatHeader()
        {
            MethodBody body = this.body;
            byte num = 3;
            if (body.InitLocals)
            {
                num = (byte) (num | 0x10);
            }
            if (body.HasExceptionHandlers)
            {
                num = (byte) (num | 8);
            }
            base.WriteByte(num);
            base.WriteByte(0x30);
            base.WriteInt16((short) body.max_stack_size);
            base.WriteInt32(body.code_size);
            body.local_var_token = body.HasVariables ? this.GetStandAloneSignature(body.Variables) : MetadataToken.Zero;
            this.WriteMetadataToken(body.local_var_token);
        }

        private void WriteFatSection(Collection<ExceptionHandler> handlers)
        {
            base.WriteByte(0x41);
            int num = (handlers.Count * 0x18) + 4;
            base.WriteByte((byte) (num & 0xff));
            base.WriteByte((byte) ((num >> 8) & 0xff));
            base.WriteByte((byte) ((num >> 0x10) & 0xff));
            this.WriteExceptionHandlers(handlers, new Action<int>(this.WriteInt32), new Action<int>(this.WriteInt32));
        }

        private void WriteInstructions()
        {
            Collection<Instruction> instructions = this.body.Instructions;
            Instruction[] items = instructions.items;
            int size = instructions.size;
            for (int i = 0; i < size; i++)
            {
                Instruction instruction = items[i];
                this.WriteOpCode(instruction.opcode);
                this.WriteOperand(instruction);
            }
        }

        private void WriteMetadataToken(MetadataToken token)
        {
            base.WriteUInt32(token.ToUInt32());
        }

        public uint WriteMethodBody(MethodDefinition method)
        {
            uint num = this.BeginMethod();
            if (IsUnresolved(method))
            {
                if (method.rva == 0)
                {
                    return 0;
                }
                this.WriteUnresolvedMethodBody(method);
            }
            else
            {
                if (IsEmptyMethodBody(method.Body))
                {
                    return 0;
                }
                this.WriteResolvedMethodBody(method);
            }
            this.Align(4);
            this.EndMethod();
            return num;
        }

        private void WriteOpCode(OpCode opcode)
        {
            if (opcode.Size == 1)
            {
                base.WriteByte(opcode.Op2);
            }
            else
            {
                base.WriteByte(opcode.Op1);
                base.WriteByte(opcode.Op2);
            }
        }

        private void WriteOperand(Instruction instruction)
        {
            OpCode opcode = instruction.opcode;
            OperandType operandType = opcode.OperandType;
            if (operandType != OperandType.InlineNone)
            {
                object operand = instruction.operand;
                if (operand == null)
                {
                    throw new ArgumentException();
                }
                switch (operandType)
                {
                    case OperandType.InlineBrTarget:
                    {
                        Instruction instruction3 = (Instruction) operand;
                        base.WriteInt32(this.GetTargetOffset(instruction3) - ((instruction.Offset + opcode.Size) + 4));
                        return;
                    }
                    case OperandType.InlineField:
                    case OperandType.InlineMethod:
                    case OperandType.InlineTok:
                    case OperandType.InlineType:
                        this.WriteMetadataToken(this.metadata.LookupToken((IMetadataTokenProvider) operand));
                        return;

                    case OperandType.InlineI:
                        base.WriteInt32((int) operand);
                        return;

                    case OperandType.InlineI8:
                        base.WriteInt64((long) operand);
                        return;

                    case OperandType.InlineR:
                        base.WriteDouble((double) operand);
                        return;

                    case OperandType.InlineSig:
                        this.WriteMetadataToken(this.GetStandAloneSignature((CallSite) operand));
                        return;

                    case OperandType.InlineString:
                        this.WriteMetadataToken(new MetadataToken(TokenType.String, this.GetUserStringIndex((string) operand)));
                        return;

                    case OperandType.InlineSwitch:
                    {
                        Instruction[] instructionArray = (Instruction[]) operand;
                        base.WriteInt32(instructionArray.Length);
                        int num = (instruction.Offset + opcode.Size) + (4 * (instructionArray.Length + 1));
                        for (int i = 0; i < instructionArray.Length; i++)
                        {
                            base.WriteInt32(this.GetTargetOffset(instructionArray[i]) - num);
                        }
                        return;
                    }
                    case OperandType.InlineVar:
                        base.WriteInt16((short) GetVariableIndex((VariableDefinition) operand));
                        return;

                    case OperandType.InlineArg:
                        base.WriteInt16((short) this.GetParameterIndex((ParameterDefinition) operand));
                        return;

                    case OperandType.ShortInlineBrTarget:
                    {
                        Instruction instruction2 = (Instruction) operand;
                        base.WriteSByte((sbyte) (this.GetTargetOffset(instruction2) - ((instruction.Offset + opcode.Size) + 1)));
                        return;
                    }
                    case OperandType.ShortInlineI:
                        if (opcode == OpCodes.Ldc_I4_S)
                        {
                            base.WriteSByte((sbyte) operand);
                            return;
                        }
                        base.WriteByte((byte) operand);
                        return;

                    case OperandType.ShortInlineR:
                        base.WriteSingle((float) operand);
                        return;

                    case OperandType.ShortInlineVar:
                        base.WriteByte((byte) GetVariableIndex((VariableDefinition) operand));
                        return;

                    case OperandType.ShortInlineArg:
                        base.WriteByte((byte) this.GetParameterIndex((ParameterDefinition) operand));
                        return;
                }
                throw new ArgumentException();
            }
        }

        private void WriteResolvedMethodBody(MethodDefinition method)
        {
            this.body = method.Body;
            this.ComputeHeader();
            if (this.RequiresFatHeader())
            {
                this.WriteFatHeader();
            }
            else
            {
                base.WriteByte((byte) (2 | (this.body.CodeSize << 2)));
            }
            this.WriteInstructions();
            if (this.body.HasExceptionHandlers)
            {
                this.WriteExceptionHandlers();
            }
            ISymbolWriter writer = this.metadata.symbol_writer;
            if (writer != null)
            {
                writer.Write(this.body);
            }
        }

        private void WriteSmallSection(Collection<ExceptionHandler> handlers)
        {
            base.WriteByte(1);
            base.WriteByte((byte) ((handlers.Count * 12) + 4));
            base.WriteBytes(2);
            this.WriteExceptionHandlers(handlers, i => base.WriteUInt16((ushort) i), i => base.WriteByte((byte) i));
        }

        private void WriteUnresolvedMethodBody(MethodDefinition method)
        {
            MethodSymbols symbols;
            ByteBuffer buffer = this.metadata.module.Read<MethodDefinition, CodeReader>(method, (_, reader) => reader.code).PatchRawMethodBody(method, this, out symbols);
            base.WriteBytes(buffer);
            if (!symbols.instructions.IsNullOrEmpty<InstructionSymbol>())
            {
                symbols.method_token = method.token;
                symbols.local_var_token = GetLocalVarToken(buffer, symbols);
                ISymbolWriter writer = this.metadata.symbol_writer;
                if (writer != null)
                {
                    writer.Write(symbols);
                }
            }
        }
    }
}

