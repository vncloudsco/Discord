namespace Splat
{
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;

    internal static class PointMathExtensions
    {
        public static float AngleInDegrees(this PointF This) => 
            ((float) ((Math.Atan2((double) This.Y, (double) This.X) * 180.0) / 3.1415926535897931));

        public static float DistanceTo(this PointF This, PointF other)
        {
            float num = other.X - This.X;
            float num2 = other.Y - This.Y;
            return (float) Math.Sqrt((double) ((num * num) + (num2 * num2)));
        }

        public static float DotProduct(this PointF This, PointF other) => 
            ((This.X * other.X) + (This.Y * other.Y));

        public static PointF Floor(this Point This) => 
            new PointF((float) Math.Floor((double) This.X), (float) Math.Ceiling((double) This.Y));

        public static float Length(this PointF This) => 
            PointF.Empty.DistanceTo(This);

        public static PointF Normalize(this PointF This)
        {
            float num = This.Length();
            return ((num != 0f) ? new PointF(This.X / num, This.Y / num) : This);
        }

        public static PointF ProjectAlong(this PointF This, PointF direction)
        {
            PointF @this = direction.Normalize();
            return @this.ScaledBy(This.DotProduct(@this));
        }

        public static PointF ProjectAlongAngle(this PointF This, float angleInDegrees)
        {
            double d = (angleInDegrees * 3.1415926535897931) / 180.0;
            PointF direction = new PointF((float) Math.Cos(d), (float) Math.Sin(d));
            return This.ProjectAlong(direction);
        }

        public static PointF ScaledBy(this PointF This, float factor) => 
            new PointF(This.X * factor, This.Y * factor);

        public static bool WithinEpsilonOf(this PointF This, PointF other, float epsilon) => 
            (This.DistanceTo(other) < epsilon);
    }
}

