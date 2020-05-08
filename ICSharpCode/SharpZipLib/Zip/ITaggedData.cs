namespace ICSharpCode.SharpZipLib.Zip
{
    using System;

    internal interface ITaggedData
    {
        byte[] GetData();
        void SetData(byte[] data, int offset, int count);

        short TagID { get; }
    }
}

