namespace Mono.Cecil.Cil
{
    using Mono.Collections.Generic;
    using System;

    internal interface IVariableDefinitionProvider
    {
        bool HasVariables { get; }

        Collection<VariableDefinition> Variables { get; }
    }
}

