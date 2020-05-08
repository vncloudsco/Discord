namespace Mono.Cecil
{
    using System;

    internal interface IMetadataScope : IMetadataTokenProvider
    {
        Mono.Cecil.MetadataScopeType MetadataScopeType { get; }

        string Name { get; set; }
    }
}

