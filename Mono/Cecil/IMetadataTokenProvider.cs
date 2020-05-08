namespace Mono.Cecil
{
    using System;

    internal interface IMetadataTokenProvider
    {
        Mono.Cecil.MetadataToken MetadataToken { get; set; }
    }
}

