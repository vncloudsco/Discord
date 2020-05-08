namespace NuGet
{
    using System;
    using System.IO;
    using System.Text;

    internal class ZipPackageAssemblyReference : ZipPackageFile, IPackageAssemblyReference, IPackageFile, IFrameworkTargetable
    {
        public ZipPackageAssemblyReference(IPackageFile file) : base(file)
        {
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            if (base.TargetFramework != null)
            {
                builder.Append(base.TargetFramework).Append(" ");
            }
            builder.Append(this.Name).AppendFormat(" ({0})", base.Path);
            return builder.ToString();
        }

        public string Name =>
            Path.GetFileName(base.Path);
    }
}

