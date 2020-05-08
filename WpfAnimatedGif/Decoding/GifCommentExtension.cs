namespace WpfAnimatedGif.Decoding
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal class GifCommentExtension : GifExtension
    {
        internal const int ExtensionLabel = 0xfe;

        private GifCommentExtension()
        {
        }

        private void Read(Stream stream)
        {
            byte[] bytes = GifHelpers.ReadDataBlocks(stream, false);
            if (bytes != null)
            {
                this.Text = Encoding.ASCII.GetString(bytes);
            }
        }

        internal static GifCommentExtension ReadComment(Stream stream)
        {
            GifCommentExtension extension = new GifCommentExtension();
            extension.Read(stream);
            return extension;
        }

        public string Text { get; private set; }

        internal override GifBlockKind Kind =>
            GifBlockKind.SpecialPurpose;
    }
}

