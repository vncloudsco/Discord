namespace ICSharpCode.SharpZipLib.Zip
{
    using System;

    internal class RawTaggedData : ITaggedData
    {
        private short _tag;
        private byte[] _data;

        public RawTaggedData(short tag)
        {
            this._tag = tag;
        }

        public byte[] GetData() => 
            this._data;

        public void SetData(byte[] data, int offset, int count)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            this._data = new byte[count];
            Array.Copy(data, offset, this._data, 0, count);
        }

        public short TagID
        {
            get => 
                this._tag;
            set => 
                (this._tag = value);
        }

        public byte[] Data
        {
            get => 
                this._data;
            set => 
                (this._data = value);
        }
    }
}

