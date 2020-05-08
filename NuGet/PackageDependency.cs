namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;

    internal class PackageDependency
    {
        public PackageDependency(string id) : this(id, null)
        {
        }

        public PackageDependency(string id, IVersionSpec versionSpec)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "id");
            }
            this.Id = id;
            this.VersionSpec = versionSpec;
        }

        internal static PackageDependency CreateDependency(string id, string versionSpec) => 
            new PackageDependency(id, VersionUtility.ParseVersionSpec(versionSpec));

        public override string ToString() => 
            ((this.VersionSpec != null) ? (this.Id + " " + VersionUtility.PrettyPrint(this.VersionSpec)) : this.Id);

        public string Id { get; private set; }

        public IVersionSpec VersionSpec { get; private set; }
    }
}

