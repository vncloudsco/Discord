namespace ICSharpCode.SharpZipLib.Checksums
{
    using System;

    internal interface IChecksum
    {
        void Reset();
        void Update(int value);
        void Update(byte[] buffer);
        void Update(byte[] buffer, int offset, int count);

        long Value { get; }
    }
}

