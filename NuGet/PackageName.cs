namespace NuGet
{
    using System;

    internal class PackageName : IPackageName, IEquatable<PackageName>
    {
        private readonly string _packageId;
        private readonly SemanticVersion _version;

        public PackageName(string packageId, SemanticVersion version)
        {
            this._packageId = packageId;
            this._version = version;
        }

        public bool Equals(PackageName other) => 
            (this._packageId.Equals(other._packageId, StringComparison.OrdinalIgnoreCase) && this._version.Equals(other._version));

        public override int GetHashCode() => 
            ((this._packageId.GetHashCode() * 0xc41) + this._version.GetHashCode());

        public override string ToString() => 
            (this._packageId + " " + this._version);

        public string Id =>
            this._packageId;

        public SemanticVersion Version =>
            this._version;

        public string Name =>
            (this._packageId + "." + this._version.ToString());
    }
}

