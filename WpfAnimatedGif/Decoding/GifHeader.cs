namespace WpfAnimatedGif.Decoding
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal class GifHeader : GifBlock
    {
        private GifHeader()
        {
        }

        private void Read(Stream stream)
        {
            this.Signature = GifHelpers.ReadString(stream, 3);
            if (this.Signature != "GIF")
            {
                throw GifHelpers.InvalidSignatureException(this.Signature);
            }
            this.Version = GifHelpers.ReadString(stream, 3);
            if ((this.Version != "87a") && (this.Version != "89a"))
            {
                throw GifHelpers.UnsupportedVersionException(this.Version);
            }
            this.LogicalScreenDescriptor = GifLogicalScreenDescriptor.ReadLogicalScreenDescriptor(stream);
        }

        internal static GifHeader ReadHeader(Stream stream)
        {
            GifHeader header = new GifHeader();
            header.Read(stream);
            return header;
        }

        public string Signature { get; private set; }

        public string Version { get; private set; }

        public GifLogicalScreenDescriptor LogicalScreenDescriptor { get; private set; }

        internal override GifBlockKind Kind =>
            GifBlockKind.Other;
    }
}

