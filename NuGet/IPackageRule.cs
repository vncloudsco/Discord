namespace NuGet
{
    using System.Collections.Generic;

    internal interface IPackageRule
    {
        IEnumerable<PackageIssue> Validate(IPackage package);
    }
}

