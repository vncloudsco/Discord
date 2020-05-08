namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using System.Xml.Linq;

    [CLSCompliant(false)]
    internal class DataServiceContextWrapper : IDataServiceContext, IWeakEventListener
    {
        private const string MetadataKey = "DataServiceMetadata|";
        private static readonly MethodInfo _executeMethodInfo;
        private readonly DataServiceContext _context;
        private readonly Uri _metadataUri;

        public event EventHandler<ReadingWritingEntityEventArgs> ReadingEntity
        {
            add
            {
                this._context.add_ReadingEntity(value);
            }
            remove
            {
                this._context.remove_ReadingEntity(value);
            }
        }

        public event EventHandler<SendingRequest2EventArgs> SendingRequest
        {
            add
            {
                this._context.add_SendingRequest2(value);
            }
            remove
            {
                this._context.remove_SendingRequest2(value);
            }
        }

        static DataServiceContextWrapper()
        {
            Type[] types = new Type[] { typeof(Uri) };
            _executeMethodInfo = typeof(DataServiceContext).GetMethod("Execute", types);
        }

        public DataServiceContextWrapper(Uri serviceRoot)
        {
            if (serviceRoot == null)
            {
                throw new ArgumentNullException("serviceRoot");
            }
            DataServiceContext context1 = new DataServiceContext(serviceRoot);
            context1.set_MergeOption(3);
            this._context = context1;
            this._metadataUri = this._context.GetMetadataUri();
            this.AttachEvents();
        }

        private void AttachEvents()
        {
            DataServiceClientRequestPipelineConfiguration configuration1 = this._context.get_Configurations().get_RequestPipeline();
            configuration1.set_OnMessageCreating((Func<DataServiceClientRequestMessageArgs, DataServiceClientRequestMessage>) Delegate.Combine(configuration1.get_OnMessageCreating(), new Func<DataServiceClientRequestMessageArgs, DataServiceClientRequestMessage>(this.ShimWebRequests)));
        }

        public IDataServiceQuery<T> CreateQuery<T>(string entitySetName) => 
            new DataServiceQueryWrapper<T>(this, this._context.CreateQuery<T>(entitySetName));

        public IDataServiceQuery<T> CreateQuery<T>(string entitySetName, IDictionary<string, object> queryOptions)
        {
            DataServiceQuery<T> query = this._context.CreateQuery<T>(entitySetName);
            foreach (KeyValuePair<string, object> pair in queryOptions)
            {
                query = query.AddQueryOption(pair.Key, pair.Value);
            }
            return new DataServiceQueryWrapper<T>(this, query);
        }

        public IEnumerable<T> Execute<T>(Type elementType, DataServiceQueryContinuation continuation)
        {
            Type[] typeArguments = new Type[] { elementType };
            object[] parameters = new object[] { continuation.get_NextLinkUri() };
            return (IEnumerable<T>) _executeMethodInfo.MakeGenericMethod(typeArguments).Invoke(this._context, parameters);
        }

        public IEnumerable<T> ExecuteBatch<T>(DataServiceRequest request)
        {
            DataServiceRequest[] requestArray1 = new DataServiceRequest[] { request };
            return (from o in this._context.ExecuteBatch(requestArray1).Cast<QueryOperationResponse>() select o.Cast<T>());
        }

        internal static DataServiceMetadata ExtractMetadataFromSchema(Stream schemaStream)
        {
            XDocument document;
            if (schemaStream == null)
            {
                return null;
            }
            try
            {
                document = XmlUtility.LoadSafe(schemaStream);
            }
            catch
            {
                return null;
            }
            return ExtractMetadataInternal(document);
        }

        private static DataServiceMetadata ExtractMetadataInternal(XDocument schemaDocument)
        {
            var type = (from e in schemaDocument.Descendants()
                where e.Name.LocalName == "EntityContainer"
                let entitySet = Enumerable.FirstOrDefault<XElement>(e.Elements(), el => el.Name.LocalName == "EntitySet")
                let name = entitySet?.Attribute("Name").Value
                where (name != null) && name.Equals("Packages", StringComparison.OrdinalIgnoreCase)
                select new { 
                    Container = e,
                    EntitySet = entitySet
                }).FirstOrDefault();
            if (type == null)
            {
                return null;
            }
            XElement container = type.Container;
            XAttribute attribute = type.EntitySet.Attribute("EntityType");
            string packageEntityName = null;
            if (attribute != null)
            {
                packageEntityName = attribute.Value;
            }
            DataServiceMetadata metadata1 = new DataServiceMetadata();
            DataServiceMetadata metadata2 = new DataServiceMetadata();
            metadata2.SupportedMethodNames = new HashSet<string>(from e in container.Elements()
                where e.Name.LocalName == "FunctionImport"
                select e.Attribute("Name").Value, StringComparer.OrdinalIgnoreCase);
            DataServiceMetadata local8 = metadata2;
            local8.SupportedProperties = new HashSet<string>(ExtractSupportedProperties(schemaDocument, packageEntityName), StringComparer.OrdinalIgnoreCase);
            return local8;
        }

        private static IEnumerable<string> ExtractSupportedProperties(XDocument schemaDocument, string packageEntityName)
        {
            packageEntityName = TrimNamespace(packageEntityName);
            XElement element = (from e in schemaDocument.Descendants()
                where e.Name.LocalName == "EntityType"
                let attribute = e.Attribute("Name")
                where (attribute != null) && attribute.Value.Equals(packageEntityName, StringComparison.OrdinalIgnoreCase)
                select e).FirstOrDefault<XElement>();
            return ((element == null) ? Enumerable.Empty<string>() : (from e in element.Elements()
                where e.Name.LocalName == "Property"
                select e.Attribute("Name").Value));
        }

        private static DataServiceMetadata GetDataServiceMetadata(Uri metadataUri)
        {
            if (metadataUri == null)
            {
                return null;
            }
            HttpClient client = new HttpClient(metadataUri);
            using (MemoryStream stream = new MemoryStream())
            {
                client.DownloadData(stream);
                stream.Seek(0L, SeekOrigin.Begin);
                return ExtractMetadataFromSchema(stream);
            }
        }

        private string GetServiceMetadataKey() => 
            ("DataServiceMetadata|" + this._metadataUri.OriginalString);

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e) => 
            (managerType == typeof(Func<DataServiceClientRequestMessage, DataServiceClientRequestMessageArgs>));

        private DataServiceClientRequestMessage ShimWebRequests(DataServiceClientRequestMessageArgs args) => 
            HttpShim.Instance.ShimDataServiceRequest(args);

        public bool SupportsProperty(string propertyName) => 
            ((this.ServiceMetadata != null) && this.ServiceMetadata.SupportedProperties.Contains(propertyName));

        public bool SupportsServiceMethod(string methodName) => 
            ((this.ServiceMetadata != null) && this.ServiceMetadata.SupportedMethodNames.Contains(methodName));

        private static string TrimNamespace(string packageEntityName)
        {
            int num = packageEntityName.LastIndexOf('.');
            if ((num > 0) && (num < packageEntityName.Length))
            {
                packageEntityName = packageEntityName.Substring(num + 1);
            }
            return packageEntityName;
        }

        public Uri BaseUri =>
            this._context.get_BaseUri();

        public bool IgnoreMissingProperties
        {
            get => 
                this._context.get_IgnoreMissingProperties();
            set => 
                this._context.set_IgnoreMissingProperties(value);
        }

        private DataServiceMetadata ServiceMetadata =>
            MemoryCache.Instance.GetOrAdd<DataServiceMetadata>(this.GetServiceMetadataKey(), () => GetDataServiceMetadata(this._metadataUri), TimeSpan.FromMinutes(15.0), false);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly DataServiceContextWrapper.<>c <>9 = new DataServiceContextWrapper.<>c();
            public static Func<XElement, bool> <>9__31_0;
            public static Func<XElement, bool> <>9__31_2;
            public static Func<XElement, <>f__AnonymousType11<XElement, XElement>> <>9__31_1;
            public static Func<<>f__AnonymousType11<XElement, XElement>, <>f__AnonymousType12<<>f__AnonymousType11<XElement, XElement>, string>> <>9__31_3;
            public static Func<<>f__AnonymousType12<<>f__AnonymousType11<XElement, XElement>, string>, bool> <>9__31_4;
            public static Func<<>f__AnonymousType12<<>f__AnonymousType11<XElement, XElement>, string>, <>f__AnonymousType13<XElement, XElement>> <>9__31_5;
            public static Func<XElement, bool> <>9__31_6;
            public static Func<XElement, string> <>9__31_7;
            public static Func<XElement, bool> <>9__32_0;
            public static Func<XElement, <>f__AnonymousType14<XElement, XAttribute>> <>9__32_1;
            public static Func<<>f__AnonymousType14<XElement, XAttribute>, XElement> <>9__32_3;
            public static Func<XElement, bool> <>9__32_4;
            public static Func<XElement, string> <>9__32_5;

            internal bool <ExtractMetadataInternal>b__31_0(XElement e) => 
                (e.Name.LocalName == "EntityContainer");

            internal <>f__AnonymousType11<XElement, XElement> <ExtractMetadataInternal>b__31_1(XElement e) => 
                new { 
                    e = e,
                    entitySet = Enumerable.FirstOrDefault<XElement>(e.Elements(), el => el.Name.LocalName == "EntitySet")
                };

            internal bool <ExtractMetadataInternal>b__31_2(XElement el) => 
                (el.Name.LocalName == "EntitySet");

            internal <>f__AnonymousType12<<>f__AnonymousType11<XElement, XElement>, string> <ExtractMetadataInternal>b__31_3(<>f__AnonymousType11<XElement, XElement> <>h__TransparentIdentifier0) => 
                new { 
                    <>h__TransparentIdentifier0 = <>h__TransparentIdentifier0,
                    name = <>h__TransparentIdentifier0.entitySet?.Attribute("Name").Value
                };

            internal bool <ExtractMetadataInternal>b__31_4(<>f__AnonymousType12<<>f__AnonymousType11<XElement, XElement>, string> <>h__TransparentIdentifier1) => 
                ((<>h__TransparentIdentifier1.name != null) && <>h__TransparentIdentifier1.name.Equals("Packages", StringComparison.OrdinalIgnoreCase));

            internal <>f__AnonymousType13<XElement, XElement> <ExtractMetadataInternal>b__31_5(<>f__AnonymousType12<<>f__AnonymousType11<XElement, XElement>, string> <>h__TransparentIdentifier1) => 
                new { 
                    Container = <>h__TransparentIdentifier1.<>h__TransparentIdentifier0.e,
                    EntitySet = <>h__TransparentIdentifier1.<>h__TransparentIdentifier0.entitySet
                };

            internal bool <ExtractMetadataInternal>b__31_6(XElement e) => 
                (e.Name.LocalName == "FunctionImport");

            internal string <ExtractMetadataInternal>b__31_7(XElement e) => 
                e.Attribute("Name").Value;

            internal bool <ExtractSupportedProperties>b__32_0(XElement e) => 
                (e.Name.LocalName == "EntityType");

            internal <>f__AnonymousType14<XElement, XAttribute> <ExtractSupportedProperties>b__32_1(XElement e) => 
                new { 
                    e = e,
                    attribute = e.Attribute("Name")
                };

            internal XElement <ExtractSupportedProperties>b__32_3(<>f__AnonymousType14<XElement, XAttribute> <>h__TransparentIdentifier0) => 
                <>h__TransparentIdentifier0.e;

            internal bool <ExtractSupportedProperties>b__32_4(XElement e) => 
                (e.Name.LocalName == "Property");

            internal string <ExtractSupportedProperties>b__32_5(XElement e) => 
                e.Attribute("Name").Value;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c__24<T>
        {
            public static readonly DataServiceContextWrapper.<>c__24<T> <>9;
            public static Func<QueryOperationResponse, IEnumerable<T>> <>9__24_0;

            static <>c__24()
            {
                DataServiceContextWrapper.<>c__24<T>.<>9 = new DataServiceContextWrapper.<>c__24<T>();
            }

            internal IEnumerable<T> <ExecuteBatch>b__24_0(QueryOperationResponse o) => 
                o.Cast<T>();
        }

        internal sealed class DataServiceMetadata
        {
            public HashSet<string> SupportedMethodNames { get; set; }

            public HashSet<string> SupportedProperties { get; set; }
        }
    }
}

