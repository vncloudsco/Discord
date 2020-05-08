namespace Squirrel.Bsdiff
{
    using System;
    using System.IO;

    internal class WrappingStream : Stream
    {
        private Stream m_streamBase;
        private readonly Ownership m_ownership;

        public WrappingStream(Stream streamBase, Ownership ownership)
        {
            if (streamBase == null)
            {
                throw new ArgumentNullException("streamBase");
            }
            this.m_streamBase = streamBase;
            this.m_ownership = ownership;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.ThrowIfDisposed();
            return this.m_streamBase.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.ThrowIfDisposed();
            return this.m_streamBase.BeginWrite(buffer, offset, count, callback, state);
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if ((this.m_streamBase != null) && (this.m_ownership == Ownership.Owns))
                    {
                        this.m_streamBase.Dispose();
                    }
                    this.m_streamBase = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            this.ThrowIfDisposed();
            return this.m_streamBase.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.ThrowIfDisposed();
            this.m_streamBase.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            this.ThrowIfDisposed();
            this.m_streamBase.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            this.ThrowIfDisposed();
            return this.m_streamBase.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            this.ThrowIfDisposed();
            return this.m_streamBase.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            this.ThrowIfDisposed();
            return this.m_streamBase.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.ThrowIfDisposed();
            this.m_streamBase.SetLength(value);
        }

        private void ThrowIfDisposed()
        {
            if (this.m_streamBase == null)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.ThrowIfDisposed();
            this.m_streamBase.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            this.ThrowIfDisposed();
            this.m_streamBase.WriteByte(value);
        }

        public override bool CanRead =>
            ((this.m_streamBase != null) && this.m_streamBase.CanRead);

        public override bool CanSeek =>
            ((this.m_streamBase != null) && this.m_streamBase.CanSeek);

        public override bool CanWrite =>
            ((this.m_streamBase != null) && this.m_streamBase.CanWrite);

        public override long Length
        {
            get
            {
                this.ThrowIfDisposed();
                return this.m_streamBase.Length;
            }
        }

        public override long Position
        {
            get
            {
                this.ThrowIfDisposed();
                return this.m_streamBase.Position;
            }
            set
            {
                this.ThrowIfDisposed();
                this.m_streamBase.Position = value;
            }
        }

        protected Stream WrappedStream =>
            this.m_streamBase;
    }
}

