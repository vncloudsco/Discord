namespace NuGet
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal class BufferedEnumerable<TElement> : IEnumerable<TElement>, IEnumerable
    {
        private readonly IQueryable<TElement> _source;
        private readonly int _bufferSize;
        private readonly QueryState<TElement, TElement> _state;

        public BufferedEnumerable(IQueryable<TElement> source, int bufferSize)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            this._state = new QueryState<TElement, TElement>(bufferSize);
            this._source = source;
            this._bufferSize = bufferSize;
        }

        public IEnumerator<TElement> GetEnumerator() => 
            new BufferedEnumerator<TElement, TElement>(this._state, this._source, this._bufferSize);

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public override string ToString() => 
            this._source.ToString();

        internal class BufferedEnumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
        {
            private readonly int _bufferSize;
            private IQueryable<T> _source;
            private BufferedEnumerable<TElement>.QueryState<T> _state;
            private int _index;

            public BufferedEnumerator(BufferedEnumerable<TElement>.QueryState<T> state, IQueryable<T> source, int bufferSize)
            {
                this._index = -1;
                this._state = state;
                this._source = source;
                this._bufferSize = bufferSize;
            }

            public void Dispose()
            {
                this._source = null;
                this._state = null;
            }

            public bool MoveNext()
            {
                if (this.IsEmpty)
                {
                    List<T> collection = this._source.Skip<T>(this._state.Cache.Count).Take<T>(this._bufferSize).ToList<T>();
                    this._state.HasItems = this._bufferSize == collection.Count;
                    this._state.Cache.AddRange(collection);
                }
                this._index++;
                return (this._index < this._state.Cache.Count);
            }

            public void Reset()
            {
                this._index = -1;
            }

            public override string ToString() => 
                this._source.ToString();

            public T Current =>
                this._state.Cache[this._index];

            internal bool IsEmpty =>
                (this._state.HasItems && (this._index == (this._state.Cache.Count - 1)));

            object IEnumerator.Current =>
                this.Current;
        }

        internal class QueryState<T>
        {
            public QueryState(int bufferSize)
            {
                this.Cache = new List<T>(bufferSize);
                this.HasItems = true;
            }

            public List<T> Cache { get; private set; }

            public bool HasItems { get; set; }
        }
    }
}

