namespace Splat
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    internal class PlatformBitmapLoader : IBitmapLoader
    {
        public IBitmap Create(float width, float height) => 
            new BitmapSourceBitmap(new WriteableBitmap((int) width, (int) height, 96.0, 96.0, PixelFormats.Default, null));

        public Task<IBitmap> Load(Stream sourceStream, float? desiredWidth, float? desiredHeight) => 
            Task.Run<IBitmap>(delegate {
                BitmapImage source = new BitmapImage();
                this.withInit(source, delegate (BitmapImage source) {
                    if (desiredWidth != null)
                    {
                        source.DecodePixelWidth = (int) desiredWidth.Value;
                        source.DecodePixelHeight = (int) desiredHeight.Value;
                    }
                    source.StreamSource = sourceStream;
                    source.CacheOption = BitmapCacheOption.OnLoad;
                });
                return new BitmapSourceBitmap(source);
            });

        public Task<IBitmap> LoadFromResource(string resource, float? desiredWidth, float? desiredHeight) => 
            Task.Run<IBitmap>(delegate {
                BitmapImage source = new BitmapImage();
                this.withInit(source, delegate (BitmapImage x) {
                    if (desiredWidth != null)
                    {
                        x.DecodePixelWidth = (int) desiredWidth.Value;
                        x.DecodePixelHeight = (int) desiredHeight.Value;
                    }
                    x.UriSource = new Uri(resource);
                });
                return new BitmapSourceBitmap(source);
            });

        private void withInit(BitmapImage source, Action<BitmapImage> block)
        {
            source.BeginInit();
            block(source);
            source.EndInit();
            source.Freeze();
        }
    }
}

