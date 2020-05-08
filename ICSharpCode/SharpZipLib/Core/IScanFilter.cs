namespace ICSharpCode.SharpZipLib.Core
{
    using System;

    internal interface IScanFilter
    {
        bool IsMatch(string name);
    }
}

