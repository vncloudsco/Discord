namespace Mono.Collections.Generic
{
    using Mono;
    using Mono.Cecil;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class Collection<T> : IList<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
    {
        internal T[] items;
        internal int size;
        private int version;

        public Collection()
        {
            this.items = Empty<T>.Array;
        }

        public Collection(ICollection<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            this.items = new T[items.Count];
            items.CopyTo(this.items, 0);
            this.size = this.items.Length;
        }

        public Collection(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            this.items = new T[capacity];
        }

        public void Add(T item)
        {
            int num;
            if (this.size == this.items.Length)
            {
                this.Grow(1);
            }
            this.OnAdd(item, this.size);
            this.size = (num = this.size) + 1;
            this.items[num] = item;
            this.version++;
        }

        private void CheckIndex(int index)
        {
            if ((index < 0) || (index > this.size))
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public void Clear()
        {
            this.OnClear();
            Array.Clear(this.items, 0, this.size);
            this.size = 0;
            this.version++;
        }

        public bool Contains(T item) => 
            (this.IndexOf(item) != -1);

        public void CopyTo(T[] array, int arrayIndex)
        {
            Array.Copy(this.items, 0, array, arrayIndex, this.size);
        }

        public Enumerator<T> GetEnumerator() => 
            new Enumerator<T>((Collection<T>) this);

        internal virtual void Grow(int desired)
        {
            int num = this.size + desired;
            if (num > this.items.Length)
            {
                num = Math.Max(Math.Max(this.items.Length * 2, 4), num);
                this.Resize(num);
            }
        }

        public int IndexOf(T item) => 
            Array.IndexOf<T>(this.items, item, 0, this.size);

        public void Insert(int index, T item)
        {
            this.CheckIndex(index);
            if (this.size == this.items.Length)
            {
                this.Grow(1);
            }
            this.OnInsert(item, index);
            this.Shift(index, 1);
            this.items[index] = item;
            this.version++;
        }

        protected virtual void OnAdd(T item, int index)
        {
        }

        protected virtual void OnClear()
        {
        }

        protected virtual void OnInsert(T item, int index)
        {
        }

        protected virtual void OnRemove(T item, int index)
        {
        }

        protected virtual void OnSet(T item, int index)
        {
        }

        public bool Remove(T item)
        {
            int index = this.IndexOf(item);
            if (index == -1)
            {
                return false;
            }
            this.OnRemove(item, index);
            this.Shift(index, -1);
            Array.Clear(this.items, this.size, 1);
            this.version++;
            return true;
        }

        public void RemoveAt(int index)
        {
            if ((index < 0) || (index >= this.size))
            {
                throw new ArgumentOutOfRangeException();
            }
            T item = this.items[index];
            this.OnRemove(item, index);
            this.Shift(index, -1);
            Array.Clear(this.items, this.size, 1);
            this.version++;
        }

        protected void Resize(int new_size)
        {
            if (new_size != this.size)
            {
                if (new_size < this.size)
                {
                    throw new ArgumentOutOfRangeException();
                }
                this.items = this.items.Resize<T>(new_size);
            }
        }

        private void Shift(int start, int delta)
        {
            if (delta < 0)
            {
                start -= delta;
            }
            if (start < this.size)
            {
                Array.Copy(this.items, start, this.items, start + delta, this.size - start);
            }
            this.size += delta;
            if (delta < 0)
            {
                Array.Clear(this.items, this.size, -delta);
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => 
            new Enumerator<T>((Collection<T>) this);

        void ICollection.CopyTo(Array array, int index)
        {
            Array.Copy(this.items, 0, array, index, this.size);
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            new Enumerator<T>((Collection<T>) this);

        int IList.Add(object value)
        {
            try
            {
                this.Add((T) value);
                return (this.size - 1);
            }
            catch (InvalidCastException)
            {
            }
            catch (NullReferenceException)
            {
            }
            throw new ArgumentException();
        }

        void IList.Clear()
        {
            this.Clear();
        }

        bool IList.Contains(object value) => 
            (((IList) this).IndexOf(value) > -1);

        int IList.IndexOf(object value)
        {
            try
            {
                return this.IndexOf((T) value);
            }
            catch (InvalidCastException)
            {
            }
            catch (NullReferenceException)
            {
            }
            return -1;
        }

        void IList.Insert(int index, object value)
        {
            this.CheckIndex(index);
            try
            {
                this.Insert(index, (T) value);
            }
            catch (InvalidCastException)
            {
            }
            catch (NullReferenceException)
            {
            }
        }

        void IList.Remove(object value)
        {
            try
            {
                this.Remove((T) value);
            }
            catch (InvalidCastException)
            {
            }
            catch (NullReferenceException)
            {
            }
        }

        void IList.RemoveAt(int index)
        {
            this.RemoveAt(index);
        }

        public T[] ToArray()
        {
            T[] destinationArray = new T[this.size];
            Array.Copy(this.items, 0, destinationArray, 0, this.size);
            return destinationArray;
        }

        public int Count =>
            this.size;

        public T this[int index]
        {
            get
            {
                if (index >= this.size)
                {
                    throw new ArgumentOutOfRangeException();
                }
                return this.items[index];
            }
            set
            {
                this.CheckIndex(index);
                if (index == this.size)
                {
                    throw new ArgumentOutOfRangeException();
                }
                this.OnSet(value, index);
                this.items[index] = value;
            }
        }

        public int Capacity
        {
            get => 
                this.items.Length;
            set
            {
                if ((value < 0) || (value < this.size))
                {
                    throw new ArgumentOutOfRangeException();
                }
                this.Resize(value);
            }
        }

        bool ICollection<T>.IsReadOnly =>
            false;

        bool IList.IsFixedSize =>
            false;

        bool IList.IsReadOnly =>
            false;

        object IList.this[int index]
        {
            get => 
                this[index];
            set
            {
                this.CheckIndex(index);
                try
                {
                    this[index] = (T) value;
                }
                catch (InvalidCastException)
                {
                }
                catch (NullReferenceException)
                {
                }
            }
        }

        int ICollection.Count =>
            this.Count;

        bool ICollection.IsSynchronized =>
            false;

        object ICollection.SyncRoot =>
            this;

        [StructLayout(LayoutKind.Sequential)]
        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            private Collection<T> collection;
            private T current;
            private int next;
            private readonly int version;
            public T Current =>
                this.current;
            object IEnumerator.Current
            {
                get
                {
                    this.CheckState();
                    if (this.next <= 0)
                    {
                        throw new InvalidOperationException();
                    }
                    return this.current;
                }
            }
            internal Enumerator(Collection<T> collection)
            {
                this = (Collection<>.Enumerator) new Collection<T>.Enumerator();
                this.collection = collection;
                this.version = collection.version;
            }

            public bool MoveNext()
            {
                int num;
                this.CheckState();
                if (this.next < 0)
                {
                    return false;
                }
                if (this.next >= this.collection.size)
                {
                    this.next = -1;
                    return false;
                }
                this.next = (num = this.next) + 1;
                this.current = this.collection.items[num];
                return true;
            }

            public void Reset()
            {
                this.CheckState();
                this.next = 0;
            }

            private void CheckState()
            {
                if (this.collection == null)
                {
                    throw new ObjectDisposedException(((Collection<T>.Enumerator) this).GetType().FullName);
                }
                if (this.version != this.collection.version)
                {
                    throw new InvalidOperationException();
                }
            }

            public void Dispose()
            {
                this.collection = null;
            }
        }
    }
}

