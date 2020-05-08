namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Runtime.CompilerServices;

    [CLSCompliant(false)]
    internal interface IDataServiceContext
    {
        event EventHandler<ReadingWritingEntityEventArgs> ReadingEntity;

        event EventHandler<SendingRequest2EventArgs> SendingRequest;

        IDataServiceQuery<T> CreateQuery<T>(string entitySetName);
        IDataServiceQuery<T> CreateQuery<T>(string entitySetName, IDictionary<string, object> queryOptions);
        IEnumerable<T> Execute<T>(Type elementType, DataServiceQueryContinuation continuation);
        IEnumerable<T> ExecuteBatch<T>(DataServiceRequest request);
        bool SupportsProperty(string propertyName);
        bool SupportsServiceMethod(string methodName);

        Uri BaseUri { get; }

        bool IgnoreMissingProperties { get; set; }
    }
}

