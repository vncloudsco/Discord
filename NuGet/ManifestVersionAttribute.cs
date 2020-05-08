namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
    internal sealed class ManifestVersionAttribute : Attribute
    {
        public ManifestVersionAttribute(int version)
        {
            this.Version = version;
        }

        public int Version { get; private set; }
    }
}

