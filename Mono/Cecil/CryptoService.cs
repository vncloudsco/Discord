namespace Mono.Cecil
{
    using Mono;
    using Mono.Cecil.PE;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;

    internal static class CryptoService
    {
        public static byte[] ComputeHash(string file)
        {
            if (!File.Exists(file))
            {
                return Empty<byte>.Array;
            }
            SHA1Managed transform = new SHA1Managed();
            using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] buffer = new byte[0x2000];
                using (CryptoStream stream2 = new CryptoStream(Stream.Null, transform, CryptoStreamMode.Write))
                {
                    CopyStreamChunk(stream, stream2, buffer, (int) stream.Length);
                }
            }
            return transform.Hash;
        }

        private static void CopyStreamChunk(Stream stream, Stream dest_stream, byte[] buffer, int length)
        {
            while (length > 0)
            {
                int count = stream.Read(buffer, 0, Math.Min(buffer.Length, length));
                dest_stream.Write(buffer, 0, count);
                length -= count;
            }
        }

        private static byte[] CreateStrongName(StrongNameKeyPair key_pair, byte[] hash)
        {
            using (RSA rsa = key_pair.CreateRSA())
            {
                RSAPKCS1SignatureFormatter formatter = new RSAPKCS1SignatureFormatter(rsa);
                formatter.SetHashAlgorithm("SHA1");
                byte[] array = formatter.CreateSignature(hash);
                Array.Reverse(array);
                return array;
            }
        }

        private static byte[] HashStream(Stream stream, ImageWriter writer, out int strong_name_pointer)
        {
            Section text = writer.text;
            int headerSize = (int) writer.GetHeaderSize();
            int pointerToRawData = (int) text.PointerToRawData;
            DataDirectory strongNameSignatureDirectory = writer.GetStrongNameSignatureDirectory();
            if (strongNameSignatureDirectory.Size == 0)
            {
                throw new InvalidOperationException();
            }
            strong_name_pointer = pointerToRawData + ((int) (strongNameSignatureDirectory.VirtualAddress - text.VirtualAddress));
            int size = (int) strongNameSignatureDirectory.Size;
            SHA1Managed transform = new SHA1Managed();
            byte[] buffer = new byte[0x2000];
            using (CryptoStream stream2 = new CryptoStream(Stream.Null, transform, CryptoStreamMode.Write))
            {
                stream.Seek(0L, SeekOrigin.Begin);
                CopyStreamChunk(stream, stream2, buffer, headerSize);
                stream.Seek((long) pointerToRawData, SeekOrigin.Begin);
                CopyStreamChunk(stream, stream2, buffer, strong_name_pointer - pointerToRawData);
                stream.Seek((long) size, SeekOrigin.Current);
                CopyStreamChunk(stream, stream2, buffer, ((int) stream.Length) - (strong_name_pointer + size));
            }
            return transform.Hash;
        }

        private static void PatchStrongName(Stream stream, int strong_name_pointer, byte[] strong_name)
        {
            stream.Seek((long) strong_name_pointer, SeekOrigin.Begin);
            stream.Write(strong_name, 0, strong_name.Length);
        }

        public static void StrongName(Stream stream, ImageWriter writer, StrongNameKeyPair key_pair)
        {
            int num;
            PatchStrongName(stream, num, CreateStrongName(key_pair, HashStream(stream, writer, out num)));
        }
    }
}

