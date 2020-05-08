namespace ICSharpCode.SharpZipLib.Zip.Compression
{
    using System;

    internal class DeflaterPending : PendingBuffer
    {
        public DeflaterPending() : base(0x10000)
        {
        }
    }
}

