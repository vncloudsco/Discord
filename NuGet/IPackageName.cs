namespace NuGet
{
    using System;

    internal interface IPackageName
    {
        string Id { get; }

        SemanticVersion Version { get; }
    }
}

