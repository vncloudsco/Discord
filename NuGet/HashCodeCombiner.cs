namespace NuGet
{
    using System;

    internal class HashCodeCombiner
    {
        private long _combinedHash64 = 0x1505L;

        public void AddInt32(int i)
        {
            this._combinedHash64 = ((this._combinedHash64 << 5) + this._combinedHash64) ^ i;
        }

        public void AddObject(object o)
        {
            int i = (o != null) ? o.GetHashCode() : 0;
            this.AddInt32(i);
        }

        public int CombinedHash =>
            this._combinedHash64.GetHashCode();
    }
}

