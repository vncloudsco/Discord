namespace WpfAnimatedGif.Decoding
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal class GifPlainTextExtension : GifExtension
    {
        internal const int ExtensionLabel = 1;

        private GifPlainTextExtension()
        {
        }

        private void Read(Stream stream, IEnumerable<GifExtension> controlExtensions, bool metadataOnly)
        {
            byte[] buffer = new byte[13];
            stream.ReadAll(buffer, 0, buffer.Length);
            this.BlockSize = buffer[0];
            if (this.BlockSize != 12)
            {
                throw GifHelpers.InvalidBlockSizeException("Plain Text Extension", 12, this.BlockSize);
            }
            this.Left = BitConverter.ToUInt16(buffer, 1);
            this.Top = BitConverter.ToUInt16(buffer, 3);
            this.Width = BitConverter.ToUInt16(buffer, 5);
            this.Height = BitConverter.ToUInt16(buffer, 7);
            this.CellWidth = buffer[9];
            this.CellHeight = buffer[10];
            this.ForegroundColorIndex = buffer[11];
            this.BackgroundColorIndex = buffer[12];
            byte[] bytes = GifHelpers.ReadDataBlocks(stream, metadataOnly);
            this.Text = Encoding.ASCII.GetString(bytes);
            this.Extensions = controlExtensions.ToList<GifExtension>().AsReadOnly();
        }

        internal static GifPlainTextExtension ReadPlainText(Stream stream, IEnumerable<GifExtension> controlExtensions, bool metadataOnly)
        {
            GifPlainTextExtension extension = new GifPlainTextExtension();
            extension.Read(stream, controlExtensions, metadataOnly);
            return extension;
        }

        public int BlockSize { get; private set; }

        public int Left { get; private set; }

        public int Top { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int CellWidth { get; private set; }

        public int CellHeight { get; private set; }

        public int ForegroundColorIndex { get; private set; }

        public int BackgroundColorIndex { get; private set; }

        public string Text { get; private set; }

        public IList<GifExtension> Extensions { get; private set; }

        internal override GifBlockKind Kind =>
            GifBlockKind.GraphicRendering;
    }
}

