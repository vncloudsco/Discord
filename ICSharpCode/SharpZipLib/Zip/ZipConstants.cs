namespace ICSharpCode.SharpZipLib.Zip
{
    using System;
    using System.Text;

    internal sealed class ZipConstants
    {
        public const int VersionMadeBy = 0x33;
        [Obsolete("Use VersionMadeBy instead")]
        public const int VERSION_MADE_BY = 0x33;
        public const int VersionStrongEncryption = 50;
        [Obsolete("Use VersionStrongEncryption instead")]
        public const int VERSION_STRONG_ENCRYPTION = 50;
        public const int VERSION_AES = 0x33;
        public const int VersionZip64 = 0x2d;
        public const int LocalHeaderBaseSize = 30;
        [Obsolete("Use LocalHeaderBaseSize instead")]
        public const int LOCHDR = 30;
        public const int Zip64DataDescriptorSize = 0x18;
        public const int DataDescriptorSize = 0x10;
        [Obsolete("Use DataDescriptorSize instead")]
        public const int EXTHDR = 0x10;
        public const int CentralHeaderBaseSize = 0x2e;
        [Obsolete("Use CentralHeaderBaseSize instead")]
        public const int CENHDR = 0x2e;
        public const int EndOfCentralRecordBaseSize = 0x16;
        [Obsolete("Use EndOfCentralRecordBaseSize instead")]
        public const int ENDHDR = 0x16;
        public const int CryptoHeaderSize = 12;
        [Obsolete("Use CryptoHeaderSize instead")]
        public const int CRYPTO_HEADER_SIZE = 12;
        public const int LocalHeaderSignature = 0x4034b50;
        [Obsolete("Use LocalHeaderSignature instead")]
        public const int LOCSIG = 0x4034b50;
        public const int SpanningSignature = 0x8074b50;
        [Obsolete("Use SpanningSignature instead")]
        public const int SPANNINGSIG = 0x8074b50;
        public const int SpanningTempSignature = 0x30304b50;
        [Obsolete("Use SpanningTempSignature instead")]
        public const int SPANTEMPSIG = 0x30304b50;
        public const int DataDescriptorSignature = 0x8074b50;
        [Obsolete("Use DataDescriptorSignature instead")]
        public const int EXTSIG = 0x8074b50;
        [Obsolete("Use CentralHeaderSignature instead")]
        public const int CENSIG = 0x2014b50;
        public const int CentralHeaderSignature = 0x2014b50;
        public const int Zip64CentralFileHeaderSignature = 0x6064b50;
        [Obsolete("Use Zip64CentralFileHeaderSignature instead")]
        public const int CENSIG64 = 0x6064b50;
        public const int Zip64CentralDirLocatorSignature = 0x7064b50;
        public const int ArchiveExtraDataSignature = 0x7064b50;
        public const int CentralHeaderDigitalSignature = 0x5054b50;
        [Obsolete("Use CentralHeaderDigitalSignaure instead")]
        public const int CENDIGITALSIG = 0x5054b50;
        public const int EndOfCentralDirectorySignature = 0x6054b50;
        [Obsolete("Use EndOfCentralDirectorySignature instead")]
        public const int ENDSIG = 0x6054b50;
        private static int defaultCodePage = (((Thread.CurrentThread.CurrentCulture.TextInfo.OEMCodePage == 1) || ((Thread.CurrentThread.CurrentCulture.TextInfo.OEMCodePage == 2) || ((Thread.CurrentThread.CurrentCulture.TextInfo.OEMCodePage == 3) || (Thread.CurrentThread.CurrentCulture.TextInfo.OEMCodePage == 0x2a)))) ? 0x1b5 : Thread.CurrentThread.CurrentCulture.TextInfo.OEMCodePage);

        private ZipConstants()
        {
        }

        public static byte[] ConvertToArray(string str) => 
            ((str != null) ? Encoding.GetEncoding(DefaultCodePage).GetBytes(str) : new byte[0]);

        public static byte[] ConvertToArray(int flags, string str) => 
            ((str != null) ? (((flags & 0x800) == 0) ? ConvertToArray(str) : Encoding.UTF8.GetBytes(str)) : new byte[0]);

        public static string ConvertToString(byte[] data) => 
            ((data != null) ? ConvertToString(data, data.Length) : string.Empty);

        public static string ConvertToString(byte[] data, int count) => 
            ((data != null) ? Encoding.GetEncoding(DefaultCodePage).GetString(data, 0, count) : string.Empty);

        public static string ConvertToStringExt(int flags, byte[] data) => 
            ((data != null) ? (((flags & 0x800) == 0) ? ConvertToString(data, data.Length) : Encoding.UTF8.GetString(data, 0, data.Length)) : string.Empty);

        public static string ConvertToStringExt(int flags, byte[] data, int count) => 
            ((data != null) ? (((flags & 0x800) == 0) ? ConvertToString(data, count) : Encoding.UTF8.GetString(data, 0, count)) : string.Empty);

        public static int DefaultCodePage
        {
            get => 
                defaultCodePage;
            set
            {
                if ((value < 0) || ((value > 0xffff) || ((value == 1) || ((value == 2) || ((value == 3) || (value == 0x2a))))))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                defaultCodePage = value;
            }
        }
    }
}

