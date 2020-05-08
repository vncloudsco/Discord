namespace NuGet
{
    using System;

    internal interface IPackageConstraintProvider
    {
        IVersionSpec GetConstraint(string packageId);

        string Source { get; }
    }
}

