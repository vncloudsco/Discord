namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;

    internal interface IGenericInstance : IMetadataTokenProvider
    {
        bool HasGenericArguments { get; }

        Collection<TypeReference> GenericArguments { get; }
    }
}

