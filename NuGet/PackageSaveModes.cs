namespace NuGet
{
    using System;

    [Flags]
    internal enum PackageSaveModes
    {
        None,
        Nuspec,
        Nupkg
    }
}

