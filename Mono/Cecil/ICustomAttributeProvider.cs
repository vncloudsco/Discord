namespace Mono.Cecil
{
    using Mono.Collections.Generic;
    using System;

    internal interface ICustomAttributeProvider : IMetadataTokenProvider
    {
        Collection<CustomAttribute> CustomAttributes { get; }

        bool HasCustomAttributes { get; }
    }
}

