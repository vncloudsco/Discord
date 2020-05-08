namespace Mono.Cecil.Cil
{
    using Mono.Cecil;
    using Mono.Collections.Generic;
    using System;

    internal sealed class MethodSymbols
    {
        internal int code_size;
        internal string method_name;
        internal MetadataToken method_token;
        internal MetadataToken local_var_token;
        internal Collection<VariableDefinition> variables;
        internal Collection<InstructionSymbol> instructions;

        public MethodSymbols(MetadataToken methodToken)
        {
            this.method_token = methodToken;
        }

        internal MethodSymbols(string methodName)
        {
            this.method_name = methodName;
        }

        public bool HasVariables =>
            !this.variables.IsNullOrEmpty<VariableDefinition>();

        public Collection<VariableDefinition> Variables
        {
            get
            {
                if (this.variables == null)
                {
                    this.variables = new Collection<VariableDefinition>();
                }
                return this.variables;
            }
        }

        public Collection<InstructionSymbol> Instructions
        {
            get
            {
                if (this.instructions == null)
                {
                    this.instructions = new Collection<InstructionSymbol>();
                }
                return this.instructions;
            }
        }

        public int CodeSize =>
            this.code_size;

        public string MethodName =>
            this.method_name;

        public MetadataToken MethodToken =>
            this.method_token;

        public MetadataToken LocalVarToken =>
            this.local_var_token;
    }
}

