namespace ICSharpCode.SharpZipLib.Encryption
{
    using ICSharpCode.SharpZipLib.Checksums;
    using System;
    using System.Security.Cryptography;

    internal abstract class PkzipClassic : SymmetricAlgorithm
    {
        protected PkzipClassic()
        {
        }

        public static byte[] GenerateKeys(byte[] seed)
        {
            if (seed == null)
            {
                throw new ArgumentNullException("seed");
            }
            if (seed.Length == 0)
            {
                throw new ArgumentException("Length is zero", "seed");
            }
            uint[] numArray = new uint[] { 0x12345678, 0x23456789, 0x34567890 };
            for (int i = 0; i < seed.Length; i++)
            {
                numArray[0] = Crc32.ComputeCrc32(numArray[0], seed[i]);
                numArray[1] += (byte) numArray[0];
                numArray[1] = (numArray[1] * 0x8088405) + 1;
                numArray[2] = Crc32.ComputeCrc32(numArray[2], (byte) (numArray[1] >> 0x18));
            }
            byte[] buffer1 = new byte[12];
            buffer1[0] = (byte) (numArray[0] & 0xff);
            buffer1[1] = (byte) ((numArray[0] >> 8) & 0xff);
            buffer1[2] = (byte) ((numArray[0] >> 0x10) & 0xff);
            buffer1[3] = (byte) ((numArray[0] >> 0x18) & 0xff);
            buffer1[4] = (byte) (numArray[1] & 0xff);
            buffer1[5] = (byte) ((numArray[1] >> 8) & 0xff);
            buffer1[6] = (byte) ((numArray[1] >> 0x10) & 0xff);
            buffer1[7] = (byte) ((numArray[1] >> 0x18) & 0xff);
            buffer1[8] = (byte) (numArray[2] & 0xff);
            buffer1[9] = (byte) ((numArray[2] >> 8) & 0xff);
            buffer1[10] = (byte) ((numArray[2] >> 0x10) & 0xff);
            buffer1[11] = (byte) ((numArray[2] >> 0x18) & 0xff);
            return buffer1;
        }
    }
}

