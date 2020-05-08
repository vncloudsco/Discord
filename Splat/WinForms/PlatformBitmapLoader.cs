namespace Splat.WinForms
{
    using Splat;
    using System;
    using System.Drawing;
    using System.IO;
    using System.Threading.Tasks;

    internal class PlatformBitmapLoader : IBitmapLoader
    {
        public IBitmap Create(float width, float height) => 
            new BitmapBitmap(new Bitmap((int) width, (int) height));

        public Task<IBitmap> Load(Stream sourceStream, float? desiredWidth, float? desiredHeight) => 
            Task.Run<IBitmap>(delegate {
                Bitmap original = new Bitmap(sourceStream);
                if (desiredWidth != null)
                {
                    original = new Bitmap(original, (int) desiredWidth.Value, (int) desiredHeight.Value);
                }
                return new BitmapBitmap(original);
            });

        public Task<IBitmap> LoadFromResource(string source, float? desiredWidth, float? desiredHeight) => 
            Task.Run<IBitmap>(delegate {
                Bitmap original = new Bitmap(source);
                if (desiredWidth != null)
                {
                    original = new Bitmap(original, (int) desiredWidth.Value, (int) desiredHeight.Value);
                }
                return new BitmapBitmap(original);
            });
    }
}

