namespace Splat
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    internal interface IBitmap : IDisposable
    {
        Task Save(CompressedBitmapFormat format, float quality, Stream target);

        float Width { get; }

        float Height { get; }
    }
}

