namespace ICSharpCode.SharpZipLib.Zip
{
    using System;

    internal class DescriptorData
    {
        private long size;
        private long compressedSize;
        private long crc;

        public long CompressedSize
        {
            get => 
                this.compressedSize;
            set => 
                (this.compressedSize = value);
        }

        public long Size
        {
            get => 
                this.size;
            set => 
                (this.size = value);
        }

        public long Crc
        {
            get => 
                this.crc;
            set => 
                (this.crc = (long) (((ulong) value) & 0xffffffffUL));
        }
    }
}

