namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;

    internal interface ICustomAttribute
    {
        TypeReference AttributeType { get; }

        bool HasFields { get; }

        bool HasProperties { get; }

        Collection<CustomAttributeNamedArgument> Fields { get; }

        Collection<CustomAttributeNamedArgument> Properties { get; }
    }
}

