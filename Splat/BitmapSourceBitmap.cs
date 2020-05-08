namespace Splat
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Media.Imaging;

    internal class BitmapSourceBitmap : IBitmap, IDisposable
    {
        internal BitmapSource inner;

        public BitmapSourceBitmap(BitmapSource bitmap)
        {
            this.inner = bitmap;
            this.Width = (float) this.inner.Width;
            this.Height = (float) this.inner.Height;
        }

        public void Dispose()
        {
            this.inner = null;
        }

        public Task Save(CompressedBitmapFormat format, float quality, Stream target) => 
            Task.Run(delegate {
                JpegBitmapEncoder encoder3;
                if (format != CompressedBitmapFormat.Jpeg)
                {
                    encoder3 = (JpegBitmapEncoder) new PngBitmapEncoder();
                }
                else
                {
                    encoder3 = new JpegBitmapEncoder {
                        QualityLevel = (int) (quality * 100f)
                    };
                }
                BitmapEncoder encoder = encoder3;
                encoder.Frames.Add(BitmapFrame.Create(this.inner));
                encoder.Save(target);
            });

        public float Width { get; protected set; }

        public float Height { get; protected set; }
    }
}

