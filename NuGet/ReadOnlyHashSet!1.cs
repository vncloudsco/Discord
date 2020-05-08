namespace NuGet
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal class ReadOnlyHashSet<T> : ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private readonly ISet<T> _backingSet;

        public ReadOnlyHashSet(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            this._backingSet = (ISet<T>) new HashSet<T>(items);
        }

        public ReadOnlyHashSet(ISet<T> backingSet)
        {
            if (backingSet == null)
            {
                throw new ArgumentNullException("backingSet");
            }
            this._backingSet = backingSet;
        }

        public ReadOnlyHashSet(IEnumerable<T> items, IEqualityComparer<T> comparer)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            this._backingSet = (ISet<T>) new HashSet<T>(items, comparer);
        }

        public bool Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(T item) => 
            this._backingSet.Contains(item);

        public void CopyTo(T[] array, int arrayIndex)
        {
            this._backingSet.CopyTo(array, arrayIndex);
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            this._backingSet.ExceptWith(other);
        }

        public IEnumerator<T> GetEnumerator() => 
            this._backingSet.GetEnumerator();

        public void IntersectWith(IEnumerable<T> other)
        {
            this._backingSet.IntersectWith(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other) => 
            this._backingSet.IsProperSubsetOf(other);

        public bool IsProperSupersetOf(IEnumerable<T> other) => 
            this._backingSet.IsProperSupersetOf(other);

        public bool IsSubsetOf(IEnumerable<T> other) => 
            this._backingSet.IsSubsetOf(other);

        public bool IsSupersetOf(IEnumerable<T> other) => 
            this._backingSet.IsSupersetOf(other);

        public bool Overlaps(IEnumerable<T> other) => 
            this._backingSet.Overlaps(other);

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public bool SetEquals(IEnumerable<T> other) => 
            this._backingSet.SetEquals(other);

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            this._backingSet.SymmetricExceptWith(other);
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this._backingSet.GetEnumerator();

        public void UnionWith(IEnumerable<T> other)
        {
            this._backingSet.UnionWith(other);
        }

        public int Count =>
            this._backingSet.Count;

        public bool IsReadOnly =>
            true;
    }
}

