namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Data.Services.Client;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Xml.Linq;

    internal class DataServicePackageRepository : PackageRepositoryBase, IHttpClientEvents, IProgressProvider, IServiceBasedRepository, IPackageRepository, ICloneableRepository, ICultureAwareRepository, IOperationAwareRepository, IPackageLookup, ILatestPackageLookup, IWeakEventListener
    {
        private const string FindPackagesByIdSvcMethod = "FindPackagesById";
        private const string PackageServiceEntitySetName = "Packages";
        private const string SearchSvcMethod = "Search";
        private const string GetUpdatesSvcMethod = "GetUpdates";
        private IDataServiceContext _context;
        private readonly IHttpClient _httpClient;
        private readonly NuGet.PackageDownloader _packageDownloader;
        private CultureInfo _culture;
        private Tuple<string, string, string> _currentOperation;
        [CompilerGenerated]
        private EventHandler<WebRequestEventArgs> _sendingRequest;

        private event EventHandler<WebRequestEventArgs> _sendingRequest
        {
            [CompilerGenerated] add
            {
                EventHandler<WebRequestEventArgs> objA = this._sendingRequest;
                while (true)
                {
                    EventHandler<WebRequestEventArgs> a = objA;
                    EventHandler<WebRequestEventArgs> handler3 = (EventHandler<WebRequestEventArgs>) Delegate.Combine(a, value);
                    objA = Interlocked.CompareExchange<EventHandler<WebRequestEventArgs>>(ref this._sendingRequest, handler3, a);
                    if (ReferenceEquals(objA, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<WebRequestEventArgs> objA = this._sendingRequest;
                while (true)
                {
                    EventHandler<WebRequestEventArgs> source = objA;
                    EventHandler<WebRequestEventArgs> handler3 = (EventHandler<WebRequestEventArgs>) Delegate.Remove(source, value);
                    objA = Interlocked.CompareExchange<EventHandler<WebRequestEventArgs>>(ref this._sendingRequest, handler3, source);
                    if (ReferenceEquals(objA, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EventHandler<ProgressEventArgs> ProgressAvailable
        {
            add
            {
                this._packageDownloader.ProgressAvailable += value;
            }
            remove
            {
                this._packageDownloader.ProgressAvailable -= value;
            }
        }

        public event EventHandler<WebRequestEventArgs> SendingRequest
        {
            add
            {
                this._packageDownloader.SendingRequest += value;
                this._httpClient.SendingRequest += value;
                this._sendingRequest += value;
            }
            remove
            {
                this._packageDownloader.SendingRequest -= value;
                this._httpClient.SendingRequest -= value;
                this._sendingRequest -= value;
            }
        }

        public DataServicePackageRepository(IHttpClient client) : this(client, new NuGet.PackageDownloader())
        {
        }

        public DataServicePackageRepository(Uri serviceRoot) : this(new HttpClient(serviceRoot))
        {
        }

        public DataServicePackageRepository(IHttpClient client, NuGet.PackageDownloader packageDownloader)
        {
            if (client == null)
            {
                throw new ArgumentNullException("client");
            }
            if (packageDownloader == null)
            {
                throw new ArgumentNullException("packageDownloader");
            }
            this._httpClient = client;
            this._httpClient.AcceptCompression = true;
            this._packageDownloader = packageDownloader;
            if (!EnvironmentUtility.RunningFromCommandLine && !EnvironmentUtility.IsMonoRuntime)
            {
                SendingRequestEventManager.AddListener(this._packageDownloader, this);
            }
            else
            {
                this._packageDownloader.SendingRequest += new EventHandler<WebRequestEventArgs>(this.OnPackageDownloaderSendingRequest);
            }
        }

        public IPackageRepository Clone() => 
            new DataServicePackageRepository(this._httpClient, this._packageDownloader);

        public bool Exists(string packageId, SemanticVersion version)
        {
            bool flag;
            IQueryable<DataServicePackage> queryable = this.Context.CreateQuery<DataServicePackage>("Packages").AsQueryable();
            using (IEnumerator<string> enumerator = version.GetComparableVersionStrings().GetEnumerator())
            {
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        string versionString = enumerator.Current;
                        try
                        {
                            <>c__DisplayClass38_1 class_2;
                            ParameterExpression expression = Expression.Parameter(typeof(DataServicePackage), "p");
                            ParameterExpression[] parameters = new ParameterExpression[] { expression };
                            IQueryable<DataServicePackage> queryable1 = Queryable.Where<DataServicePackage>(queryable, Expression.Lambda<Func<DataServicePackage, bool>>(Expression.AndAlso(Expression.Equal(Expression.Property(expression, (MethodInfo) methodof(DataServicePackage.get_Id)), Expression.Field(Expression.Field(Expression.Constant(class_2, typeof(<>c__DisplayClass38_1)), fieldof(<>c__DisplayClass38_1.CS$<>8__locals1)), fieldof(<>c__DisplayClass38_0.packageId))), Expression.Equal(Expression.Property(expression, (MethodInfo) methodof(DataServicePackage.get_Version)), Expression.Field(Expression.Constant(class_2, typeof(<>c__DisplayClass38_1)), fieldof(<>c__DisplayClass38_1.versionString)))), parameters));
                            expression = Expression.Parameter(typeof(DataServicePackage), "p");
                            ParameterExpression[] expressionArray2 = new ParameterExpression[] { expression };
                            if (Queryable.Select<DataServicePackage, string>(queryable1, Expression.Lambda<Func<DataServicePackage, string>>(Expression.Property(expression, (MethodInfo) methodof(DataServicePackage.get_Id)), expressionArray2)).ToArray<string>().Length == 1)
                            {
                                flag = true;
                                break;
                            }
                        }
                        catch (DataServiceQueryException)
                        {
                        }
                        continue;
                    }
                    return false;
                }
            }
            return flag;
        }

        public IPackage FindPackage(string packageId, SemanticVersion version)
        {
            IPackage package;
            IQueryable<DataServicePackage> queryable = this.Context.CreateQuery<DataServicePackage>("Packages").AsQueryable();
            using (IEnumerator<string> enumerator = version.GetComparableVersionStrings().GetEnumerator())
            {
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        string versionString = enumerator.Current;
                        try
                        {
                            <>c__DisplayClass39_1 class_2;
                            ParameterExpression expression = Expression.Parameter(typeof(DataServicePackage), "p");
                            ParameterExpression[] parameters = new ParameterExpression[] { expression };
                            DataServicePackage[] packageArray = Queryable.Where<DataServicePackage>(queryable, Expression.Lambda<Func<DataServicePackage, bool>>(Expression.AndAlso(Expression.Equal(Expression.Property(expression, (MethodInfo) methodof(DataServicePackage.get_Id)), Expression.Field(Expression.Field(Expression.Constant(class_2, typeof(<>c__DisplayClass39_1)), fieldof(<>c__DisplayClass39_1.CS$<>8__locals1)), fieldof(<>c__DisplayClass39_0.packageId))), Expression.Equal(Expression.Property(expression, (MethodInfo) methodof(DataServicePackage.get_Version)), Expression.Field(Expression.Constant(class_2, typeof(<>c__DisplayClass39_1)), fieldof(<>c__DisplayClass39_1.versionString)))), parameters)).ToArray<DataServicePackage>();
                            if (packageArray.Length != 0)
                            {
                                package = packageArray[0];
                                break;
                            }
                        }
                        catch (DataServiceQueryException)
                        {
                        }
                        continue;
                    }
                    return null;
                }
            }
            return package;
        }

        public IEnumerable<IPackage> FindPackagesById(string packageId)
        {
            IEnumerable<IPackage> enumerable;
            try
            {
                if (!this.Context.SupportsServiceMethod("FindPackagesById"))
                {
                    enumerable = PackageRepositoryExtensions.FindPackagesByIdCore(this, packageId);
                }
                else
                {
                    Dictionary<string, object> dictionary1 = new Dictionary<string, object>();
                    dictionary1.Add("id", "'" + UrlEncodeOdataParameter(packageId) + "'");
                    Dictionary<string, object> queryOptions = dictionary1;
                    IDataServiceQuery<DataServicePackage> query = this.Context.CreateQuery<DataServicePackage>("FindPackagesById", queryOptions);
                    enumerable = (IEnumerable<IPackage>) new SmartDataServiceQuery<DataServicePackage>(this.Context, query);
                }
            }
            catch (Exception exception)
            {
                object[] args = new object[] { this._httpClient.OriginalUri, exception.Message };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.ErrorLoadingPackages, args), exception);
            }
            return enumerable;
        }

        public override IQueryable<IPackage> GetPackages() => 
            ((IQueryable<IPackage>) new SmartDataServiceQuery<DataServicePackage>(this.Context, "Packages"));

        public IEnumerable<IPackage> GetUpdates(IEnumerable<IPackageName> packages, bool includePrerelease, bool includeAllVersions, IEnumerable<FrameworkName> targetFrameworks, IEnumerable<IVersionSpec> versionConstraints)
        {
            if (!this.Context.SupportsServiceMethod("GetUpdates"))
            {
                return this.GetUpdatesCore(packages, includePrerelease, includeAllVersions, targetFrameworks, versionConstraints);
            }
            string str = string.Join("|", (IEnumerable<string>) (from p in packages select p.Id));
            string str2 = string.Join("|", (IEnumerable<string>) (from p in packages select p.Version.ToString()));
            string str3 = targetFrameworks.IsEmpty<FrameworkName>() ? "" : string.Join("|", Enumerable.Select<FrameworkName, string>(targetFrameworks, new Func<FrameworkName, string>(VersionUtility.GetShortFrameworkName)));
            string str4 = versionConstraints.IsEmpty<IVersionSpec>() ? "" : string.Join("|", (IEnumerable<string>) (from v in versionConstraints select (v == null) ? ((IEnumerable<string>) "") : ((IEnumerable<string>) v.ToString())));
            Dictionary<string, object> dictionary1 = new Dictionary<string, object>();
            dictionary1.Add("packageIds", "'" + str + "'");
            dictionary1.Add("versions", "'" + str2 + "'");
            dictionary1.Add("includePrerelease", ToLowerCaseString(includePrerelease));
            dictionary1.Add("includeAllVersions", ToLowerCaseString(includeAllVersions));
            dictionary1.Add("targetFrameworks", "'" + UrlEncodeOdataParameter(str3) + "'");
            dictionary1.Add("versionConstraints", "'" + UrlEncodeOdataParameter(str4) + "'");
            Dictionary<string, object> queryOptions = dictionary1;
            IDataServiceQuery<DataServicePackage> query = this.Context.CreateQuery<DataServicePackage>("GetUpdates", queryOptions);
            return (IEnumerable<IPackage>) new SmartDataServiceQuery<DataServicePackage>(this.Context, query);
        }

        private void OnPackageDownloaderSendingRequest(object sender, WebRequestEventArgs e)
        {
            if (this._currentOperation != null)
            {
                string str = this._currentOperation.Item1;
                string str2 = this._currentOperation.Item2;
                string str3 = this._currentOperation.Item3;
                if (!string.IsNullOrEmpty(str2) && (!string.IsNullOrEmpty(this._packageDownloader.CurrentDownloadPackageId) && !str2.Equals(this._packageDownloader.CurrentDownloadPackageId, StringComparison.OrdinalIgnoreCase)))
                {
                    str = str + "-Dependency";
                }
                if (!string.IsNullOrEmpty(this._packageDownloader.CurrentDownloadPackageId) && !string.IsNullOrEmpty(this._packageDownloader.CurrentDownloadPackageVersion))
                {
                    e.Request.Headers[RepositoryOperationNames.PackageId] = this._packageDownloader.CurrentDownloadPackageId;
                    e.Request.Headers[RepositoryOperationNames.PackageVersion] = this._packageDownloader.CurrentDownloadPackageVersion;
                }
                e.Request.Headers[RepositoryOperationNames.OperationHeaderName] = str;
                if (!str.Equals(this._currentOperation.Item1, StringComparison.OrdinalIgnoreCase))
                {
                    e.Request.Headers[RepositoryOperationNames.DependentPackageHeaderName] = str2;
                    if (!string.IsNullOrEmpty(str3))
                    {
                        e.Request.Headers[RepositoryOperationNames.DependentPackageVersionHeaderName] = str3;
                    }
                }
                this.RaiseSendingRequest(e);
            }
        }

        private void OnReadingEntity(object sender, ReadingWritingEntityEventArgs e)
        {
            string uriString = e.get_Data().Element(e.get_Data().Name.Namespace.GetName("content")).Attribute(XName.Get("src")).Value;
            DataServicePackage package1 = (DataServicePackage) e.get_Entity();
            package1.DownloadUrl = new Uri(uriString);
            package1.Downloader = this._packageDownloader;
        }

        private void OnSendingRequest(object sender, SendingRequest2EventArgs e)
        {
            ShimDataRequestMessage message = new ShimDataRequestMessage(e);
            this._httpClient.InitializeRequest(message.WebRequest);
            this.RaiseSendingRequest(new WebRequestEventArgs(message.WebRequest));
        }

        private void RaiseSendingRequest(WebRequestEventArgs e)
        {
            if (this._sendingRequest != null)
            {
                this._sendingRequest(this, e);
            }
        }

        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (!(managerType == typeof(SendingRequestEventManager)))
            {
                return false;
            }
            this.OnPackageDownloaderSendingRequest(sender, (WebRequestEventArgs) e);
            return true;
        }

        public IQueryable<IPackage> Search(string searchTerm, IEnumerable<string> targetFrameworks, bool allowPrereleaseVersions, bool includeDelisted)
        {
            if (!this.Context.SupportsServiceMethod("Search"))
            {
                IEnumerable<IPackage> source = this.GetPackages().Find<IPackage>(searchTerm).FilterByPrerelease(allowPrereleaseVersions);
                if (!includeDelisted)
                {
                    source = from p in source
                        where p.IsListed()
                        select p;
                }
                return source.AsQueryable<IPackage>();
            }
            string str = string.Join("|", targetFrameworks);
            Dictionary<string, object> dictionary1 = new Dictionary<string, object>();
            dictionary1.Add("searchTerm", "'" + UrlEncodeOdataParameter(searchTerm) + "'");
            dictionary1.Add("targetFramework", "'" + UrlEncodeOdataParameter(str) + "'");
            Dictionary<string, object> queryOptions = dictionary1;
            if (this.SupportsPrereleasePackages)
            {
                queryOptions.Add("includePrerelease", ToLowerCaseString(allowPrereleaseVersions));
            }
            if (includeDelisted)
            {
                queryOptions.Add("includeDelisted", "true");
            }
            IDataServiceQuery<DataServicePackage> query = this.Context.CreateQuery<DataServicePackage>("Search", queryOptions);
            return (IQueryable<IPackage>) new SmartDataServiceQuery<DataServicePackage>(this.Context, query);
        }

        public IDisposable StartOperation(string operation, string mainPackageId, string mainPackageVersion)
        {
            Tuple<string, string, string> oldOperation = this._currentOperation;
            this._currentOperation = Tuple.Create<string, string, string>(operation, mainPackageId, mainPackageVersion);
            return new DisposableAction(delegate {
                this._currentOperation = oldOperation;
            });
        }

        private static string ToLowerCaseString(bool value) => 
            value.ToString().ToLowerInvariant();

        public bool TryFindLatestPackageById(string id, out SemanticVersion latestVersion)
        {
            latestVersion = null;
            try
            {
                Dictionary<string, object> dictionary1 = new Dictionary<string, object>();
                dictionary1.Add("id", "'" + UrlEncodeOdataParameter(id) + "'");
                Dictionary<string, object> queryOptions = dictionary1;
                ParameterExpression expression = Expression.Parameter(typeof(DataServicePackage), "p");
                ParameterExpression[] parameters = new ParameterExpression[] { expression };
                IQueryable<DataServicePackage> queryable1 = Queryable.Where<DataServicePackage>(this.Context.CreateQuery<DataServicePackage>("FindPackagesById", queryOptions).AsQueryable(), Expression.Lambda<Func<DataServicePackage, bool>>(Expression.Property(expression, (MethodInfo) methodof(DataServicePackage.get_IsLatestVersion)), parameters));
                expression = Expression.Parameter(typeof(DataServicePackage), "p");
                Expression[] arguments = new Expression[] { Expression.Property(expression, (MethodInfo) methodof(DataServicePackage.get_Id)), Expression.Property(expression, (MethodInfo) methodof(DataServicePackage.get_Version)) };
                MemberInfo[] members = new MemberInfo[] { (MethodInfo) methodof(<>f__AnonymousType20<string, string>.get_Id, <>f__AnonymousType20<string, string>), (MethodInfo) methodof(<>f__AnonymousType20<string, string>.get_Version, <>f__AnonymousType20<string, string>) };
                ParameterExpression[] expressionArray3 = new ParameterExpression[] { expression };
                var type = Queryable.Select(queryable1, Expression.Lambda(Expression.New((ConstructorInfo) methodof(<>f__AnonymousType20<string, string>..ctor, <>f__AnonymousType20<string, string>), arguments, members), expressionArray3)).FirstOrDefault();
                if (type != null)
                {
                    latestVersion = new SemanticVersion(type.Version);
                    return true;
                }
            }
            catch (DataServiceQueryException)
            {
            }
            return false;
        }

        public bool TryFindLatestPackageById(string id, bool includePrerelease, out IPackage package)
        {
            try
            {
                ParameterExpression expression;
                Dictionary<string, object> dictionary1 = new Dictionary<string, object>();
                dictionary1.Add("id", "'" + UrlEncodeOdataParameter(id) + "'");
                Dictionary<string, object> queryOptions = dictionary1;
                IQueryable<DataServicePackage> queryable = this.Context.CreateQuery<DataServicePackage>("FindPackagesById", queryOptions).AsQueryable();
                if (includePrerelease)
                {
                    expression = Expression.Parameter(typeof(DataServicePackage), "p");
                    ParameterExpression[] parameters = new ParameterExpression[] { expression };
                    IQueryable<DataServicePackage> queryable1 = Queryable.Where<DataServicePackage>(queryable, Expression.Lambda<Func<DataServicePackage, bool>>(Expression.Property(expression, (MethodInfo) methodof(DataServicePackage.get_IsAbsoluteLatestVersion)), parameters));
                    expression = Expression.Parameter(typeof(DataServicePackage), "p");
                    ParameterExpression[] expressionArray2 = new ParameterExpression[] { expression };
                    package = Queryable.OrderByDescending<DataServicePackage, string>(queryable1, Expression.Lambda<Func<DataServicePackage, string>>(Expression.Property(expression, (MethodInfo) methodof(DataServicePackage.get_Version)), expressionArray2)).FirstOrDefault<DataServicePackage>();
                }
                else
                {
                    expression = Expression.Parameter(typeof(DataServicePackage), "p");
                    ParameterExpression[] parameters = new ParameterExpression[] { expression };
                    IQueryable<DataServicePackage> queryable2 = Queryable.Where<DataServicePackage>(queryable, Expression.Lambda<Func<DataServicePackage, bool>>(Expression.Property(expression, (MethodInfo) methodof(DataServicePackage.get_IsLatestVersion)), parameters));
                    expression = Expression.Parameter(typeof(DataServicePackage), "p");
                    ParameterExpression[] expressionArray4 = new ParameterExpression[] { expression };
                    package = Queryable.OrderByDescending<DataServicePackage, string>(queryable2, Expression.Lambda<Func<DataServicePackage, string>>(Expression.Property(expression, (MethodInfo) methodof(DataServicePackage.get_Version)), expressionArray4)).FirstOrDefault<DataServicePackage>();
                }
                return (package != null);
            }
            catch (DataServiceQueryException)
            {
                package = null;
                return false;
            }
        }

        private static string UrlEncodeOdataParameter(string value) => 
            (string.IsNullOrEmpty(value) ? value : Uri.EscapeDataString(value).Replace("'", "''").Replace("%27", "''"));

        public CultureInfo Culture
        {
            get
            {
                if (this._culture == null)
                {
                    this._culture = this._httpClient.Uri.IsLoopback ? CultureInfo.CurrentCulture : CultureInfo.InvariantCulture;
                }
                return this._culture;
            }
        }

        public NuGet.PackageDownloader PackageDownloader =>
            this._packageDownloader;

        public override string Source =>
            this._httpClient.Uri.OriginalString;

        public override bool SupportsPrereleasePackages =>
            this.Context.SupportsProperty("IsAbsoluteLatestVersion");

        internal IDataServiceContext Context
        {
            private get
            {
                if (this._context == null)
                {
                    this._context = new DataServiceContextWrapper(this._httpClient.Uri);
                    this._context.SendingRequest += new EventHandler<SendingRequest2EventArgs>(this.OnSendingRequest);
                    this._context.ReadingEntity += new EventHandler<ReadingWritingEntityEventArgs>(this.OnReadingEntity);
                    this._context.IgnoreMissingProperties = true;
                }
                return this._context;
            }
            set => 
                (this._context = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly DataServicePackageRepository.<>c <>9 = new DataServicePackageRepository.<>c();
            public static Func<IPackage, bool> <>9__37_0;
            public static Func<IPackageName, string> <>9__41_0;
            public static Func<IPackageName, string> <>9__41_1;
            public static Func<IVersionSpec, string> <>9__41_2;

            internal string <GetUpdates>b__41_0(IPackageName p) => 
                p.Id;

            internal string <GetUpdates>b__41_1(IPackageName p) => 
                p.Version.ToString();

            internal string <GetUpdates>b__41_2(IVersionSpec v) => 
                ((v == null) ? "" : v.ToString());

            internal bool <Search>b__37_0(IPackage p) => 
                p.IsListed();
        }
    }
}

