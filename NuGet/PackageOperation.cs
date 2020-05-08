namespace NuGet
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;

    internal class PackageOperation
    {
        public PackageOperation(IPackage package, PackageAction action)
        {
            this.Package = package;
            this.Action = action;
            this.Target = PackageOperationTarget.Project;
        }

        public override bool Equals(object obj)
        {
            PackageOperation operation = obj as PackageOperation;
            return ((operation != null) && ((operation.Action == this.Action) && (operation.Package.Id.Equals(this.Package.Id, StringComparison.OrdinalIgnoreCase) && operation.Package.Version.Equals(this.Package.Version))));
        }

        public override int GetHashCode()
        {
            HashCodeCombiner combiner1 = new HashCodeCombiner();
            combiner1.AddObject(this.Package.Id);
            combiner1.AddObject(this.Package.Version);
            combiner1.AddObject(this.Action);
            return combiner1.CombinedHash;
        }

        public override string ToString()
        {
            object[] objArray1 = new object[3];
            object[] objArray2 = new object[3];
            objArray2[0] = (this.Action == PackageAction.Install) ? "+" : "-";
            object[] args = objArray2;
            args[1] = this.Package.Id;
            args[2] = this.Package.Version;
            return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2}", args);
        }

        public IPackage Package { get; private set; }

        public PackageAction Action { get; private set; }

        public PackageOperationTarget Target { get; set; }
    }
}

