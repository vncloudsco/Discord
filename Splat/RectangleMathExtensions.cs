namespace Splat
{
    using System;
    using System.Drawing;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static class RectangleMathExtensions
    {
        public static PointF Center(this RectangleF This) => 
            new PointF(This.X + (This.Width / 2f), This.Y + (This.Height / 2f));

        public static RectangleF Copy(this RectangleF rect, float? X = new float?(), float? Y = new float?(), float? Width = new float?(), float? Height = new float?(), float? Top = new float?(), float? Bottom = new float?())
        {
            RectangleF ef = new RectangleF(rect.Location, rect.Size);
            if (X != null)
            {
                ef.X = X.Value;
            }
            if (Y != null)
            {
                ef.Y = Y.Value;
            }
            if (Width != null)
            {
                ef.Width = Width.Value;
            }
            if (Height != null)
            {
                ef.Height = Height.Value;
            }
            if (Top != null)
            {
                if (Y != null)
                {
                    throw new ArgumentException("Conflicting Copy arguments Y and Top");
                }
                ef.Y = Top.Value;
            }
            if (Bottom != null)
            {
                if (Height != null)
                {
                    throw new ArgumentException("Conflicting Copy arguments Height and Bottom");
                }
                ef.Height = ef.Y + Bottom.Value;
            }
            return ef;
        }

        public static Tuple<RectangleF, RectangleF> Divide(this RectangleF This, float amount, RectEdge fromEdge)
        {
            float num = 0f;
            switch (fromEdge)
            {
                case RectEdge.Left:
                {
                    num = Math.Max(This.Width, amount);
                    float? x = null;
                    float? y = null;
                    float? height = null;
                    float? top = null;
                    float? bottom = null;
                    float? nullable6 = null;
                    float? nullable7 = null;
                    float? nullable8 = null;
                    float? nullable9 = null;
                    return Tuple.Create<RectangleF, RectangleF>(This.Copy(x, y, new float?(num), height, top, bottom), This.Copy(new float?(This.Left + num), nullable6, new float?(This.Width - num), nullable7, nullable8, nullable9));
                }
                case RectEdge.Top:
                {
                    num = Math.Max(This.Height, amount);
                    float? x = null;
                    float? y = null;
                    float? width = null;
                    float? top = null;
                    float? bottom = null;
                    float? nullable15 = null;
                    float? nullable16 = null;
                    float? nullable17 = null;
                    float? nullable18 = null;
                    return Tuple.Create<RectangleF, RectangleF>(This.Copy(x, y, width, new float?(amount), top, bottom), This.Copy(nullable15, new float?(This.Top + num), nullable16, new float?(This.Height - num), nullable17, nullable18));
                }
                case RectEdge.Right:
                {
                    num = Math.Max(This.Width, amount);
                    float? y = null;
                    float? height = null;
                    float? top = null;
                    float? bottom = null;
                    float? x = null;
                    float? nullable24 = null;
                    float? nullable25 = null;
                    float? nullable26 = null;
                    float? nullable27 = null;
                    return Tuple.Create<RectangleF, RectangleF>(This.Copy(new float?(This.Right - num), y, new float?(num), height, top, bottom), This.Copy(x, nullable24, new float?(This.Width - num), nullable25, nullable26, nullable27));
                }
                case RectEdge.Bottom:
                {
                    num = Math.Max(This.Height, amount);
                    float? x = null;
                    float? width = null;
                    float? top = null;
                    float? bottom = null;
                    float? nullable32 = null;
                    float? y = null;
                    float? nullable34 = null;
                    float? nullable35 = null;
                    float? nullable36 = null;
                    return Tuple.Create<RectangleF, RectangleF>(This.Copy(x, new float?(This.Bottom - num), width, new float?(num), top, bottom), This.Copy(nullable32, y, nullable34, new float?(This.Height - num), nullable35, nullable36));
                }
            }
            throw new ArgumentException("edge");
        }

        public static Tuple<RectangleF, RectangleF> DivideWithPadding(this RectangleF This, float sliceAmount, float padding, RectEdge fromEdge)
        {
            Tuple<RectangleF, RectangleF> tuple2 = This.Divide(padding, fromEdge);
            return Tuple.Create<RectangleF, RectangleF>(This.Divide(sliceAmount, fromEdge).Item1, tuple2.Item2);
        }

        public static RectangleF InvertWithin(this RectangleF This, RectangleF containingRect) => 
            new RectangleF(This.X, containingRect.Height - This.Bottom, This.Width, This.Height);
    }
}

