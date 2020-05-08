namespace WpfAnimatedGif.Decoding
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    internal class GifDecoderException : Exception
    {
        internal GifDecoderException()
        {
        }

        internal GifDecoderException(string message) : base(message)
        {
        }

        protected GifDecoderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        internal GifDecoderException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}

