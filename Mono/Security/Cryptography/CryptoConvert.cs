namespace Mono.Security.Cryptography
{
    using System;
    using System.Security.Cryptography;

    internal static class CryptoConvert
    {
        public static RSA FromCapiKeyBlob(byte[] blob) => 
            FromCapiKeyBlob(blob, 0);

        public static RSA FromCapiKeyBlob(byte[] blob, int offset)
        {
            if (blob == null)
            {
                throw new ArgumentNullException("blob");
            }
            if (offset >= blob.Length)
            {
                throw new ArgumentException("blob is too small.");
            }
            byte num = blob[offset];
            if (num == 0)
            {
                if (blob[offset + 12] == 6)
                {
                    return FromCapiPublicKeyBlob(blob, offset + 12);
                }
            }
            else
            {
                switch (num)
                {
                    case 6:
                        return FromCapiPublicKeyBlob(blob, offset);

                    case 7:
                        return FromCapiPrivateKeyBlob(blob, offset);

                    default:
                        break;
                }
            }
            throw new CryptographicException("Unknown blob format.");
        }

        private static RSA FromCapiPrivateKeyBlob(byte[] blob, int offset)
        {
            RSAParameters parameters = new RSAParameters();
            try
            {
                if ((blob[offset] != 7) || ((blob[offset + 1] != 2) || ((blob[offset + 2] != 0) || ((blob[offset + 3] != 0) || (ToUInt32LE(blob, offset + 8) != 0x32415352)))))
                {
                    throw new CryptographicException("Invalid blob header");
                }
                byte[] dst = new byte[4];
                Buffer.BlockCopy(blob, offset + 0x10, dst, 0, 4);
                Array.Reverse(dst);
                parameters.Exponent = Trim(dst);
                int srcOffset = offset + 20;
                int count = ToInt32LE(blob, offset + 12) >> 3;
                parameters.Modulus = new byte[count];
                Buffer.BlockCopy(blob, srcOffset, parameters.Modulus, 0, count);
                Array.Reverse(parameters.Modulus);
                srcOffset += count;
                int num4 = count >> 1;
                parameters.P = new byte[num4];
                Buffer.BlockCopy(blob, srcOffset, parameters.P, 0, num4);
                Array.Reverse(parameters.P);
                srcOffset += num4;
                parameters.Q = new byte[num4];
                Buffer.BlockCopy(blob, srcOffset, parameters.Q, 0, num4);
                Array.Reverse(parameters.Q);
                srcOffset += num4;
                parameters.DP = new byte[num4];
                Buffer.BlockCopy(blob, srcOffset, parameters.DP, 0, num4);
                Array.Reverse(parameters.DP);
                srcOffset += num4;
                parameters.DQ = new byte[num4];
                Buffer.BlockCopy(blob, srcOffset, parameters.DQ, 0, num4);
                Array.Reverse(parameters.DQ);
                srcOffset += num4;
                parameters.InverseQ = new byte[num4];
                Buffer.BlockCopy(blob, srcOffset, parameters.InverseQ, 0, num4);
                Array.Reverse(parameters.InverseQ);
                srcOffset += num4;
                parameters.D = new byte[count];
                if (((srcOffset + count) + offset) <= blob.Length)
                {
                    Buffer.BlockCopy(blob, srcOffset, parameters.D, 0, count);
                    Array.Reverse(parameters.D);
                }
            }
            catch (Exception exception)
            {
                throw new CryptographicException("Invalid blob.", exception);
            }
            RSA rsa = null;
            try
            {
                rsa = RSA.Create();
                rsa.ImportParameters(parameters);
            }
            catch (CryptographicException)
            {
                bool flag = false;
                try
                {
                    CspParameters parameters2 = new CspParameters {
                        Flags = CspProviderFlags.UseMachineKeyStore
                    };
                    new RSACryptoServiceProvider(parameters2).ImportParameters(parameters);
                }
                catch
                {
                    flag = true;
                }
                if (flag)
                {
                    throw;
                }
            }
            return rsa;
        }

        private static RSA FromCapiPublicKeyBlob(byte[] blob, int offset)
        {
            RSA rsa2;
            try
            {
                if ((blob[offset] != 6) || ((blob[offset + 1] != 2) || ((blob[offset + 2] != 0) || ((blob[offset + 3] != 0) || (ToUInt32LE(blob, offset + 8) != 0x31415352)))))
                {
                    throw new CryptographicException("Invalid blob header");
                }
                RSAParameters parameters = new RSAParameters {
                    Exponent = new byte[] { 
                        blob[offset + 0x12],
                        blob[offset + 0x11],
                        blob[offset + 0x10]
                    }
                };
                int srcOffset = offset + 20;
                int count = ToInt32LE(blob, offset + 12) >> 3;
                parameters.Modulus = new byte[count];
                Buffer.BlockCopy(blob, srcOffset, parameters.Modulus, 0, count);
                Array.Reverse(parameters.Modulus);
                RSA rsa = null;
                try
                {
                    RSA.Create().ImportParameters(parameters);
                }
                catch (CryptographicException)
                {
                    CspParameters parameters2 = new CspParameters {
                        Flags = CspProviderFlags.UseMachineKeyStore
                    };
                    rsa = new RSACryptoServiceProvider(parameters2);
                    rsa.ImportParameters(parameters);
                }
                rsa2 = rsa;
            }
            catch (Exception exception)
            {
                throw new CryptographicException("Invalid blob.", exception);
            }
            return rsa2;
        }

        private static int ToInt32LE(byte[] bytes, int offset) => 
            ((((bytes[offset + 3] << 0x18) | (bytes[offset + 2] << 0x10)) | (bytes[offset + 1] << 8)) | bytes[offset]);

        private static uint ToUInt32LE(byte[] bytes, int offset) => 
            ((uint) ((((bytes[offset + 3] << 0x18) | (bytes[offset + 2] << 0x10)) | (bytes[offset + 1] << 8)) | bytes[offset]));

        private static byte[] Trim(byte[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != 0)
                {
                    byte[] dst = new byte[array.Length - i];
                    Buffer.BlockCopy(array, i, dst, 0, dst.Length);
                    return dst;
                }
            }
            return null;
        }
    }
}

