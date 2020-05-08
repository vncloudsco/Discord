namespace WpfAnimatedGif.Decoding
{
    using System;

    internal class GifTrailer : GifBlock
    {
        internal const int TrailerByte = 0x3b;

        private GifTrailer()
        {
        }

        internal static GifTrailer ReadTrailer() => 
            new GifTrailer();

        internal override GifBlockKind Kind =>
            GifBlockKind.Other;
    }
}

