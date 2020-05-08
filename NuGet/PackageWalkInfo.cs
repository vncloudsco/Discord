namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;

    internal class PackageWalkInfo
    {
        public PackageWalkInfo(PackageTargets initialTarget)
        {
            this.InitialTarget = initialTarget;
            this.Target = initialTarget;
        }

        public override string ToString()
        {
            object[] objArray1 = new object[] { "Initial Target:", this.InitialTarget, ", Current Target: ", this.Target, ", Parent: ", this.Parent };
            return string.Concat(objArray1);
        }

        public PackageTargets InitialTarget { get; private set; }

        public PackageTargets Target { get; set; }

        public IPackage Parent { get; set; }
    }
}

