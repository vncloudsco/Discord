namespace Splat
{
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Windows.Media;

    internal static class ColorExtensions
    {
        public static Color FromNative(this Color This) => 
            Color.FromArgb(This.A, This.R, This.G, This.B);

        public static Color ToNative(this Color This) => 
            Color.FromArgb(This.A, This.R, This.G, This.B);

        public static SolidColorBrush ToNativeBrush(this Color This)
        {
            SolidColorBrush brush = new SolidColorBrush(This.ToNative());
            brush.Freeze();
            return brush;
        }
    }
}

