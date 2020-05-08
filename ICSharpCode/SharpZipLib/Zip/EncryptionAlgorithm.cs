namespace ICSharpCode.SharpZipLib.Zip
{
    using System;

    internal enum EncryptionAlgorithm
    {
        None = 0,
        PkzipClassic = 1,
        Des = 0x6601,
        RC2 = 0x6602,
        TripleDes168 = 0x6603,
        TripleDes112 = 0x6609,
        Aes128 = 0x660e,
        Aes192 = 0x660f,
        Aes256 = 0x6610,
        RC2Corrected = 0x6702,
        Blowfish = 0x6720,
        Twofish = 0x6721,
        RC4 = 0x6801,
        Unknown = 0xffff
    }
}

