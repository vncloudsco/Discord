namespace ICSharpCode.SharpZipLib.GZip
{
    using ICSharpCode.SharpZipLib.Checksums;
    using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
    using System;
    using System.IO;

    internal class GZipOutputStream : DeflaterOutputStream
    {
        protected Crc32 crc;
        private OutputState state_;

        public GZipOutputStream(Stream baseOutputStream) : this(baseOutputStream, 0x1000)
        {
        }

        public GZipOutputStream(Stream baseOutputStream, int size) : base(baseOutputStream, new Deflater(-1, true), size)
        {
            this.crc = new Crc32();
        }

        public override void Close()
        {
            try
            {
                this.Finish();
            }
            finally
            {
                if (this.state_ != OutputState.Closed)
                {
                    this.state_ = OutputState.Closed;
                    if (base.IsStreamOwner)
                    {
                        base.baseOutputStream_.Close();
                    }
                }
            }
        }

        public override void Finish()
        {
            if (this.state_ == OutputState.Header)
            {
                this.WriteHeader();
            }
            if (this.state_ == OutputState.Footer)
            {
                this.state_ = OutputState.Finished;
                base.Finish();
                uint num = (uint) (((ulong) base.deflater_.TotalIn) & 0xffffffffUL);
                uint num2 = (uint) (((ulong) this.crc.Value) & 0xffffffffUL);
                byte[] buffer = new byte[] { (byte) num2, (byte) (num2 >> 8), (byte) (num2 >> 0x10), (byte) (num2 >> 0x18), (byte) num, (byte) (num >> 8), (byte) (num >> 0x10), (byte) (num >> 0x18) };
                base.baseOutputStream_.Write(buffer, 0, buffer.Length);
            }
        }

        public int GetLevel() => 
            base.deflater_.GetLevel();

        public void SetLevel(int level)
        {
            if (level < 1)
            {
                throw new ArgumentOutOfRangeException("level");
            }
            base.deflater_.SetLevel(level);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this.state_ == OutputState.Header)
            {
                this.WriteHeader();
            }
            if (this.state_ != OutputState.Footer)
            {
                throw new InvalidOperationException("Write not permitted in current state");
            }
            this.crc.Update(buffer, offset, count);
            base.Write(buffer, offset, count);
        }

        private void WriteHeader()
        {
            if (this.state_ == OutputState.Header)
            {
                this.state_ = OutputState.Footer;
                int num = (int) ((DateTime.Now.Ticks - new DateTime(0x7b2, 1, 1).Ticks) / 0x989680L);
                byte[] buffer1 = new byte[] { 0x1f, 0x8b, 8, 0, 0, 0, 0, 0, 0, 0xff };
                buffer1[4] = (byte) num;
                buffer1[5] = (byte) (num >> 8);
                buffer1[6] = (byte) (num >> 0x10);
                buffer1[7] = (byte) (num >> 0x18);
                byte[] buffer = buffer1;
                base.baseOutputStream_.Write(buffer, 0, buffer.Length);
            }
        }

        private enum OutputState
        {
            Header,
            Footer,
            Finished,
            Closed
        }
    }
}

