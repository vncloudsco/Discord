namespace WpfAnimatedGif.Decoding
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal static class GifHelpers
    {
        public static ushort GetRepeatCount(GifApplicationExtension ext) => 
            ((ext.Data.Length < 3) ? 1 : BitConverter.ToUInt16(ext.Data, 1));

        public static Exception InvalidBlockSizeException(string blockName, int expectedBlockSize, int actualBlockSize) => 
            new GifDecoderException($"Invalid block size for {blockName}. Expected {expectedBlockSize}, but was {actualBlockSize}");

        public static Exception InvalidSignatureException(string signature) => 
            new GifDecoderException("Invalid file signature: " + signature);

        public static bool IsNetscapeExtension(GifApplicationExtension ext) => 
            ((ext.ApplicationIdentifier == "NETSCAPE") && (Encoding.ASCII.GetString(ext.AuthenticationCode) == "2.0"));

        public static void ReadAll(this Stream stream, byte[] buffer, int offset, int count)
        {
            for (int i = 0; i < count; i += stream.Read(buffer, offset + i, count - i))
            {
            }
        }

        public static GifColor[] ReadColorTable(Stream stream, int size)
        {
            int count = 3 * size;
            byte[] buffer = new byte[count];
            stream.ReadAll(buffer, 0, count);
            GifColor[] colorArray = new GifColor[size];
            for (int i = 0; i < size; i++)
            {
                byte r = buffer[3 * i];
                byte g = buffer[(3 * i) + 1];
                byte b = buffer[(3 * i) + 2];
                colorArray[i] = new GifColor(r, g, b);
            }
            return colorArray;
        }

        public static byte[] ReadDataBlocks(Stream stream, bool discard)
        {
            byte[] buffer2;
            MemoryStream stream2 = discard ? null : new MemoryStream();
            using (stream2)
            {
                while (true)
                {
                    int count = stream.ReadByte();
                    if (count <= 0)
                    {
                        buffer2 = stream2?.ToArray();
                        break;
                    }
                    byte[] buffer = new byte[count];
                    stream.ReadAll(buffer, 0, count);
                    if (stream2 != null)
                    {
                        stream2.Write(buffer, 0, count);
                    }
                }
            }
            return buffer2;
        }

        public static string ReadString(Stream stream, int length)
        {
            byte[] buffer = new byte[length];
            stream.ReadAll(buffer, 0, length);
            return Encoding.ASCII.GetString(buffer);
        }

        public static Exception UnexpectedEndOfStreamException() => 
            new GifDecoderException("Unexpected end of stream before trailer was encountered");

        public static Exception UnknownBlockTypeException(int blockId) => 
            new GifDecoderException("Unknown block type: 0x" + blockId.ToString("x2"));

        public static Exception UnknownExtensionTypeException(int extensionLabel) => 
            new GifDecoderException("Unknown extension type: 0x" + extensionLabel.ToString("x2"));

        public static Exception UnsupportedVersionException(string version) => 
            new GifDecoderException("Unsupported version: " + version);
    }
}

