namespace WpfAnimatedGif.Decoding
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal class GifApplicationExtension : GifExtension
    {
        internal const int ExtensionLabel = 0xff;

        private GifApplicationExtension()
        {
        }

        private void Read(Stream stream)
        {
            byte[] buffer = new byte[12];
            stream.ReadAll(buffer, 0, buffer.Length);
            this.BlockSize = buffer[0];
            if (this.BlockSize != 11)
            {
                throw GifHelpers.InvalidBlockSizeException("Application Extension", 11, this.BlockSize);
            }
            this.ApplicationIdentifier = Encoding.ASCII.GetString(buffer, 1, 8);
            byte[] destinationArray = new byte[3];
            Array.Copy(buffer, 9, destinationArray, 0, 3);
            this.AuthenticationCode = destinationArray;
            this.Data = GifHelpers.ReadDataBlocks(stream, false);
        }

        internal static GifApplicationExtension ReadApplication(Stream stream)
        {
            GifApplicationExtension extension = new GifApplicationExtension();
            extension.Read(stream);
            return extension;
        }

        public int BlockSize { get; private set; }

        public string ApplicationIdentifier { get; private set; }

        public byte[] AuthenticationCode { get; private set; }

        public byte[] Data { get; private set; }

        internal override GifBlockKind Kind =>
            GifBlockKind.SpecialPurpose;
    }
}

