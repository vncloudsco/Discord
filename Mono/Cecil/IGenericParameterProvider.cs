namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;

    internal interface IGenericParameterProvider : IMetadataTokenProvider
    {
        bool HasGenericParameters { get; }

        bool IsDefinition { get; }

        ModuleDefinition Module { get; }

        Collection<GenericParameter> GenericParameters { get; }

        Mono.Cecil.GenericParameterType GenericParameterType { get; }
    }
}

