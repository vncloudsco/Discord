namespace Splat
{
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Windows;

    internal static class PointExtensions
    {
        public static PointF FromNative(this Point This) => 
            new PointF((float) This.X, (float) This.Y);

        public static SizeF FromNative(this Size This) => 
            new SizeF((float) This.Width, (float) This.Height);

        public static Point ToNative(this Point This) => 
            new Point((double) This.X, (double) This.Y);

        public static Point ToNative(this PointF This) => 
            new Point((double) This.X, (double) This.Y);

        public static Size ToNative(this Size This) => 
            new Size((double) This.Width, (double) This.Height);

        public static Size ToNative(this SizeF This) => 
            new Size((double) This.Width, (double) This.Height);
    }
}

