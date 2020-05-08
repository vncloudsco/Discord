namespace ICSharpCode.SharpZipLib.Core
{
    using System;

    internal interface INameTransform
    {
        string TransformDirectory(string name);
        string TransformFile(string name);
    }
}

