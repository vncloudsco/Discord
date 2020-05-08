namespace Mono.Collections.Generic
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal sealed class ReadOnlyCollection<T> : Collection<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
    {
        private static ReadOnlyCollection<T> empty;

        private ReadOnlyCollection()
        {
        }

        public ReadOnlyCollection(Collection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException();
            }
            this.Initialize(collection.items, collection.size);
        }

        public ReadOnlyCollection(T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException();
            }
            this.Initialize(array, array.Length);
        }

        internal override void Grow(int desired)
        {
            throw new InvalidOperationException();
        }

        private void Initialize(T[] items, int size)
        {
            base.items = new T[size];
            Array.Copy(items, 0, base.items, 0, size);
            base.size = size;
        }

        protected override void OnAdd(T item, int index)
        {
            throw new InvalidOperationException();
        }

        protected override void OnClear()
        {
            throw new InvalidOperationException();
        }

        protected override void OnInsert(T item, int index)
        {
            throw new InvalidOperationException();
        }

        protected override void OnRemove(T item, int index)
        {
            throw new InvalidOperationException();
        }

        protected override void OnSet(T item, int index)
        {
            throw new InvalidOperationException();
        }

        public static ReadOnlyCollection<T> Empty =>
            (ReadOnlyCollection<T>.empty ?? (ReadOnlyCollection<T>.empty = new ReadOnlyCollection<T>()));

        bool ICollection<T>.IsReadOnly =>
            true;

        bool IList.IsFixedSize =>
            true;

        bool IList.IsReadOnly =>
            true;
    }
}

