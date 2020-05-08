namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;

    internal class PackageReference : IEquatable<PackageReference>
    {
        public PackageReference(string id, SemanticVersion version, IVersionSpec versionConstraint, FrameworkName targetFramework, bool isDevelopmentDependency, bool requireReinstallation = false)
        {
            this.Id = id;
            this.Version = version;
            this.VersionConstraint = versionConstraint;
            this.TargetFramework = targetFramework;
            this.IsDevelopmentDependency = isDevelopmentDependency;
            this.RequireReinstallation = requireReinstallation;
        }

        public bool Equals(PackageReference other) => 
            (this.Id.Equals(other.Id, StringComparison.OrdinalIgnoreCase) && (this.Version == other.Version));

        public override bool Equals(object obj)
        {
            PackageReference other = obj as PackageReference;
            return ((other != null) && this.Equals(other));
        }

        public override int GetHashCode() => 
            ((this.Id.GetHashCode() * 0xc41) + ((this.Version == null) ? 0 : this.Version.GetHashCode()));

        public override string ToString()
        {
            if (this.Version == null)
            {
                return this.Id;
            }
            if (this.VersionConstraint == null)
            {
                return (this.Id + " " + this.Version);
            }
            object[] objArray1 = new object[] { this.Id, " ", this.Version, " (", this.VersionConstraint, ")" };
            return string.Concat(objArray1);
        }

        public string Id { get; private set; }

        public SemanticVersion Version { get; private set; }

        public IVersionSpec VersionConstraint { get; set; }

        public FrameworkName TargetFramework { get; private set; }

        public bool IsDevelopmentDependency { get; private set; }

        public bool RequireReinstallation { get; private set; }
    }
}

