namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Xml.Linq;

    [CLSCompliant(false)]
    internal class DataServiceQueryWrapper<T> : IDataServiceQuery<T>, IDataServiceQuery
    {
        private const int MaxUrlLength = 0x800;
        private readonly DataServiceQuery _query;
        private readonly IDataServiceContext _context;
        private readonly Type _concreteType;

        public DataServiceQueryWrapper(IDataServiceContext context, DataServiceQuery query) : this(context, query, typeof(T))
        {
        }

        public DataServiceQueryWrapper(IDataServiceContext context, DataServiceQuery query, Type concreteType)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            this._context = context;
            this._query = query;
            this._concreteType = concreteType;
        }

        public IQueryable<T> AsQueryable() => 
            ((IQueryable<T>) this._query);

        public IDataServiceQuery<TElement> CreateQuery<TElement>(Expression expression)
        {
            expression = this.GetInnerExpression(expression);
            return new DataServiceQueryWrapper<TElement>(this._context, (DataServiceQuery) this._query.get_Provider().CreateQuery<TElement>(expression), typeof(T));
        }

        private TResult Execute<TResult>(Func<TResult> action)
        {
            TResult local;
            try
            {
                local = action();
            }
            catch (Exception exception)
            {
                string str = DataServiceQueryWrapper<T>.ExtractMessageFromClientException(exception);
                if (!string.IsNullOrEmpty(str))
                {
                    throw new InvalidOperationException(str, exception);
                }
                object[] args = new object[] { this._context.BaseUri };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.InvalidFeed, args), exception);
            }
            return local;
        }

        public object Execute(Expression expression) => 
            this.Execute<object>((Func<object>) (() => ((DataServiceQueryWrapper<T>) this)._query.get_Provider().Execute(((DataServiceQueryWrapper<T>) this).GetInnerExpression(expression))));

        public TResult Execute<TResult>(Expression expression) => 
            this.Execute<TResult>((Func<TResult>) (() => ((DataServiceQueryWrapper<T>) this)._query.get_Provider().Execute<TResult>(((DataServiceQueryWrapper<T>) this).GetInnerExpression(expression))));

        private static string ExtractMessageFromClientException(Exception exception)
        {
            DataServiceQueryException exception2 = exception as DataServiceQueryException;
            if ((exception2 != null) && (exception2.InnerException != null))
            {
                XDocument document;
                DataServiceClientException innerException = exception2.InnerException as DataServiceClientException;
                if ((exception2 != null) && (XmlUtility.TryParseDocument(innerException.Message, out document) && document.Root.Name.LocalName.Equals("error", StringComparison.OrdinalIgnoreCase)))
                {
                    return document.Root.GetOptionalElementValue("message", null);
                }
            }
            return null;
        }

        [IteratorStateMachine(typeof(<GetAll>d__14))]
        private IEnumerable<T> GetAll()
        {
            <GetAll>d__14<T> d__1 = new <GetAll>d__14<T>(-2);
            d__1.<>4__this = (DataServiceQueryWrapper<T>) this;
            return d__1;
        }

        public IEnumerator<T> GetEnumerator() => 
            this.GetAll().GetEnumerator();

        private Expression GetInnerExpression(Expression expression) => 
            QueryableUtility.ReplaceQueryableExpression(this._query, expression);

        public DataServiceRequest GetRequest(Expression expression) => 
            ((DataServiceRequest) this._query.get_Provider().CreateQuery(this.GetInnerExpression(expression)));

        public virtual Uri GetRequestUri(Expression expression) => 
            this.GetRequest(expression).get_RequestUri();

        public bool RequiresBatch(Expression expression) => 
            (this.GetRequestUri(expression).AbsoluteUri.Length >= 0x800);

        public override string ToString() => 
            this._query.ToString();

        [CompilerGenerated]
        private sealed class <GetAll>d__14 : IEnumerable<T>, IEnumerable, IEnumerator<T>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private T <>2__current;
            private int <>l__initialThreadId;
            public DataServiceQueryWrapper<T> <>4__this;
            private IEnumerable <results>5__1;
            private IDataServiceContext <>7__wrap1;
            private bool <>7__wrap2;
            private IEnumerator <>7__wrap3;

            [DebuggerHidden]
            public <GetAll>d__14(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap2)
                {
                    Monitor.Exit(this.<>7__wrap1);
                }
            }

            private void <>m__Finally2()
            {
                this.<>1__state = -3;
                IDisposable disposable = this.<>7__wrap3 as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
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
                        DataServiceQuery query = this.<>4__this._query;
                        if (typeof(T) == typeof(IPackage))
                        {
                            query = (DataServiceQuery) this.<>4__this._query.get_Provider().CreateQuery<DataServicePackage>(this.<>4__this._query.get_Expression()).Cast<DataServicePackage>();
                        }
                        this.<results>5__1 = this.<>4__this.Execute<IEnumerable>(new Func<IEnumerable>(query.Execute));
                        goto TR_0004;
                    }
                    else if (num == 1)
                    {
                        this.<>1__state = -4;
                    }
                    else
                    {
                        return false;
                    }
                    goto TR_000B;
                TR_0004:
                    this.<>7__wrap1 = this.<>4__this._context;
                    this.<>7__wrap2 = false;
                    this.<>1__state = -3;
                    Monitor.Enter(this.<>7__wrap1, ref this.<>7__wrap2);
                    this.<>7__wrap3 = this.<results>5__1.GetEnumerator();
                    this.<>1__state = -4;
                TR_000B:
                    while (true)
                    {
                        if (this.<>7__wrap3.MoveNext())
                        {
                            T current = (T) this.<>7__wrap3.Current;
                            this.<>2__current = current;
                            this.<>1__state = 1;
                            flag = true;
                        }
                        else
                        {
                            this.<>m__Finally2();
                            this.<>7__wrap3 = null;
                            this.<>m__Finally1();
                            this.<>7__wrap1 = null;
                            DataServiceQueryContinuation continuation = ((QueryOperationResponse) this.<results>5__1).GetContinuation();
                            if (continuation != null)
                            {
                                this.<results>5__1 = this.<>4__this._context.Execute<T>(this.<>4__this._concreteType, continuation);
                            }
                            if (continuation != null)
                            {
                                break;
                            }
                            flag = false;
                        }
                        return flag;
                    }
                    goto TR_0004;
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                DataServiceQueryWrapper<T>.<GetAll>d__14 d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = (DataServiceQueryWrapper<T>.<GetAll>d__14) this;
                }
                else
                {
                    d__ = new DataServiceQueryWrapper<T>.<GetAll>d__14(0) {
                        <>4__this = this.<>4__this
                    };
                }
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
                int num = this.<>1__state;
                if (((num == -4) || (num == -3)) || (num == 1))
                {
                    try
                    {
                        if ((num == -4) || (num == 1))
                        {
                            try
                            {
                            }
                            finally
                            {
                                this.<>m__Finally2();
                            }
                        }
                    }
                    finally
                    {
                        this.<>m__Finally1();
                    }
                }
            }

            T IEnumerator<T>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

