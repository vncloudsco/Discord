﻿namespace Mono.Cecil
{
    using System;

    internal interface IMemberDefinition : ICustomAttributeProvider, IMetadataTokenProvider
    {
        string Name { get; set; }

        string FullName { get; }

        bool IsSpecialName { get; set; }

        bool IsRuntimeSpecialName { get; set; }

        TypeDefinition DeclaringType { get; set; }
    }
}

