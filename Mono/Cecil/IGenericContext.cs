namespace Mono.Cecil
{
    using System;

    internal interface IGenericContext
    {
        bool IsDefinition { get; }

        IGenericParameterProvider Type { get; }

        IGenericParameterProvider Method { get; }
    }
}

