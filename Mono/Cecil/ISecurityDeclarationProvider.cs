namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;

    internal interface ISecurityDeclarationProvider : IMetadataTokenProvider
    {
        bool HasSecurityDeclarations { get; }

        Collection<SecurityDeclaration> SecurityDeclarations { get; }
    }
}

