namespace Splat
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Windows.Media.Imaging;

    internal static class BitmapMixins
    {
        public static IBitmap FromNative(this BitmapSource This) => 
            new BitmapSourceBitmap(This);

        public static BitmapSource ToNative(this IBitmap This) => 
            ((BitmapSourceBitmap) This).inner;
    }
}

