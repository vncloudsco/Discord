namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [CLSCompliant(false)]
    internal interface IDataServiceQuery<out T> : IDataServiceQuery
    {
        IQueryable<T> AsQueryable();
        IEnumerator<T> GetEnumerator();
    }
}

