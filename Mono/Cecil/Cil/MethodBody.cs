namespace Mono.Cecil.Cil
{
    using Mono.Cecil;
    using Mono.Collections.Generic;
    using System;
    using System.Threading;

    internal sealed class MethodBody : IVariableDefinitionProvider
    {
        internal readonly MethodDefinition method;
        internal ParameterDefinition this_parameter;
        internal int max_stack_size;
        internal int code_size;
        internal bool init_locals;
        internal MetadataToken local_var_token;
        internal Collection<Instruction> instructions;
        internal Collection<ExceptionHandler> exceptions;
        internal Collection<VariableDefinition> variables;
        private Mono.Cecil.Cil.Scope scope;

        public MethodBody(MethodDefinition method)
        {
            this.method = method;
        }

        private static ParameterDefinition CreateThisParameter(MethodDefinition method)
        {
            TypeDefinition declaringType = method.DeclaringType;
            return new ParameterDefinition((declaringType.IsValueType || declaringType.IsPrimitive) ? ((TypeReference) new PointerType(declaringType)) : ((TypeReference) declaringType), method);
        }

        public ILProcessor GetILProcessor() => 
            new ILProcessor(this);

        public MethodDefinition Method =>
            this.method;

        public int MaxStackSize
        {
            get => 
                this.max_stack_size;
            set => 
                (this.max_stack_size = value);
        }

        public int CodeSize =>
            this.code_size;

        public bool InitLocals
        {
            get => 
                this.init_locals;
            set => 
                (this.init_locals = value);
        }

        public MetadataToken LocalVarToken
        {
            get => 
                this.local_var_token;
            set => 
                (this.local_var_token = value);
        }

        public Collection<Instruction> Instructions
        {
            get
            {
                Collection<Instruction> instructions = this.instructions;
                if (this.instructions == null)
                {
                    Collection<Instruction> local1 = this.instructions;
                    instructions = this.instructions = new InstructionCollection();
                }
                return instructions;
            }
        }

        public bool HasExceptionHandlers =>
            !this.exceptions.IsNullOrEmpty<ExceptionHandler>();

        public Collection<ExceptionHandler> ExceptionHandlers
        {
            get
            {
                Collection<ExceptionHandler> exceptions = this.exceptions;
                if (this.exceptions == null)
                {
                    Collection<ExceptionHandler> local1 = this.exceptions;
                    exceptions = this.exceptions = new Collection<ExceptionHandler>();
                }
                return exceptions;
            }
        }

        public bool HasVariables =>
            !this.variables.IsNullOrEmpty<VariableDefinition>();

        public Collection<VariableDefinition> Variables
        {
            get
            {
                Collection<VariableDefinition> variables = this.variables;
                if (this.variables == null)
                {
                    Collection<VariableDefinition> local1 = this.variables;
                    variables = this.variables = new VariableDefinitionCollection();
                }
                return variables;
            }
        }

        public Mono.Cecil.Cil.Scope Scope
        {
            get => 
                this.scope;
            set => 
                (this.scope = value);
        }

        public ParameterDefinition ThisParameter
        {
            get
            {
                if ((this.method == null) || (this.method.DeclaringType == null))
                {
                    throw new NotSupportedException();
                }
                if (!this.method.HasThis)
                {
                    return null;
                }
                if (this.this_parameter == null)
                {
                    Interlocked.CompareExchange<ParameterDefinition>(ref this.this_parameter, CreateThisParameter(this.method), null);
                }
                return this.this_parameter;
            }
        }
    }
}

