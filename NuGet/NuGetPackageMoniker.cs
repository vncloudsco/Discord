namespace NuGet
{
    using Microsoft.VisualStudio.ProjectSystem.Interop;
    using System;
    using System.Runtime.CompilerServices;

    internal class NuGetPackageMoniker : INuGetPackageMoniker
    {
        public string Id { get; set; }

        public string Version { get; set; }
    }
}

