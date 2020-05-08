namespace NuGet
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    internal class AggregateQuery<TVal> : IQueryable<TVal>, IEnumerable<TVal>, IEnumerable, IQueryable, IQueryProvider, IOrderedQueryable<TVal>, IOrderedQueryable
    {
        private const int QueryCacheSize = 30;
        private readonly IEnumerable<IQueryable<TVal>> _queryables;
        private readonly System.Linq.Expressions.Expression _expression;
        private readonly IEqualityComparer<TVal> _equalityComparer;
        private readonly IList<IEnumerable<TVal>> _subQueries;
        private readonly bool _ignoreFailures;
        private readonly ILogger _logger;

        public AggregateQuery(IEnumerable<IQueryable<TVal>> queryables, IEqualityComparer<TVal> equalityComparer, ILogger logger, bool ignoreFailures)
        {
            this._queryables = queryables;
            this._equalityComparer = equalityComparer;
            this._expression = System.Linq.Expressions.Expression.Constant(this);
            this._ignoreFailures = ignoreFailures;
            this._logger = logger;
            this._subQueries = this.GetSubQueries(this._expression);
        }

        private AggregateQuery(IEnumerable<IQueryable<TVal>> queryables, IEqualityComparer<TVal> equalityComparer, IList<IEnumerable<TVal>> subQueries, System.Linq.Expressions.Expression expression, ILogger logger, bool ignoreInvalidRepositories)
        {
            this._queryables = queryables;
            this._equalityComparer = equalityComparer;
            this._expression = expression;
            this._subQueries = subQueries;
            this._ignoreFailures = ignoreInvalidRepositories;
            this._logger = logger;
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }
            Type elementType = QueryableUtility.FindGenericType(typeof(IQueryable<>), expression.Type);
            if (elementType == null)
            {
                throw new ArgumentException(string.Empty, "expression");
            }
            return this.CreateQuery(elementType, expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression) => 
            ((IQueryable<TElement>) this.CreateQuery(typeof(TElement), expression));

        private IQueryable CreateQuery(Type elementType, System.Linq.Expressions.Expression expression)
        {
            Type[] typeArguments = new Type[] { elementType };
            IList<IEnumerable<TVal>> subQueries = this._subQueries;
            if (QueryableUtility.IsQueryableMethod(expression, "Where") || QueryableUtility.IsOrderingMethod(expression))
            {
                subQueries = this.GetSubQueries(expression);
            }
            object[] parameters = new object[] { this._queryables, this._equalityComparer, subQueries, expression, this._logger, this._ignoreFailures };
            return (IQueryable) typeof(AggregateQuery<>).MakeGenericType(typeArguments).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Single<ConstructorInfo>().Invoke(parameters);
        }

        public object Execute(System.Linq.Expressions.Expression expression) => 
            this.Execute<object>(expression);

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            IQueryable<TResult> queryable = (from queryable in this._queryables select ((AggregateQuery<TVal>) this).TryExecute<TResult>(queryable, expression)).AsQueryable<TResult>();
            return (!QueryableUtility.IsQueryableMethod(expression, "Count") ? this.TryExecute<TResult>(queryable, expression) : ((TResult) queryable.Cast<int>().Sum()));
        }

        private static TResult Execute<TResult>(IQueryable queryable, System.Linq.Expressions.Expression expression) => 
            queryable.Provider.Execute<TResult>(AggregateQuery<TVal>.Rewrite(queryable, expression));

        private IEnumerable<TVal> GetAggregateEnumerable()
        {
            OrderingComparer<TVal> comparer = new OrderingComparer<TVal>(this.Expression);
            return (comparer.CanCompare ? this.ReadOrderedQueues(comparer) : (from query in this._subQueries select base._ignoreFailures ? query.SafeIterate<TVal>() : query).Distinct<TVal>(this._equalityComparer));
        }

        public IEnumerator<TVal> GetEnumerator()
        {
            IQueryable<TVal> queryable = this.GetAggregateEnumerable().AsQueryable<TVal>();
            System.Linq.Expressions.Expression expression = AggregateQuery<TVal>.RewriteForAggregation(queryable, this.Expression);
            return queryable.Provider.CreateQuery<TVal>(expression).GetEnumerator();
        }

        private IList<IEnumerable<TVal>> GetSubQueries(System.Linq.Expressions.Expression expression) => 
            (from query in this._queryables select AggregateQuery<TVal>.GetSubQuery(query, expression)).ToList<IEnumerable<TVal>>();

        private static IEnumerable<TVal> GetSubQuery(IQueryable queryable, System.Linq.Expressions.Expression expression)
        {
            expression = AggregateQuery<TVal>.Rewrite(queryable, expression);
            return new BufferedEnumerable<TVal>(queryable.Provider.CreateQuery<TVal>(expression), 30);
        }

        private void LogWarning(Exception ex)
        {
            this._logger.Log(MessageLevel.Warning, ExceptionUtility.Unwrap(ex).Message, new object[0]);
        }

        [IteratorStateMachine(typeof(<ReadOrderedQueues>d__22))]
        private IEnumerable<TVal> ReadOrderedQueues(IComparer<TVal> comparer)
        {
            <ReadOrderedQueues>d__22<TVal> d__1 = new <ReadOrderedQueues>d__22<TVal>(-2);
            d__1.<>4__this = (AggregateQuery<TVal>) this;
            d__1.<>3__comparer = comparer;
            return d__1;
        }

        private TaskResult<TVal> ReadQueue(LazyQueue<TVal> queue)
        {
            TVal local;
            TaskResult<TVal> result1 = new TaskResult<TVal>();
            result1.Queue = queue;
            TaskResult<TVal> result = result1;
            if (!this._ignoreFailures)
            {
                result.HasValue = queue.TryPeek(out local);
            }
            else
            {
                try
                {
                    result.HasValue = queue.TryPeek(out local);
                }
                catch (Exception exception)
                {
                    this.LogWarning(exception);
                    local = default(TVal);
                }
            }
            result.Value = local;
            return result;
        }

        private static System.Linq.Expressions.Expression Rewrite(IQueryable queryable, System.Linq.Expressions.Expression expression)
        {
            string[] methodsToExclude = new string[] { "Skip", "Take" };
            return new ExpressionRewriter(queryable, methodsToExclude).Visit(expression);
        }

        private static System.Linq.Expressions.Expression RewriteForAggregation(IQueryable queryable, System.Linq.Expressions.Expression expression)
        {
            string[] methodsToExclude = new string[] { "Where", "OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending" };
            return new ExpressionRewriter(queryable, methodsToExclude).Visit(expression);
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        private TResult TryExecute<TResult>(IQueryable queryable, System.Linq.Expressions.Expression expression)
        {
            if (!this._ignoreFailures)
            {
                return AggregateQuery<TVal>.Execute<TResult>(queryable, expression);
            }
            try
            {
                return AggregateQuery<TVal>.Execute<TResult>(queryable, expression);
            }
            catch (Exception exception)
            {
                this.LogWarning(exception);
                return default(TResult);
            }
        }

        public Type ElementType =>
            typeof(TVal);

        public System.Linq.Expressions.Expression Expression =>
            this._expression;

        public IQueryProvider Provider =>
            this;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly AggregateQuery<TVal>.<>c <>9;
            public static Func<IEnumerable<TVal>, LazyQueue<TVal>> <>9__22_0;

            static <>c()
            {
                AggregateQuery<TVal>.<>c.<>9 = new AggregateQuery<TVal>.<>c();
            }

            internal LazyQueue<TVal> <ReadOrderedQueues>b__22_0(IEnumerable<TVal> query) => 
                new LazyQueue<TVal>(query.GetEnumerator());
        }

        [CompilerGenerated]
        private sealed class <ReadOrderedQueues>d__22 : IEnumerable<TVal>, IEnumerable, IEnumerator<TVal>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private TVal <>2__current;
            private int <>l__initialThreadId;
            public AggregateQuery<TVal> <>4__this;
            private List<LazyQueue<TVal>> <lazyQueues>5__1;
            private IComparer<TVal> comparer;
            public IComparer<TVal> <>3__comparer;
            private HashSet<TVal> <seen>5__2;
            private LazyQueue<TVal> <minQueue>5__3;

            [DebuggerHidden]
            public <ReadOrderedQueues>d__22(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private bool MoveNext()
            {
                TVal local;
                int num = this.<>1__state;
                if (num == 0)
                {
                    this.<>1__state = -1;
                    this.<lazyQueues>5__1 = Enumerable.Select<IEnumerable<TVal>, LazyQueue<TVal>>(this.<>4__this._subQueries, AggregateQuery<TVal>.<>c.<>9__22_0 ?? (AggregateQuery<TVal>.<>c.<>9__22_0 = new Func<IEnumerable<TVal>, LazyQueue<TVal>>(this.<ReadOrderedQueues>b__22_0))).ToList<LazyQueue<TVal>>();
                    this.<seen>5__2 = new HashSet<TVal>(this.<>4__this._equalityComparer);
                }
                else
                {
                    if (num != 1)
                    {
                        return false;
                    }
                    this.<>1__state = -1;
                    goto TR_0012;
                }
            TR_000D:
                local = default(TVal);
                this.<minQueue>5__3 = null;
                Task<AggregateQuery<TVal>.TaskResult>[] tasks = Enumerable.Select<LazyQueue<TVal>, Task<AggregateQuery<TVal>.TaskResult>>(this.<lazyQueues>5__1, new Func<LazyQueue<TVal>, Task<AggregateQuery<TVal>.TaskResult>>(this.<>4__this.<ReadOrderedQueues>b__22_1)).ToArray<Task<AggregateQuery<TVal>.TaskResult>>();
                Task.WaitAll(tasks);
                Task<AggregateQuery<TVal>.TaskResult>[] taskArray = tasks;
                int index = 0;
                while (true)
                {
                    if (index < taskArray.Length)
                    {
                        Task<AggregateQuery<TVal>.TaskResult> task = taskArray[index];
                        if (!task.Result.HasValue)
                        {
                            this.<lazyQueues>5__1.Remove(task.Result.Queue);
                        }
                        else if ((local == null) || (this.comparer.Compare(task.Result.Value, local) < 0))
                        {
                            local = task.Result.Value;
                            this.<minQueue>5__3 = task.Result.Queue;
                        }
                        index++;
                        continue;
                    }
                    if (!this.<lazyQueues>5__1.Any<LazyQueue<TVal>>())
                    {
                        goto TR_0010;
                    }
                    else if (this.<seen>5__2.Add(local))
                    {
                        this.<>2__current = local;
                        this.<>1__state = 1;
                        return true;
                    }
                    break;
                }
                goto TR_0012;
            TR_0010:
                while (true)
                {
                    this.<minQueue>5__3 = null;
                    if (this.<lazyQueues>5__1.Count > 0)
                    {
                        break;
                    }
                    return false;
                }
                goto TR_000D;
            TR_0012:
                while (true)
                {
                    this.<minQueue>5__3.Dequeue();
                    break;
                }
                goto TR_0010;
            }

            [DebuggerHidden]
            IEnumerator<TVal> IEnumerable<TVal>.GetEnumerator()
            {
                AggregateQuery<TVal>.<ReadOrderedQueues>d__22 d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = (AggregateQuery<TVal>.<ReadOrderedQueues>d__22) this;
                }
                else
                {
                    d__ = new AggregateQuery<TVal>.<ReadOrderedQueues>d__22(0) {
                        <>4__this = this.<>4__this
                    };
                }
                d__.comparer = this.<>3__comparer;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<TVal>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
            }

            TVal IEnumerator<TVal>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }

        private class TaskResult
        {
            public LazyQueue<TVal> Queue { get; set; }

            public bool HasValue { get; set; }

            public TVal Value { get; set; }
        }
    }
}

