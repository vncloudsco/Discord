namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;

    internal interface IMethodSignature : IMetadataTokenProvider
    {
        bool HasThis { get; set; }

        bool ExplicitThis { get; set; }

        MethodCallingConvention CallingConvention { get; set; }

        bool HasParameters { get; }

        Collection<ParameterDefinition> Parameters { get; }

        TypeReference ReturnType { get; set; }

        Mono.Cecil.MethodReturnType MethodReturnType { get; }
    }
}

