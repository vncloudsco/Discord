namespace Squirrel
{
    using NuGet;
    using System;

    internal static class VersionComparer
    {
        public static bool Matches(IVersionSpec versionSpec, SemanticVersion version)
        {
            if (versionSpec == null)
            {
                return true;
            }
            bool flag = (versionSpec.MinVersion != null) ? (!versionSpec.IsMinInclusive ? (version > versionSpec.MinVersion) : (version >= versionSpec.MinVersion)) : true;
            bool flag2 = (versionSpec.MaxVersion != null) ? (!versionSpec.IsMaxInclusive ? (version < versionSpec.MaxVersion) : (version <= versionSpec.MaxVersion)) : true;
            return (flag2 & flag);
        }
    }
}

