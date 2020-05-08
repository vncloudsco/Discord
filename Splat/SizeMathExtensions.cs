namespace Splat
{
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;

    internal static class SizeMathExtensions
    {
        public static SizeF ScaledBy(this SizeF This, float factor) => 
            new SizeF(This.Width * factor, This.Height * factor);

        public static bool WithinEpsilonOf(this SizeF This, SizeF other, float epsilon)
        {
            float num = other.Width - This.Width;
            float num2 = other.Height - This.Height;
            return (Math.Sqrt((double) ((num * num) + (num2 * num2))) < epsilon);
        }
    }
}

