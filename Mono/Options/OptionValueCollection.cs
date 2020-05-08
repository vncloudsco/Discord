namespace Mono.Options
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    public class OptionValueCollection : IList, ICollection, IEnumerable, IList<string>, ICollection<string>, IEnumerable<string>
    {
        private List<string> values = new List<string>();
        private OptionContext c;

        internal OptionValueCollection(OptionContext c)
        {
            this.c = c;
        }

        public void Add(string item)
        {
            this.values.Add(item);
        }

        private void AssertValid(int index)
        {
            if (this.c.Option == null)
            {
                throw new InvalidOperationException("OptionContext.Option is null.");
            }
            if (index >= this.c.Option.MaxValueCount)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            if ((this.c.Option.OptionValueType == OptionValueType.Required) && (index >= this.values.Count))
            {
                throw new OptionException(string.Format(this.c.OptionSet.MessageLocalizer("Missing required value for option '{0}'."), this.c.OptionName), this.c.OptionName);
            }
        }

        public void Clear()
        {
            this.values.Clear();
        }

        public bool Contains(string item) => 
            this.values.Contains(item);

        public void CopyTo(string[] array, int arrayIndex)
        {
            this.values.CopyTo(array, arrayIndex);
        }

        public IEnumerator<string> GetEnumerator() => 
            this.values.GetEnumerator();

        public int IndexOf(string item) => 
            this.values.IndexOf(item);

        public void Insert(int index, string item)
        {
            this.values.Insert(index, item);
        }

        public bool Remove(string item) => 
            this.values.Remove(item);

        public void RemoveAt(int index)
        {
            this.values.RemoveAt(index);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.values.CopyTo(array, index);
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this.values.GetEnumerator();

        int IList.Add(object value) => 
            this.values.Add(value);

        bool IList.Contains(object value) => 
            this.values.Contains(value);

        int IList.IndexOf(object value) => 
            this.values.IndexOf(value);

        void IList.Insert(int index, object value)
        {
            this.values.Insert(index, value);
        }

        void IList.Remove(object value)
        {
            this.values.Remove(value);
        }

        void IList.RemoveAt(int index)
        {
            this.values.RemoveAt(index);
        }

        public string[] ToArray() => 
            this.values.ToArray();

        public List<string> ToList() => 
            new List<string>(this.values);

        public override string ToString() => 
            string.Join(", ", this.values.ToArray());

        bool ICollection.IsSynchronized =>
            ((ICollection) this.values).IsSynchronized;

        object ICollection.SyncRoot =>
            ((ICollection) this.values).SyncRoot;

        public int Count =>
            this.values.Count;

        public bool IsReadOnly =>
            false;

        bool IList.IsFixedSize =>
            false;

        object IList.this[int index]
        {
            get => 
                this[index];
            set => 
                (((IList) this.values)[index] = value);
        }

        public string this[int index]
        {
            get
            {
                this.AssertValid(index);
                return ((index >= this.values.Count) ? null : this.values[index]);
            }
            set => 
                (this.values[index] = value);
        }
    }
}

