namespace WpfAnimatedGif.Decoding
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal class GifGraphicControlExtension : GifExtension
    {
        internal const int ExtensionLabel = 0xf9;

        private GifGraphicControlExtension()
        {
        }

        private void Read(Stream stream)
        {
            byte[] buffer = new byte[6];
            stream.ReadAll(buffer, 0, buffer.Length);
            this.BlockSize = buffer[0];
            if (this.BlockSize != 4)
            {
                throw GifHelpers.InvalidBlockSizeException("Graphic Control Extension", 4, this.BlockSize);
            }
            byte num = buffer[1];
            this.DisposalMethod = (num & 0x1c) >> 2;
            this.UserInput = (num & 2) != 0;
            this.HasTransparency = (num & 1) != 0;
            this.Delay = BitConverter.ToUInt16(buffer, 2) * 10;
            this.TransparencyIndex = buffer[4];
        }

        internal static GifGraphicControlExtension ReadGraphicsControl(Stream stream)
        {
            GifGraphicControlExtension extension = new GifGraphicControlExtension();
            extension.Read(stream);
            return extension;
        }

        public int BlockSize { get; private set; }

        public int DisposalMethod { get; private set; }

        public bool UserInput { get; private set; }

        public bool HasTransparency { get; private set; }

        public int Delay { get; private set; }

        public int TransparencyIndex { get; private set; }

        internal override GifBlockKind Kind =>
            GifBlockKind.Control;
    }
}

