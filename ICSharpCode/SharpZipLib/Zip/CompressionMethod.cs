﻿namespace ICSharpCode.SharpZipLib.Zip
{
    using System;

    internal enum CompressionMethod
    {
        Stored = 0,
        Deflated = 8,
        Deflate64 = 9,
        BZip2 = 11,
        WinZipAES = 0x63
    }
}

