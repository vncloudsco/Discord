namespace NuGet
{
    using System;

    internal interface IVersionSpec
    {
        SemanticVersion MinVersion { get; }

        bool IsMinInclusive { get; }

        SemanticVersion MaxVersion { get; }

        bool IsMaxInclusive { get; }
    }
}

