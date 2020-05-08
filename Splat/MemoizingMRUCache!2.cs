namespace Splat
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    internal class MemoizingMRUCache<TParam, TVal>
    {
        private readonly Func<TParam, object, TVal> calculationFunction;
        private readonly Action<TVal> releaseFunction;
        private readonly int maxCacheSize;
        private LinkedList<TParam> cacheMRUList;
        private Dictionary<TParam, Tuple<LinkedListNode<TParam>, TVal>> cacheEntries;

        public MemoizingMRUCache(Func<TParam, object, TVal> calculationFunc, int maxSize, Action<TVal> onRelease = null)
        {
            this.calculationFunction = calculationFunc;
            this.releaseFunction = onRelease;
            this.maxCacheSize = maxSize;
            this.InvalidateAll();
        }

        public IEnumerable<TVal> CachedValues() => 
            (from x in this.cacheEntries select x.Value.Item2);

        public TVal Get(TParam key) => 
            this.Get(key, null);

        public TVal Get(TParam key, object context = null)
        {
            if (this.cacheEntries.ContainsKey(key))
            {
                Tuple<LinkedListNode<TParam>, TVal> tuple = this.cacheEntries[key];
                this.cacheMRUList.Remove(tuple.Item1);
                this.cacheMRUList.AddFirst(tuple.Item1);
                return tuple.Item2;
            }
            TVal local = this.calculationFunction(key, context);
            LinkedListNode<TParam> node = new LinkedListNode<TParam>(key);
            this.cacheMRUList.AddFirst(node);
            this.cacheEntries[key] = new Tuple<LinkedListNode<TParam>, TVal>(node, local);
            this.maintainCache();
            return local;
        }

        public void Invalidate(TParam key)
        {
            if (this.cacheEntries.ContainsKey(key))
            {
                Tuple<LinkedListNode<TParam>, TVal> tuple = this.cacheEntries[key];
                if (this.releaseFunction != null)
                {
                    this.releaseFunction(tuple.Item2);
                }
                this.cacheMRUList.Remove(tuple.Item1);
                this.cacheEntries.Remove(key);
            }
        }

        public void InvalidateAll()
        {
            if ((this.releaseFunction == null) || (this.cacheEntries == null))
            {
                this.cacheMRUList = new LinkedList<TParam>();
                this.cacheEntries = new Dictionary<TParam, Tuple<LinkedListNode<TParam>, TVal>>();
            }
            else if (this.cacheEntries.Count != 0)
            {
                foreach (TParam local in this.cacheEntries.Keys.ToArray<TParam>())
                {
                    this.Invalidate(local);
                }
            }
        }

        private void Invariants()
        {
        }

        private void maintainCache()
        {
            while (this.cacheMRUList.Count > this.maxCacheSize)
            {
                TParam local = this.cacheMRUList.Last.Value;
                if (this.releaseFunction != null)
                {
                    this.releaseFunction(this.cacheEntries[local].Item2);
                }
                this.cacheEntries.Remove(this.cacheMRUList.Last.Value);
                this.cacheMRUList.RemoveLast();
            }
        }

        public bool TryGet(TParam key, out TVal result)
        {
            Tuple<LinkedListNode<TParam>, TVal> tuple;
            bool flag = this.cacheEntries.TryGetValue(key, out tuple);
            if (!flag || (tuple == null))
            {
                result = default(TVal);
            }
            else
            {
                this.cacheMRUList.Remove(tuple.Item1);
                this.cacheMRUList.AddFirst(tuple.Item1);
                result = tuple.Item2;
            }
            return flag;
        }
    }
}

