namespace Squirrel
{
    using System;

    internal interface IDeltaPackageBuilder
    {
        ReleasePackage ApplyDeltaPackage(ReleasePackage basePackage, ReleasePackage deltaPackage, string outputFile);
        ReleasePackage CreateDeltaPackage(ReleasePackage basePackage, ReleasePackage newPackage, string outputFile);
    }
}

