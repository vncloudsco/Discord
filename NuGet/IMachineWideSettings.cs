namespace NuGet
{
    using System.Collections.Generic;

    internal interface IMachineWideSettings
    {
        IEnumerable<NuGet.Settings> Settings { get; }
    }
}

