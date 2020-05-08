namespace WpfAnimatedGif.Decoding
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal class GifFrame : GifBlock
    {
        internal const int ImageSeparator = 0x2c;

        private GifFrame()
        {
        }

        private void Read(Stream stream, IEnumerable<GifExtension> controlExtensions, bool metadataOnly)
        {
            this.Descriptor = GifImageDescriptor.ReadImageDescriptor(stream);
            if (this.Descriptor.HasLocalColorTable)
            {
                this.LocalColorTable = GifHelpers.ReadColorTable(stream, this.Descriptor.LocalColorTableSize);
            }
            this.ImageData = GifImageData.ReadImageData(stream, metadataOnly);
            this.Extensions = controlExtensions.ToList<GifExtension>().AsReadOnly();
        }

        internal static GifFrame ReadFrame(Stream stream, IEnumerable<GifExtension> controlExtensions, bool metadataOnly)
        {
            GifFrame frame = new GifFrame();
            frame.Read(stream, controlExtensions, metadataOnly);
            return frame;
        }

        public GifImageDescriptor Descriptor { get; private set; }

        public GifColor[] LocalColorTable { get; private set; }

        public IList<GifExtension> Extensions { get; private set; }

        public GifImageData ImageData { get; private set; }

        internal override GifBlockKind Kind =>
            GifBlockKind.GraphicRendering;
    }
}

