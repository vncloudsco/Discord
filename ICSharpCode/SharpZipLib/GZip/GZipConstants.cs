namespace ICSharpCode.SharpZipLib.GZip
{
    using System;

    internal sealed class GZipConstants
    {
        public const int GZIP_MAGIC = 0x1f8b;
        public const int FTEXT = 1;
        public const int FHCRC = 2;
        public const int FEXTRA = 4;
        public const int FNAME = 8;
        public const int FCOMMENT = 0x10;

        private GZipConstants()
        {
        }
    }
}

