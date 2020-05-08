namespace ICSharpCode.SharpZipLib.Zip
{
    using System.IO;

    internal interface IStaticDataSource
    {
        Stream GetSource();
    }
}

