namespace NuGet
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal static class EnumerableExtensions
    {
        [IteratorStateMachine(typeof(<DistinctLast>d__0))]
        internal static IEnumerable<TElement> DistinctLast<TElement>(this IEnumerable<TElement> source, IEqualityComparer<TElement> equalityComparer, IComparer<TElement> comparer)
        {
            bool flag2 = true;
            bool flag3 = false;
            TElement y = default(TElement);
            TElement x = default(TElement);
            IEnumerator<TElement> enumerator = source.GetEnumerator();
            while (true)
            {
                TElement current;
                if (enumerator.MoveNext())
                {
                    current = enumerator.Current;
                    if (!flag2 && !equalityComparer.Equals(current, y))
                    {
                        yield return x;
                        yield break;
                        break;
                    }
                }
                else
                {
                    enumerator = null;
                    if (!flag2)
                    {
                        yield return x;
                        yield break;
                        break;
                    }
                }
                if (!flag3 || (flag3 && (comparer.Compare(x, current) < 0)))
                {
                    x = current;
                    flag3 = true;
                }
                y = current;
                flag2 = false;
                current = default(TElement);
            }
        }

        public static bool IsEmpty<T>(this IEnumerable<T> sequence) => 
            ((sequence == null) || !sequence.Any<T>());

        internal static IEnumerable<TElement> SafeIterate<TElement>(this IEnumerable<TElement> source)
        {
            List<TElement> list = new List<TElement>();
            using (IEnumerator<TElement> enumerator = source.GetEnumerator())
            {
                bool flag = true;
                while (flag)
                {
                    try
                    {
                        if (enumerator.MoveNext())
                        {
                            list.Add(enumerator.Current);
                            continue;
                        }
                    }
                    catch
                    {
                    }
                    break;
                }
            }
            return list;
        }

        [CompilerGenerated]
        private sealed class <DistinctLast>d__0<TElement> : IEnumerable<TElement>, IEnumerable, IEnumerator<TElement>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private TElement <>2__current;
            private int <>l__initialThreadId;
            private IEnumerable<TElement> source;
            public IEnumerable<TElement> <>3__source;
            private IEqualityComparer<TElement> equalityComparer;
            public IEqualityComparer<TElement> <>3__equalityComparer;
            private TElement <maxElement>5__1;
            private IComparer<TElement> comparer;
            public IComparer<TElement> <>3__comparer;
            private TElement <element>5__2;
            private IEnumerator<TElement> <>7__wrap1;

            [DebuggerHidden]
            public <DistinctLast>d__0(int <>1__state)
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
                    bool flag2;
                    bool flag3;
                    TElement local;
                    switch (this.<>1__state)
                    {
                        case 0:
                            this.<>1__state = -1;
                            flag2 = true;
                            flag3 = false;
                            local = default(TElement);
                            this.<maxElement>5__1 = default(TElement);
                            this.<>7__wrap1 = this.source.GetEnumerator();
                            this.<>1__state = -3;
                            break;

                        case 1:
                            this.<>1__state = -3;
                            flag3 = false;
                            goto TR_000A;

                        case 2:
                            this.<>1__state = -1;
                            goto TR_0004;

                        default:
                            return false;
                    }
                    goto TR_000E;
                TR_0004:
                    return false;
                TR_000A:
                    if (!flag3 || (flag3 && (this.comparer.Compare(this.<maxElement>5__1, this.<element>5__2) < 0)))
                    {
                        this.<maxElement>5__1 = this.<element>5__2;
                        flag3 = true;
                    }
                    local = this.<element>5__2;
                    flag2 = false;
                    this.<element>5__2 = default(TElement);
                TR_000E:
                    while (true)
                    {
                        if (this.<>7__wrap1.MoveNext())
                        {
                            this.<element>5__2 = this.<>7__wrap1.Current;
                            if (!flag2 && !this.equalityComparer.Equals(this.<element>5__2, local))
                            {
                                this.<>2__current = this.<maxElement>5__1;
                                this.<>1__state = 1;
                                flag = true;
                                break;
                            }
                        }
                        else
                        {
                            this.<>m__Finally1();
                            this.<>7__wrap1 = null;
                            if (!flag2)
                            {
                                this.<>2__current = this.<maxElement>5__1;
                                this.<>1__state = 2;
                                flag = true;
                                break;
                            }
                            goto TR_0004;
                        }
                        goto TR_000A;
                    }
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
            {
                EnumerableExtensions.<DistinctLast>d__0<TElement> d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new EnumerableExtensions.<DistinctLast>d__0<TElement>(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = (EnumerableExtensions.<DistinctLast>d__0<TElement>) this;
                }
                d__.source = this.<>3__source;
                d__.equalityComparer = this.<>3__equalityComparer;
                d__.comparer = this.<>3__comparer;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<TElement>.GetEnumerator();

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

            TElement IEnumerator<TElement>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

