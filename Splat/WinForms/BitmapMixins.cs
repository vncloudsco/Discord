namespace Splat.WinForms
{
    using Splat;
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;

    internal static class BitmapMixins
    {
        public static IBitmap FromNative(this Bitmap This) => 
            new BitmapBitmap(This);

        public static Bitmap ToNative(this IBitmap This) => 
            ((BitmapBitmap) This).inner;
    }
}

