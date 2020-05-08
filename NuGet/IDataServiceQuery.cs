namespace NuGet
{
    using System;
    using System.Data.Services.Client;
    using System.Linq.Expressions;

    [CLSCompliant(false)]
    internal interface IDataServiceQuery
    {
        IDataServiceQuery<TElement> CreateQuery<TElement>(Expression expression);
        object Execute(Expression expression);
        TResult Execute<TResult>(Expression expression);
        DataServiceRequest GetRequest(Expression expression);
        bool RequiresBatch(Expression expression);
    }
}

