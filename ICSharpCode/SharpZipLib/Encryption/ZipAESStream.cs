﻿namespace ICSharpCode.SharpZipLib.Encryption
{
    using System;
    using System.IO;
    using System.Security.Cryptography;

    internal class ZipAESStream : CryptoStream
    {
        private const int AUTH_CODE_LENGTH = 10;
        private Stream _stream;
        private ZipAESTransform _transform;
        private byte[] _slideBuffer;
        private int _slideBufStartPos;
        private int _slideBufFreePos;
        private const int CRYPTO_BLOCK_SIZE = 0x10;
        private int _blockAndAuth;

        public ZipAESStream(Stream stream, ZipAESTransform transform, CryptoStreamMode mode) : base(stream, transform, mode)
        {
            this._stream = stream;
            this._transform = transform;
            this._slideBuffer = new byte[0x400];
            this._blockAndAuth = 0x1a;
            if (mode != CryptoStreamMode.Read)
            {
                throw new Exception("ZipAESStream only for read");
            }
        }

        public override int Read(byte[] outBuffer, int offset, int count)
        {
            int num = 0;
            while (num < count)
            {
                int num2 = this._slideBufFreePos - this._slideBufStartPos;
                int num3 = this._blockAndAuth - num2;
                if ((this._slideBuffer.Length - this._slideBufFreePos) < num3)
                {
                    int index = 0;
                    int num6 = this._slideBufStartPos;
                    while (true)
                    {
                        if (num6 >= this._slideBufFreePos)
                        {
                            this._slideBufFreePos -= this._slideBufStartPos;
                            this._slideBufStartPos = 0;
                            break;
                        }
                        this._slideBuffer[index] = this._slideBuffer[num6];
                        num6++;
                        index++;
                    }
                }
                int num4 = this._stream.Read(this._slideBuffer, this._slideBufFreePos, num3);
                this._slideBufFreePos += num4;
                num2 = this._slideBufFreePos - this._slideBufStartPos;
                if (num2 < this._blockAndAuth)
                {
                    if (num2 > 10)
                    {
                        int inputCount = num2 - 10;
                        this._transform.TransformBlock(this._slideBuffer, this._slideBufStartPos, inputCount, outBuffer, offset);
                        num += inputCount;
                        this._slideBufStartPos += inputCount;
                    }
                    else if (num2 < 10)
                    {
                        throw new Exception("Internal error missed auth code");
                    }
                    byte[] authCode = this._transform.GetAuthCode();
                    for (int i = 0; i < 10; i++)
                    {
                        if (authCode[i] != this._slideBuffer[this._slideBufStartPos + i])
                        {
                            throw new Exception("AES Authentication Code does not match. This is a super-CRC check on the data in the file after compression and encryption. \r\nThe file may be damaged.");
                        }
                    }
                    break;
                }
                this._transform.TransformBlock(this._slideBuffer, this._slideBufStartPos, 0x10, outBuffer, offset);
                num += 0x10;
                offset += 0x10;
                this._slideBufStartPos += 0x10;
            }
            return num;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}

