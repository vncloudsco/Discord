namespace WpfAnimatedGif.Decoding
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal abstract class GifBlock
    {
        protected GifBlock()
        {
        }

        internal static GifBlock ReadBlock(Stream stream, IEnumerable<GifExtension> controlExtensions, bool metadataOnly)
        {
            int blockId = stream.ReadByte();
            if (blockId < 0)
            {
                throw GifHelpers.UnexpectedEndOfStreamException();
            }
            int num2 = blockId;
            if (num2 == 0x21)
            {
                return GifExtension.ReadExtension(stream, controlExtensions, metadataOnly);
            }
            if (num2 == 0x2c)
            {
                return GifFrame.ReadFrame(stream, controlExtensions, metadataOnly);
            }
            if (num2 != 0x3b)
            {
                throw GifHelpers.UnknownBlockTypeException(blockId);
            }
            return GifTrailer.ReadTrailer();
        }

        internal abstract GifBlockKind Kind { get; }
    }
}

