namespace NuGet
{
    using System;
    using System.Collections.Generic;

    internal interface IBatchProcessor<in T>
    {
        void BeginProcessing(IEnumerable<T> batch, PackageAction action);
        void EndProcessing();
    }
}

