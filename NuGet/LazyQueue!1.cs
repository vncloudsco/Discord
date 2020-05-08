namespace NuGet
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal class LazyQueue<TVal> : IEnumerable<TVal>, IEnumerable
    {
        private readonly IEnumerator<TVal> _enumerator;
        private TVal _peeked;

        public LazyQueue(IEnumerator<TVal> enumerator)
        {
            this._enumerator = enumerator;
        }

        public void Dequeue()
        {
            this._peeked = default(TVal);
        }

        public IEnumerator<TVal> GetEnumerator() => 
            this._enumerator;

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public bool TryPeek(out TVal element)
        {
            element = default(TVal);
            if (this._peeked != null)
            {
                element = this._peeked;
                return true;
            }
            bool flag1 = this._enumerator.MoveNext();
            if (flag1)
            {
                element = this._enumerator.Current;
                this._peeked = element;
            }
            return flag1;
        }
    }
}

