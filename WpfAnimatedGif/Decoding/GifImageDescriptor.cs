namespace WpfAnimatedGif.Decoding
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal class GifImageDescriptor
    {
        private GifImageDescriptor()
        {
        }

        private void Read(Stream stream)
        {
            byte[] buffer = new byte[9];
            stream.ReadAll(buffer, 0, buffer.Length);
            this.Left = BitConverter.ToUInt16(buffer, 0);
            this.Top = BitConverter.ToUInt16(buffer, 2);
            this.Width = BitConverter.ToUInt16(buffer, 4);
            this.Height = BitConverter.ToUInt16(buffer, 6);
            byte num = buffer[8];
            this.HasLocalColorTable = (num & 0x80) != 0;
            this.Interlace = (num & 0x40) != 0;
            this.IsLocalColorTableSorted = (num & 0x20) != 0;
            this.LocalColorTableSize = 1 << (((num & 7) + 1) & 0x1f);
        }

        internal static GifImageDescriptor ReadImageDescriptor(Stream stream)
        {
            GifImageDescriptor descriptor = new GifImageDescriptor();
            descriptor.Read(stream);
            return descriptor;
        }

        public int Left { get; private set; }

        public int Top { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public bool HasLocalColorTable { get; private set; }

        public bool Interlace { get; private set; }

        public bool IsLocalColorTableSorted { get; private set; }

        public int LocalColorTableSize { get; private set; }
    }
}

