namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Versioning;

    internal class FrameworkNameEqualityComparer : IEqualityComparer<FrameworkName>
    {
        public static readonly FrameworkNameEqualityComparer Default = new FrameworkNameEqualityComparer();

        private FrameworkNameEqualityComparer()
        {
        }

        public bool Equals(FrameworkName x, FrameworkName y) => 
            (string.Equals(x.get_Identifier(), y.get_Identifier(), StringComparison.OrdinalIgnoreCase) && ((x.get_Version() == y.get_Version()) && string.Equals(x.get_Profile(), y.get_Profile(), StringComparison.OrdinalIgnoreCase)));

        public int GetHashCode(FrameworkName x) => 
            x.GetHashCode();
    }
}

