namespace NuGet
{
    using System.Collections.Generic;

    internal interface IDependentsResolver
    {
        IEnumerable<IPackage> GetDependents(IPackage package);
    }
}

