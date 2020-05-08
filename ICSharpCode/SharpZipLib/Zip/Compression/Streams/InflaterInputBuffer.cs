namespace ICSharpCode.SharpZipLib.Zip.Compression.Streams
{
    using ICSharpCode.SharpZipLib.Zip;
    using ICSharpCode.SharpZipLib.Zip.Compression;
    using System;
    using System.IO;
    using System.Security.Cryptography;

    internal class InflaterInputBuffer
    {
        private int rawLength;
        private byte[] rawData;
        private int clearTextLength;
        private byte[] clearText;
        private byte[] internalClearText;
        private int available;
        private ICryptoTransform cryptoTransform;
        private Stream inputStream;

        public InflaterInputBuffer(Stream stream) : this(stream, 0x1000)
        {
        }

        public InflaterInputBuffer(Stream stream, int bufferSize)
        {
            this.inputStream = stream;
            if (bufferSize < 0x400)
            {
                bufferSize = 0x400;
            }
            this.rawData = new byte[bufferSize];
            this.clearText = this.rawData;
        }

        public void Fill()
        {
            this.rawLength = 0;
            int length = this.rawData.Length;
            while (true)
            {
                if (length > 0)
                {
                    int num2 = this.inputStream.Read(this.rawData, this.rawLength, length);
                    if (num2 > 0)
                    {
                        this.rawLength += num2;
                        length -= num2;
                        continue;
                    }
                }
                this.clearTextLength = (this.cryptoTransform == null) ? this.rawLength : this.cryptoTransform.TransformBlock(this.rawData, 0, this.rawLength, this.clearText, 0);
                this.available = this.clearTextLength;
                return;
            }
        }

        public int ReadClearTextBuffer(byte[] outBuffer, int offset, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            int destinationIndex = offset;
            int num2 = length;
            while (num2 > 0)
            {
                if (this.available <= 0)
                {
                    this.Fill();
                    if (this.available <= 0)
                    {
                        return 0;
                    }
                }
                int num3 = Math.Min(num2, this.available);
                Array.Copy(this.clearText, this.clearTextLength - this.available, outBuffer, destinationIndex, num3);
                destinationIndex += num3;
                num2 -= num3;
                this.available -= num3;
            }
            return length;
        }

        public int ReadLeByte()
        {
            if (this.available <= 0)
            {
                this.Fill();
                if (this.available <= 0)
                {
                    throw new ZipException("EOF in header");
                }
            }
            this.available--;
            return this.rawData[this.rawLength - this.available];
        }

        public int ReadLeInt() => 
            (this.ReadLeShort() | (this.ReadLeShort() << 0x10));

        public long ReadLeLong() => 
            (((long) ((ulong) this.ReadLeInt())) | (this.ReadLeInt() << 0x20));

        public int ReadLeShort() => 
            (this.ReadLeByte() | (this.ReadLeByte() << 8));

        public int ReadRawBuffer(byte[] buffer) => 
            this.ReadRawBuffer(buffer, 0, buffer.Length);

        public int ReadRawBuffer(byte[] outBuffer, int offset, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }
            int destinationIndex = offset;
            int num2 = length;
            while (num2 > 0)
            {
                if (this.available <= 0)
                {
                    this.Fill();
                    if (this.available <= 0)
                    {
                        return 0;
                    }
                }
                int num3 = Math.Min(num2, this.available);
                Array.Copy(this.rawData, this.rawLength - this.available, outBuffer, destinationIndex, num3);
                destinationIndex += num3;
                num2 -= num3;
                this.available -= num3;
            }
            return length;
        }

        public void SetInflaterInput(Inflater inflater)
        {
            if (this.available > 0)
            {
                inflater.SetInput(this.clearText, this.clearTextLength - this.available, this.available);
                this.available = 0;
            }
        }

        public int RawLength =>
            this.rawLength;

        public byte[] RawData =>
            this.rawData;

        public int ClearTextLength =>
            this.clearTextLength;

        public byte[] ClearText =>
            this.clearText;

        public int Available
        {
            get => 
                this.available;
            set => 
                (this.available = value);
        }

        public ICryptoTransform CryptoTransform
        {
            set
            {
                this.cryptoTransform = value;
                if (this.cryptoTransform == null)
                {
                    this.clearText = this.rawData;
                    this.clearTextLength = this.rawLength;
                }
                else
                {
                    if (this.rawData == this.clearText)
                    {
                        if (this.internalClearText == null)
                        {
                            this.internalClearText = new byte[this.rawData.Length];
                        }
                        this.clearText = this.internalClearText;
                    }
                    this.clearTextLength = this.rawLength;
                    if (this.available > 0)
                    {
                        this.cryptoTransform.TransformBlock(this.rawData, this.rawLength - this.available, this.available, this.clearText, this.rawLength - this.available);
                    }
                }
            }
        }
    }
}

