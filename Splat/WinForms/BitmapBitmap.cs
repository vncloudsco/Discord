namespace Splat.WinForms
{
    using Splat;
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    internal class BitmapBitmap : IBitmap, IDisposable
    {
        internal Bitmap inner;

        public BitmapBitmap(Bitmap bitmap)
        {
            this.inner = bitmap;
            this.Width = this.inner.Width;
            this.Height = this.inner.Height;
        }

        public void Dispose()
        {
            this.inner.Dispose();
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            foreach (ImageCodecInfo info in ImageCodecInfo.GetImageEncoders())
            {
                if (info.FormatID == format.Guid)
                {
                    return info;
                }
            }
            return null;
        }

        public Task Save(CompressedBitmapFormat format, float quality, Stream target) => 
            Task.Run(delegate {
                if (format != CompressedBitmapFormat.Jpeg)
                {
                    this.inner.Save(target, ImageFormat.Png);
                }
                else
                {
                    ImageCodecInfo encoder = this.GetEncoder(ImageFormat.Jpeg);
                    EncoderParameters encoderParams = new EncoderParameters(1);
                    EncoderParameter parameter = new EncoderParameter(Encoder.Quality, (long) ((int) (quality * 100f)));
                    encoderParams.Param[0] = parameter;
                    this.inner.Save(target, encoder, encoderParams);
                }
            });

        public float Width { get; protected set; }

        public float Height { get; protected set; }
    }
}

