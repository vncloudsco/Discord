namespace NuGet
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    [CLSCompliant(false)]
    internal class SmartDataServiceQuery<T> : IQueryable<T>, IEnumerable<T>, IEnumerable, IQueryable, IQueryProvider, IOrderedQueryable<T>, IOrderedQueryable
    {
        private readonly IDataServiceContext _context;
        private readonly IDataServiceQuery _query;

        public SmartDataServiceQuery(IDataServiceContext context, IDataServiceQuery query)
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
            this.Expression = System.Linq.Expressions.Expression.Constant(this);
        }

        public SmartDataServiceQuery(IDataServiceContext context, string entitySetName)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (string.IsNullOrEmpty(entitySetName))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "entitySetName");
            }
            this._context = context;
            this._query = context.CreateQuery<T>(entitySetName);
            this.Expression = System.Linq.Expressions.Expression.Constant(this);
        }

        private SmartDataServiceQuery(IDataServiceContext context, IDataServiceQuery query, System.Linq.Expressions.Expression expression)
        {
            this._context = context;
            this._query = query;
            this.Expression = expression;
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
            object[] parameters = new object[] { this._context, this._query, expression };
            return (IQueryable) typeof(SmartDataServiceQuery<>).MakeGenericType(typeArguments).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).Single<ConstructorInfo>().Invoke(parameters);
        }

        public object Execute(System.Linq.Expressions.Expression expression)
        {
            DataServiceRequest request = this._query.GetRequest(expression);
            return (!this._query.RequiresBatch(expression) ? this._query.Execute(expression) : this._context.ExecuteBatch<object>(request).FirstOrDefault<object>());
        }

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            DataServiceRequest request = this._query.GetRequest(expression);
            return (!this._query.RequiresBatch(expression) ? this._query.Execute<TResult>(expression) : this._context.ExecuteBatch<TResult>(request).FirstOrDefault<TResult>());
        }

        public IEnumerator<T> GetEnumerator()
        {
            DataServiceRequest request = this._query.GetRequest(this.Expression);
            return (!this._query.RequiresBatch(this.Expression) ? this._query.CreateQuery<T>(this.Expression).GetEnumerator() : this._context.ExecuteBatch<T>(request).GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator() => 
            this.GetEnumerator();

        public override string ToString() => 
            this._query.CreateQuery<T>(this.Expression).ToString();

        public Type ElementType =>
            typeof(T);

        public System.Linq.Expressions.Expression Expression { get; private set; }

        public IQueryProvider Provider =>
            this;
    }
}

