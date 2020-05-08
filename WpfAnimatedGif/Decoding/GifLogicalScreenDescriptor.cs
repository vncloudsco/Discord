namespace WpfAnimatedGif.Decoding
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal class GifLogicalScreenDescriptor
    {
        private void Read(Stream stream)
        {
            byte[] buffer = new byte[7];
            stream.ReadAll(buffer, 0, buffer.Length);
            this.Width = BitConverter.ToUInt16(buffer, 0);
            this.Height = BitConverter.ToUInt16(buffer, 2);
            byte num = buffer[4];
            this.HasGlobalColorTable = (num & 0x80) != 0;
            this.ColorResolution = ((num & 0x70) >> 4) + 1;
            this.IsGlobalColorTableSorted = (num & 8) != 0;
            this.GlobalColorTableSize = 1 << (((num & 7) + 1) & 0x1f);
            this.BackgroundColorIndex = buffer[5];
            this.PixelAspectRatio = (buffer[5] == 0) ? 0.0 : (((double) (15 + buffer[5])) / 64.0);
        }

        internal static GifLogicalScreenDescriptor ReadLogicalScreenDescriptor(Stream stream)
        {
            GifLogicalScreenDescriptor descriptor = new GifLogicalScreenDescriptor();
            descriptor.Read(stream);
            return descriptor;
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public bool HasGlobalColorTable { get; private set; }

        public int ColorResolution { get; private set; }

        public bool IsGlobalColorTableSorted { get; private set; }

        public int GlobalColorTableSize { get; private set; }

        public int BackgroundColorIndex { get; private set; }

        public double PixelAspectRatio { get; private set; }
    }
}

