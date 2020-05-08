namespace Mono.Cecil
{
    using System;

    [Flags]
    internal enum ManifestResourceAttributes : uint
    {
        VisibilityMask = 7,
        Public = 1,
        Private = 2
    }
}

