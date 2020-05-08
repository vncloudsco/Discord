namespace DeltaCompressionDotNet
{
    using System;

    internal interface IDeltaCompression
    {
        void ApplyDelta(string deltaFilePath, string oldFilePath, string newFilePath);
        void CreateDelta(string oldFilePath, string newFilePath, string deltaFilePath);
    }
}

