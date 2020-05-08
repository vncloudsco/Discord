namespace WpfAnimatedGif.Decoding
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct GifColor
    {
        private readonly byte _r;
        private readonly byte _g;
        private readonly byte _b;
        internal GifColor(byte r, byte g, byte b)
        {
            this._r = r;
            this._g = g;
            this._b = b;
        }

        public byte R =>
            this._r;
        public byte G =>
            this._g;
        public byte B =>
            this._b;
        public override string ToString() => 
            $"#{this._r:x2}{this._g:x2}{this._b:x2}";
    }
}

