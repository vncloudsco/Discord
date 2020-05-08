namespace Mono.Cecil
{
    using System;

    internal interface IMarshalInfoProvider : IMetadataTokenProvider
    {
        bool HasMarshalInfo { get; }

        Mono.Cecil.MarshalInfo MarshalInfo { get; set; }
    }
}

