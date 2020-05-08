namespace Splat
{
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Windows;

    internal static class RectExtensions
    {
        public static RectangleF FromNative(this Rect This) => 
            new RectangleF((float) This.X, (float) This.Y, (float) This.Width, (float) This.Height);

        public static Rect ToNative(this Rectangle This) => 
            new Rect((double) This.X, (double) This.Y, (double) This.Width, (double) This.Height);

        public static Rect ToNative(this RectangleF This) => 
            new Rect((double) This.X, (double) This.Y, (double) This.Width, (double) This.Height);
    }
}

