namespace ICSharpCode.SharpZipLib.Zip
{
    using System;
    using System.IO;

    internal interface IDynamicDataSource
    {
        Stream GetSource(ZipEntry entry, string name);
    }
}

