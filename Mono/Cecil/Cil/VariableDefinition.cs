namespace Mono.Cecil.Cil
{
    using Mono.Cecil;
    using System;

    internal sealed class VariableDefinition : VariableReference
    {
        public VariableDefinition(TypeReference variableType) : base(variableType)
        {
        }

        public VariableDefinition(string name, TypeReference variableType) : base(name, variableType)
        {
        }

        public override VariableDefinition Resolve() => 
            this;

        public bool IsPinned =>
            base.variable_type.IsPinned;
    }
}

