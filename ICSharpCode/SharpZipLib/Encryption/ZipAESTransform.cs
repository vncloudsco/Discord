namespace ICSharpCode.SharpZipLib.Encryption
{
    using System;
    using System.Security.Cryptography;

    internal class ZipAESTransform : ICryptoTransform, IDisposable
    {
        private const int PWD_VER_LENGTH = 2;
        private const int KEY_ROUNDS = 0x3e8;
        private const int ENCRYPT_BLOCK = 0x10;
        private int _blockSize;
        private ICryptoTransform _encryptor;
        private readonly byte[] _counterNonce;
        private byte[] _encryptBuffer;
        private int _encrPos;
        private byte[] _pwdVerifier;
        private HMACSHA1 _hmacsha1;
        private bool _finalised;
        private bool _writeMode;

        public ZipAESTransform(string key, byte[] saltBytes, int blockSize, bool writeMode)
        {
            if ((blockSize != 0x10) && (blockSize != 0x20))
            {
                throw new Exception("Invalid blocksize " + blockSize + ". Must be 16 or 32.");
            }
            if (saltBytes.Length != (blockSize / 2))
            {
                object[] objArray1 = new object[] { "Invalid salt len. Must be ", blockSize / 2, " for blocksize ", blockSize };
                throw new Exception(string.Concat(objArray1));
            }
            this._blockSize = blockSize;
            this._encryptBuffer = new byte[this._blockSize];
            this._encrPos = 0x10;
            Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(key, saltBytes, 0x3e8);
            RijndaelManaged managed = new RijndaelManaged {
                Mode = CipherMode.ECB
            };
            this._counterNonce = new byte[this._blockSize];
            byte[] rgbIV = bytes.GetBytes(this._blockSize);
            this._encryptor = managed.CreateEncryptor(bytes.GetBytes(this._blockSize), rgbIV);
            this._pwdVerifier = bytes.GetBytes(2);
            this._hmacsha1 = new HMACSHA1(rgbIV);
            this._writeMode = writeMode;
        }

        public void Dispose()
        {
            this._encryptor.Dispose();
        }

        public byte[] GetAuthCode()
        {
            if (!this._finalised)
            {
                byte[] inputBuffer = new byte[0];
                this._hmacsha1.TransformFinalBlock(inputBuffer, 0, 0);
                this._finalised = true;
            }
            return this._hmacsha1.Hash;
        }

        public unsafe int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (!this._writeMode)
            {
                this._hmacsha1.TransformBlock(inputBuffer, inputOffset, inputCount, inputBuffer, inputOffset);
            }
            for (int i = 0; i < inputCount; i++)
            {
                if (this._encrPos == 0x10)
                {
                    int num2 = 0;
                    while (true)
                    {
                        byte* numPtr1 = &(this._counterNonce[num2]);
                        byte num3 = (byte) (numPtr1[0] + 1);
                        numPtr1[0] = num3;
                        if (num3 != 0)
                        {
                            this._encryptor.TransformBlock(this._counterNonce, 0, this._blockSize, this._encryptBuffer, 0);
                            this._encrPos = 0;
                            break;
                        }
                        num2++;
                    }
                }
                int index = this._encrPos;
                this._encrPos = index + 1;
                outputBuffer[i + outputOffset] = (byte) (inputBuffer[i + inputOffset] ^ this._encryptBuffer[index]);
            }
            if (this._writeMode)
            {
                this._hmacsha1.TransformBlock(outputBuffer, outputOffset, inputCount, outputBuffer, outputOffset);
            }
            return inputCount;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            throw new NotImplementedException("ZipAESTransform.TransformFinalBlock");
        }

        public byte[] PwdVerifier =>
            this._pwdVerifier;

        public int InputBlockSize =>
            this._blockSize;

        public int OutputBlockSize =>
            this._blockSize;

        public bool CanTransformMultipleBlocks =>
            true;

        public bool CanReuseTransform =>
            true;
    }
}

