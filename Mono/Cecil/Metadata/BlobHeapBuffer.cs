namespace Mono.Cecil.Metadata
{
    using Mono.Cecil.PE;
    using System;
    using System.Collections.Generic;

    internal sealed class BlobHeapBuffer : HeapBuffer
    {
        private readonly Dictionary<ByteBuffer, uint> blobs;

        public BlobHeapBuffer() : base(1)
        {
            this.blobs = new Dictionary<ByteBuffer, uint>(new ByteBufferEqualityComparer());
            base.WriteByte(0);
        }

        public uint GetBlobIndex(ByteBuffer blob)
        {
            uint position;
            if (!this.blobs.TryGetValue(blob, out position))
            {
                position = (uint) base.position;
                this.WriteBlob(blob);
                this.blobs.Add(blob, position);
            }
            return position;
        }

        private void WriteBlob(ByteBuffer blob)
        {
            base.WriteCompressedUInt32((uint) blob.length);
            base.WriteBytes(blob);
        }

        public override bool IsEmpty =>
            (base.length <= 1);
    }
}

