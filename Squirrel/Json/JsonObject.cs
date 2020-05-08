namespace Squirrel.Json
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Dynamic;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [GeneratedCode("simple-json", "1.0.0"), EditorBrowsable(EditorBrowsableState.Never)]
    internal class JsonObject : DynamicObject, IDictionary<string, object>, ICollection<KeyValuePair<string, object>>, IEnumerable<KeyValuePair<string, object>>, IEnumerable
    {
        private readonly Dictionary<string, object> _members;

        public JsonObject()
        {
            this._members = new Dictionary<string, object>();
        }

        public JsonObject(IEqualityComparer<string> comparer)
        {
            this._members = new Dictionary<string, object>(comparer);
        }

        public void Add(KeyValuePair<string, object> item)
        {
            this._members.Add(item.Key, item.Value);
        }

        public void Add(string key, object value)
        {
            this._members.Add(key, value);
        }

        public void Clear()
        {
            this._members.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item) => 
            (this._members.ContainsKey(item.Key) && (this._members[item.Key] == item.Value));

        public bool ContainsKey(string key) => 
            this._members.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            int count = this.Count;
            foreach (KeyValuePair<string, object> pair in this)
            {
                array[arrayIndex++] = pair;
                if (--count <= 0)
                {
                    break;
                }
            }
        }

        internal static object GetAtIndex(IDictionary<string, object> obj, int index)
        {
            object obj2;
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            if (index >= obj.Count)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            int num = 0;
            using (IEnumerator<KeyValuePair<string, object>> enumerator = obj.GetEnumerator())
            {
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        KeyValuePair<string, object> current = enumerator.Current;
                        if (num++ != index)
                        {
                            continue;
                        }
                        obj2 = current.Value;
                    }
                    else
                    {
                        return null;
                    }
                    break;
                }
            }
            return obj2;
        }

        [IteratorStateMachine(typeof(<GetDynamicMemberNames>d__35))]
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            <GetDynamicMemberNames>d__35 d__1 = new <GetDynamicMemberNames>d__35(-2);
            d__1.<>4__this = this;
            return d__1;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => 
            this._members.GetEnumerator();

        public bool Remove(KeyValuePair<string, object> item) => 
            this._members.Remove(item.Key);

        public bool Remove(string key) => 
            this._members.Remove(key);

        IEnumerator IEnumerable.GetEnumerator() => 
            this._members.GetEnumerator();

        public override string ToString() => 
            SimpleJson.SerializeObject(this);

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }
            Type type = binder.get_Type();
            if ((type != typeof(IEnumerable)) && ((type != typeof(IEnumerable<KeyValuePair<string, object>>)) && ((type != typeof(IDictionary<string, object>)) && (type != typeof(IDictionary)))))
            {
                return base.TryConvert(binder, ref result);
            }
            result = this;
            return true;
        }

        public override bool TryDeleteMember(DeleteMemberBinder binder)
        {
            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }
            return this._members.Remove(binder.get_Name());
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            if (indexes == null)
            {
                throw new ArgumentNullException("indexes");
            }
            if (indexes.Length == 1)
            {
                result = this[(string) indexes[0]];
                return true;
            }
            result = null;
            return true;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            object obj2;
            if (this._members.TryGetValue(binder.get_Name(), out obj2))
            {
                result = obj2;
                return true;
            }
            result = null;
            return true;
        }

        public bool TryGetValue(string key, out object value) => 
            this._members.TryGetValue(key, out value);

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes == null)
            {
                throw new ArgumentNullException("indexes");
            }
            if (indexes.Length != 1)
            {
                return base.TrySetIndex(binder, indexes, value);
            }
            this[(string) indexes[0]] = value;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (binder == null)
            {
                throw new ArgumentNullException("binder");
            }
            this._members[binder.get_Name()] = value;
            return true;
        }

        public object this[int index] =>
            GetAtIndex(this._members, index);

        public ICollection<string> Keys =>
            this._members.Keys;

        public ICollection<object> Values =>
            this._members.Values;

        public object this[string key]
        {
            get => 
                this._members[key];
            set => 
                (this._members[key] = value);
        }

        public int Count =>
            this._members.Count;

        public bool IsReadOnly =>
            false;

        [CompilerGenerated]
        private sealed class <GetDynamicMemberNames>d__35 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private string <>2__current;
            private int <>l__initialThreadId;
            public JsonObject <>4__this;
            private IEnumerator<string> <>7__wrap1;

            [DebuggerHidden]
            public <GetDynamicMemberNames>d__35(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap1 != null)
                {
                    this.<>7__wrap1.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    int num = this.<>1__state;
                    if (num == 0)
                    {
                        this.<>1__state = -1;
                        this.<>7__wrap1 = this.<>4__this.Keys.GetEnumerator();
                        this.<>1__state = -3;
                    }
                    else if (num == 1)
                    {
                        this.<>1__state = -3;
                    }
                    else
                    {
                        return false;
                    }
                    if (!this.<>7__wrap1.MoveNext())
                    {
                        this.<>m__Finally1();
                        this.<>7__wrap1 = null;
                        flag = false;
                    }
                    else
                    {
                        string current = this.<>7__wrap1.Current;
                        this.<>2__current = current;
                        this.<>1__state = 1;
                        flag = true;
                    }
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<string> IEnumerable<string>.GetEnumerator()
            {
                JsonObject.<GetDynamicMemberNames>d__35 d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new JsonObject.<GetDynamicMemberNames>d__35(0) {
                        <>4__this = this.<>4__this
                    };
                }
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<System.String>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                int num = this.<>1__state;
                if ((num == -3) || (num == 1))
                {
                    try
                    {
                    }
                    finally
                    {
                        this.<>m__Finally1();
                    }
                }
            }

            string IEnumerator<string>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

