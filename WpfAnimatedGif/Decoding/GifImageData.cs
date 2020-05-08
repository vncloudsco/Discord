namespace WpfAnimatedGif.Decoding
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal class GifImageData
    {
        private GifImageData()
        {
        }

        private void Read(Stream stream, bool metadataOnly)
        {
            this.LzwMinimumCodeSize = (byte) stream.ReadByte();
            this.CompressedData = GifHelpers.ReadDataBlocks(stream, metadataOnly);
        }

        internal static GifImageData ReadImageData(Stream stream, bool metadataOnly)
        {
            GifImageData data = new GifImageData();
            data.Read(stream, metadataOnly);
            return data;
        }

        public byte LzwMinimumCodeSize { get; set; }

        public byte[] CompressedData { get; set; }
    }
}

