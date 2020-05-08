namespace Splat
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    internal interface IBitmapLoader
    {
        IBitmap Create(float width, float height);
        Task<IBitmap> Load(Stream sourceStream, float? desiredWidth, float? desiredHeight);
        Task<IBitmap> LoadFromResource(string source, float? desiredWidth, float? desiredHeight);
    }
}

