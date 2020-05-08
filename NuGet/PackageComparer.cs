namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal class PackageComparer : IComparer<IPackage>
    {
        public static readonly PackageComparer Version = new PackageComparer((x, y) => x.Version.CompareTo(y.Version));
        public static readonly PackageComparer IdVersion;
        private readonly Func<IPackage, IPackage, int> _compareTo;

        static PackageComparer()
        {
            IdVersion = new PackageComparer(delegate (IPackage x, IPackage y) {
                int num = string.Compare(x.Id, y.Id, StringComparison.OrdinalIgnoreCase);
                return (num != 0) ? num : x.Version.CompareTo(y.Version);
            });
        }

        private PackageComparer(Func<IPackage, IPackage, int> compareTo)
        {
            this._compareTo = compareTo;
        }

        public int Compare(IPackage x, IPackage y) => 
            (((x != null) || (y != null)) ? ((x != null) ? ((y != null) ? this._compareTo(x, y) : 1) : -1) : 0);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PackageComparer.<>c <>9 = new PackageComparer.<>c();

            internal int <.cctor>b__5_0(IPackage x, IPackage y) => 
                x.Version.CompareTo(y.Version);

            internal int <.cctor>b__5_1(IPackage x, IPackage y)
            {
                int num = string.Compare(x.Id, y.Id, StringComparison.OrdinalIgnoreCase);
                return ((num != 0) ? num : x.Version.CompareTo(y.Version));
            }
        }
    }
}

