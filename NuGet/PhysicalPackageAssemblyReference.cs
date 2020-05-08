namespace NuGet
{
    using System;
    using System.IO;

    internal class PhysicalPackageAssemblyReference : PhysicalPackageFile, IPackageAssemblyReference, IPackageFile, IFrameworkTargetable
    {
        public PhysicalPackageAssemblyReference()
        {
        }

        public PhysicalPackageAssemblyReference(PhysicalPackageFile file) : base(file)
        {
        }

        public PhysicalPackageAssemblyReference(Func<Stream> streamFactory) : base(streamFactory)
        {
        }

        public string Name =>
            (string.IsNullOrEmpty(base.Path) ? string.Empty : Path.GetFileName(base.Path));
    }
}

