namespace Mono.Cecil
{
    using System;
    using System.IO;

    internal sealed class EmbeddedResource : Resource
    {
        private readonly MetadataReader reader;
        private uint? offset;
        private byte[] data;
        private Stream stream;

        public EmbeddedResource(string name, ManifestResourceAttributes attributes, byte[] data) : base(name, attributes)
        {
            this.data = data;
        }

        public EmbeddedResource(string name, ManifestResourceAttributes attributes, Stream stream) : base(name, attributes)
        {
            this.stream = stream;
        }

        internal EmbeddedResource(string name, ManifestResourceAttributes attributes, uint offset, MetadataReader reader) : base(name, attributes)
        {
            this.offset = new uint?(offset);
            this.reader = reader;
        }

        public byte[] GetResourceData()
        {
            if (this.stream != null)
            {
                return ReadStream(this.stream);
            }
            if (this.data != null)
            {
                return this.data;
            }
            if (this.offset == null)
            {
                throw new InvalidOperationException();
            }
            return this.reader.GetManagedResourceStream(this.offset.Value).ToArray();
        }

        public Stream GetResourceStream()
        {
            if (this.stream != null)
            {
                return this.stream;
            }
            if (this.data != null)
            {
                return new MemoryStream(this.data);
            }
            if (this.offset == null)
            {
                throw new InvalidOperationException();
            }
            return this.reader.GetManagedResourceStream(this.offset.Value);
        }

        private static byte[] ReadStream(Stream stream)
        {
            int num;
            if (!stream.CanSeek)
            {
                byte[] buffer2 = new byte[0x2000];
                MemoryStream stream2 = new MemoryStream();
                while ((num = stream.Read(buffer2, 0, buffer2.Length)) > 0)
                {
                    stream2.Write(buffer2, 0, num);
                }
                return stream2.ToArray();
            }
            int length = (int) stream.Length;
            byte[] buffer = new byte[length];
            for (int i = 0; (num = stream.Read(buffer, i, length - i)) > 0; i += num)
            {
            }
            return buffer;
        }

        public override Mono.Cecil.ResourceType ResourceType =>
            Mono.Cecil.ResourceType.Embedded;
    }
}

