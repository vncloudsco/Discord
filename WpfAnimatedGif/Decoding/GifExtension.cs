namespace WpfAnimatedGif.Decoding
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal abstract class GifExtension : GifBlock
    {
        internal const int ExtensionIntroducer = 0x21;

        protected GifExtension()
        {
        }

        internal static GifExtension ReadExtension(Stream stream, IEnumerable<GifExtension> controlExtensions, bool metadataOnly)
        {
            int extensionLabel = stream.ReadByte();
            if (extensionLabel < 0)
            {
                throw GifHelpers.UnexpectedEndOfStreamException();
            }
            int num2 = extensionLabel;
            if (num2 == 1)
            {
                return GifPlainTextExtension.ReadPlainText(stream, controlExtensions, metadataOnly);
            }
            if (num2 == 0xf9)
            {
                return GifGraphicControlExtension.ReadGraphicsControl(stream);
            }
            switch (num2)
            {
                case 0xfe:
                    return GifCommentExtension.ReadComment(stream);

                case 0xff:
                    return GifApplicationExtension.ReadApplication(stream);
            }
            throw GifHelpers.UnknownExtensionTypeException(extensionLabel);
        }
    }
}

