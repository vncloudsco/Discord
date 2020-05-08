namespace NuGet
{
    using System.Collections.Generic;

    internal interface IFrameworkTargetable
    {
        IEnumerable<FrameworkName> SupportedFrameworks { get; }
    }
}

