namespace NuGet.Runtime
{
    using System;
    using System.Collections.Generic;

    internal interface IAssembly
    {
        string Name { get; }

        System.Version Version { get; }

        string PublicKeyToken { get; }

        string Culture { get; }

        IEnumerable<IAssembly> ReferencedAssemblies { get; }
    }
}

