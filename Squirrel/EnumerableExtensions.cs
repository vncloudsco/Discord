namespace Squirrel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal static class EnumerableExtensions
    {
        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            return source.Distinct_<TSource, TKey>(keySelector, EqualityComparer<TKey>.Default);
        }

        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            return source.Distinct_<TSource, TKey>(keySelector, comparer);
        }

        [IteratorStateMachine(typeof(<Distinct_>d__14))]
        private static IEnumerable<TSource> Distinct_<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            <Distinct_>d__14<TSource, TKey> d__1 = new <Distinct_>d__14<TSource, TKey>(-2);
            d__1.<>3__source = source;
            d__1.<>3__keySelector = keySelector;
            d__1.<>3__comparer = comparer;
            return d__1;
        }

        public static IEnumerable<TSource> Do<TSource>(this IEnumerable<TSource> source, Action<TSource> onNext)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (onNext == null)
            {
                throw new ArgumentNullException("onNext");
            }
            return source.DoHelper<TSource>(onNext, delegate (Exception _) {
            }, delegate {
            });
        }

        public static IEnumerable<TSource> Do<TSource>(this IEnumerable<TSource> source, Action<TSource> onNext, Action onCompleted)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (onNext == null)
            {
                throw new ArgumentNullException("onNext");
            }
            if (onCompleted == null)
            {
                throw new ArgumentNullException("onCompleted");
            }
            return source.DoHelper<TSource>(onNext, delegate (Exception _) {
            }, onCompleted);
        }

        public static IEnumerable<TSource> Do<TSource>(this IEnumerable<TSource> source, Action<TSource> onNext, Action<Exception> onError)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (onNext == null)
            {
                throw new ArgumentNullException("onNext");
            }
            if (onError == null)
            {
                throw new ArgumentNullException("onError");
            }
            return source.DoHelper<TSource>(onNext, onError, delegate {
            });
        }

        public static IEnumerable<TSource> Do<TSource>(this IEnumerable<TSource> source, Action<TSource> onNext, Action<Exception> onError, Action onCompleted)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (onNext == null)
            {
                throw new ArgumentNullException("onNext");
            }
            if (onError == null)
            {
                throw new ArgumentNullException("onError");
            }
            if (onCompleted == null)
            {
                throw new ArgumentNullException("onCompleted");
            }
            return source.DoHelper<TSource>(onNext, onError, onCompleted);
        }

        [IteratorStateMachine(typeof(<DoHelper>d__9))]
        private static IEnumerable<TSource> DoHelper<TSource>(this IEnumerable<TSource> source, Action<TSource> onNext, Action<Exception> onError, Action onCompleted)
        {
            <DoHelper>d__9<TSource> d__1 = new <DoHelper>d__9<TSource>(-2);
            d__1.<>3__source = source;
            d__1.<>3__onNext = onNext;
            d__1.<>3__onError = onError;
            d__1.<>3__onCompleted = onCompleted;
            return d__1;
        }

        private static IList<TSource> ExtremaBy<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TKey, TKey, int> compare)
        {
            List<TSource> list = new List<TSource>();
            using (IEnumerator<TSource> enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    throw new InvalidOperationException("Source sequence doesn't contain any elements.");
                }
                TSource current = enumerator.Current;
                TKey local2 = keySelector(current);
                list.Add(current);
                while (enumerator.MoveNext())
                {
                    TSource arg = enumerator.Current;
                    TKey local4 = keySelector(arg);
                    int num = compare(local4, local2);
                    if (num == 0)
                    {
                        list.Add(arg);
                        continue;
                    }
                    if (num > 0)
                    {
                        List<TSource> list1 = new List<TSource>();
                        list1.Add(arg);
                        list = list1;
                        local2 = local4;
                    }
                }
            }
            return list;
        }

        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> onNext)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (onNext == null)
            {
                throw new ArgumentNullException("onNext");
            }
            foreach (TSource local in source)
            {
                onNext(local);
            }
        }

        public static IList<TSource> MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            return source.MaxBy<TSource, TKey>(keySelector, Comparer<TKey>.Default);
        }

        public static IList<TSource> MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IComparer<TKey> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (keySelector == null)
            {
                throw new ArgumentNullException("keySelector");
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }
            return ExtremaBy<TSource, TKey>(source, keySelector, (key, minValue) => comparer.Compare(key, minValue));
        }

        [IteratorStateMachine(typeof(<Return>d__0))]
        public static IEnumerable<T> Return<T>(T value)
        {
            <Return>d__0<T> d__1 = new <Return>d__0<T>(-2);
            d__1.<>3__value = value;
            return d__1;
        }

        public static IEnumerable<TSource> StartWith<TSource>(this IEnumerable<TSource> source, params TSource[] values)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            return source.StartWith_<TSource>(values);
        }

        [IteratorStateMachine(typeof(<StartWith_>d__11))]
        private static IEnumerable<TSource> StartWith_<TSource>(this IEnumerable<TSource> source, params TSource[] values)
        {
            IEnumerator<TSource> enumerator;
            TSource[] <>7__wrap1 = values;
            int index = 0;
        Label_PostSwitchInIterator:;
            if (index < <>7__wrap1.Length)
            {
                TSource local = <>7__wrap1[index];
                yield return local;
                index++;
                goto Label_PostSwitchInIterator;
            }
            else
            {
                <>7__wrap1 = null;
                enumerator = source.GetEnumerator();
            }
            if (!enumerator.MoveNext())
            {
                enumerator = null;
                yield break;
            }
            else
            {
                TSource current = enumerator.Current;
                yield return current;
                yield break;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c__5<TSource>
        {
            public static readonly EnumerableExtensions.<>c__5<TSource> <>9;
            public static Action<Exception> <>9__5_0;
            public static Action <>9__5_1;

            static <>c__5()
            {
                EnumerableExtensions.<>c__5<TSource>.<>9 = new EnumerableExtensions.<>c__5<TSource>();
            }

            internal void <Do>b__5_0(Exception _)
            {
            }

            internal void <Do>b__5_1()
            {
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c__6<TSource>
        {
            public static readonly EnumerableExtensions.<>c__6<TSource> <>9;
            public static Action<Exception> <>9__6_0;

            static <>c__6()
            {
                EnumerableExtensions.<>c__6<TSource>.<>9 = new EnumerableExtensions.<>c__6<TSource>();
            }

            internal void <Do>b__6_0(Exception _)
            {
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c__7<TSource>
        {
            public static readonly EnumerableExtensions.<>c__7<TSource> <>9;
            public static Action <>9__7_0;

            static <>c__7()
            {
                EnumerableExtensions.<>c__7<TSource>.<>9 = new EnumerableExtensions.<>c__7<TSource>();
            }

            internal void <Do>b__7_0()
            {
            }
        }

        [CompilerGenerated]
        private sealed class <Distinct_>d__14<TSource, TKey> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private TSource <>2__current;
            private int <>l__initialThreadId;
            private IEqualityComparer<TKey> comparer;
            public IEqualityComparer<TKey> <>3__comparer;
            private IEnumerable<TSource> source;
            public IEnumerable<TSource> <>3__source;
            private Func<TSource, TKey> keySelector;
            public Func<TSource, TKey> <>3__keySelector;
            private HashSet<TKey> <set>5__1;
            private IEnumerator<TSource> <>7__wrap1;

            [DebuggerHidden]
            public <Distinct_>d__14(int <>1__state)
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
                        this.<set>5__1 = new HashSet<TKey>(this.comparer);
                        this.<>7__wrap1 = this.source.GetEnumerator();
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
                    while (true)
                    {
                        if (!this.<>7__wrap1.MoveNext())
                        {
                            this.<>m__Finally1();
                            this.<>7__wrap1 = null;
                            flag = false;
                        }
                        else
                        {
                            TSource current = this.<>7__wrap1.Current;
                            TKey item = this.keySelector(current);
                            if (!this.<set>5__1.Add(item))
                            {
                                continue;
                            }
                            this.<>2__current = current;
                            this.<>1__state = 1;
                            flag = true;
                        }
                        break;
                    }
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
            {
                EnumerableExtensions.<Distinct_>d__14<TSource, TKey> d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new EnumerableExtensions.<Distinct_>d__14<TSource, TKey>(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = (EnumerableExtensions.<Distinct_>d__14<TSource, TKey>) this;
                }
                d__.source = this.<>3__source;
                d__.keySelector = this.<>3__keySelector;
                d__.comparer = this.<>3__comparer;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<TSource>.GetEnumerator();

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

            TSource IEnumerator<TSource>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }

        [CompilerGenerated]
        private sealed class <DoHelper>d__9<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private TSource <>2__current;
            private int <>l__initialThreadId;
            private IEnumerable<TSource> source;
            public IEnumerable<TSource> <>3__source;
            private IEnumerator<TSource> <e>5__1;
            private Action<Exception> onError;
            public Action<Exception> <>3__onError;
            private Action<TSource> onNext;
            public Action<TSource> <>3__onNext;
            private Action onCompleted;
            public Action <>3__onCompleted;

            [DebuggerHidden]
            public <DoHelper>d__9(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<e>5__1 != null)
                {
                    this.<e>5__1.Dispose();
                }
            }

            private bool MoveNext()
            {
                try
                {
                    int num = this.<>1__state;
                    if (num == 0)
                    {
                        this.<>1__state = -1;
                        this.<e>5__1 = this.source.GetEnumerator();
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
                    TSource current = default(TSource);
                    try
                    {
                        if (this.<e>5__1.MoveNext())
                        {
                            current = this.<e>5__1.Current;
                        }
                        else
                        {
                            goto TR_0004;
                        }
                    }
                    catch (Exception exception)
                    {
                        this.onError(exception);
                        throw;
                    }
                    this.onNext(current);
                    this.<>2__current = current;
                    this.<>1__state = 1;
                    return true;
                TR_0004:
                    this.onCompleted();
                    this.<>m__Finally1();
                    this.<e>5__1 = null;
                    return false;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
            }

            [DebuggerHidden]
            IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
            {
                EnumerableExtensions.<DoHelper>d__9<TSource> d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new EnumerableExtensions.<DoHelper>d__9<TSource>(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = (EnumerableExtensions.<DoHelper>d__9<TSource>) this;
                }
                d__.source = this.<>3__source;
                d__.onNext = this.<>3__onNext;
                d__.onError = this.<>3__onError;
                d__.onCompleted = this.<>3__onCompleted;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<TSource>.GetEnumerator();

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

            TSource IEnumerator<TSource>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }

        [CompilerGenerated]
        private sealed class <Return>d__0<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private T <>2__current;
            private int <>l__initialThreadId;
            private T value;
            public T <>3__value;

            [DebuggerHidden]
            public <Return>d__0(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private bool MoveNext()
            {
                int num = this.<>1__state;
                if (num == 0)
                {
                    this.<>1__state = -1;
                    this.<>2__current = this.value;
                    this.<>1__state = 1;
                    return true;
                }
                if (num == 1)
                {
                    this.<>1__state = -1;
                }
                return false;
            }

            [DebuggerHidden]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                EnumerableExtensions.<Return>d__0<T> d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new EnumerableExtensions.<Return>d__0<T>(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = (EnumerableExtensions.<Return>d__0<T>) this;
                }
                d__.value = this.<>3__value;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<T>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
            }

            T IEnumerator<T>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }

        [CompilerGenerated]
        private sealed class <StartWith_>d__11<TSource> : IEnumerable<TSource>, IEnumerable, IEnumerator<TSource>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private TSource <>2__current;
            private int <>l__initialThreadId;
            private TSource[] values;
            public TSource[] <>3__values;
            private IEnumerable<TSource> source;
            public IEnumerable<TSource> <>3__source;
            private TSource[] <>7__wrap1;
            private int <>7__wrap2;
            private IEnumerator<TSource> <>7__wrap3;

            [DebuggerHidden]
            public <StartWith_>d__11(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap3 != null)
                {
                    this.<>7__wrap3.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            this.<>7__wrap1 = this.values;
                            this.<>7__wrap2 = 0;
                            break;

                        case 1:
                            this.<>1__state = -1;
                            this.<>7__wrap2++;
                            break;

                        case 2:
                            this.<>1__state = -3;
                            goto TR_0005;

                        default:
                            return false;
                    }
                    if (this.<>7__wrap2 < this.<>7__wrap1.Length)
                    {
                        TSource local = this.<>7__wrap1[this.<>7__wrap2];
                        this.<>2__current = local;
                        this.<>1__state = 1;
                        return true;
                    }
                    else
                    {
                        this.<>7__wrap1 = null;
                        this.<>7__wrap3 = this.source.GetEnumerator();
                        this.<>1__state = -3;
                    }
                TR_0005:
                    if (!this.<>7__wrap3.MoveNext())
                    {
                        this.<>m__Finally1();
                        this.<>7__wrap3 = null;
                        flag = false;
                    }
                    else
                    {
                        TSource current = this.<>7__wrap3.Current;
                        this.<>2__current = current;
                        this.<>1__state = 2;
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
            IEnumerator<TSource> IEnumerable<TSource>.GetEnumerator()
            {
                EnumerableExtensions.<StartWith_>d__11<TSource> d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new EnumerableExtensions.<StartWith_>d__11<TSource>(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = (EnumerableExtensions.<StartWith_>d__11<TSource>) this;
                }
                d__.source = this.<>3__source;
                d__.values = this.<>3__values;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<TSource>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                int num = this.<>1__state;
                if ((num == -3) || (num == 2))
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

            TSource IEnumerator<TSource>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

