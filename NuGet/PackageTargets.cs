namespace NuGet
{
    using System;

    [Flags]
    internal enum PackageTargets
    {
        None,
        Project,
        External,
        All
    }
}

