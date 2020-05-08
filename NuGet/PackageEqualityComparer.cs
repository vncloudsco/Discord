namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal sealed class PackageEqualityComparer : IEqualityComparer<IPackageName>
    {
        public static readonly PackageEqualityComparer IdAndVersion = new PackageEqualityComparer((x, y) => x.Id.Equals(y.Id, StringComparison.OrdinalIgnoreCase) && x.Version.Equals(y.Version), x => x.Id.GetHashCode() ^ x.Version.GetHashCode());
        public static readonly PackageEqualityComparer Id = new PackageEqualityComparer((x, y) => x.Id.Equals(y.Id, StringComparison.OrdinalIgnoreCase), x => x.Id.GetHashCode());
        private readonly Func<IPackageName, IPackageName, bool> _equals;
        private readonly Func<IPackageName, int> _getHashCode;

        private PackageEqualityComparer(Func<IPackageName, IPackageName, bool> equals, Func<IPackageName, int> getHashCode)
        {
            this._equals = equals;
            this._getHashCode = getHashCode;
        }

        public bool Equals(IPackageName x, IPackageName y) => 
            this._equals(x, y);

        public int GetHashCode(IPackageName obj) => 
            this._getHashCode(obj);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PackageEqualityComparer.<>c <>9 = new PackageEqualityComparer.<>c();

            internal bool <.cctor>b__7_0(IPackageName x, IPackageName y) => 
                (x.Id.Equals(y.Id, StringComparison.OrdinalIgnoreCase) && x.Version.Equals(y.Version));

            internal int <.cctor>b__7_1(IPackageName x) => 
                (x.Id.GetHashCode() ^ x.Version.GetHashCode());

            internal bool <.cctor>b__7_2(IPackageName x, IPackageName y) => 
                x.Id.Equals(y.Id, StringComparison.OrdinalIgnoreCase);

            internal int <.cctor>b__7_3(IPackageName x) => 
                x.Id.GetHashCode();
        }
    }
}

