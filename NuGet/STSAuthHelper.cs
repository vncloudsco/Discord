namespace NuGet
{
    using Microsoft.CSharp.RuntimeBinder;
    using NuGet.Resources;
    using System;
    using System.Globalization;
    using System.Net;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.Text;

    internal static class STSAuthHelper
    {
        private const string STSEndPointHeader = "X-NuGet-STS-EndPoint";
        private const string STSRealmHeader = "X-NuGet-STS-Realm";
        private const string STSTokenHeader = "X-NuGet-STS-Token";

        private static string EncodeHeader(string token) => 
            Convert.ToBase64String(Encoding.UTF8.GetBytes(token));

        private static string GetCacheKey(Uri requestUri) => 
            ("X-NuGet-STS-Token|" + requestUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped));

        private static TVal GetFieldValue<TVal>(Type type, string fieldName) => 
            ((TVal) type.GetField(fieldName).GetValue(null));

        private static string GetSTSEndPoint(IHttpWebResponse response) => 
            response.Headers["X-NuGet-STS-EndPoint"].SafeTrim();

        private static string GetSTSToken(Uri requestUri, string endPoint, string appliesTo)
        {
            WIFTypeProvider wIFTypes = WIFTypeProvider.GetWIFTypes();
            if (wIFTypes == null)
            {
                object[] objArray1 = new object[] { requestUri };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.UnableToLocateWIF, objArray1));
            }
            WS2007HttpBinding binding = new WS2007HttpBinding(SecurityMode.Transport);
            object[] args = new object[] { binding, endPoint };
            object obj2 = Activator.CreateInstance(wIFTypes.ChannelFactory, args);
            if (<>o__5.<>p__0 == null)
            {
                CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null) };
                <>o__5.<>p__0 = CallSite<Func<CallSite, object, TrustVersion, object>>.Create(Binder.SetMember(CSharpBinderFlags.None, "TrustVersion", typeof(STSAuthHelper), argumentInfo));
            }
            <>o__5.<>p__0.Target(<>o__5.<>p__0, obj2, TrustVersion.WSTrust13);
            object obj3 = Activator.CreateInstance(wIFTypes.RequestSecurityToken);
            if (<>o__5.<>p__1 == null)
            {
                CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null) };
                <>o__5.<>p__1 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.SetMember(CSharpBinderFlags.None, "RequestType", typeof(STSAuthHelper), argumentInfo));
            }
            <>o__5.<>p__1.Target(<>o__5.<>p__1, obj3, GetFieldValue<string>(wIFTypes.RequestTypes, "Issue"));
            if (<>o__5.<>p__2 == null)
            {
                CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null) };
                <>o__5.<>p__2 = CallSite<Func<CallSite, object, string, object>>.Create(Binder.SetMember(CSharpBinderFlags.None, "KeyType", typeof(STSAuthHelper), argumentInfo));
            }
            <>o__5.<>p__2.Target(<>o__5.<>p__2, obj3, GetFieldValue<string>(wIFTypes.KeyTypes, "Bearer"));
            object[] objArray3 = new object[] { appliesTo };
            object obj4 = Activator.CreateInstance(wIFTypes.EndPoint, objArray3);
            if (<>o__5.<>p__3 == null)
            {
                CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.IsStaticType | CSharpArgumentInfoFlags.UseCompileTimeType, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.Constant | CSharpArgumentInfoFlags.UseCompileTimeType, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType, null) };
                <>o__5.<>p__3 = CallSite<Action<CallSite, Type, object, string, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.ResultDiscarded, "SetProperty", null, typeof(STSAuthHelper), argumentInfo));
            }
            <>o__5.<>p__3.Target(<>o__5.<>p__3, typeof(STSAuthHelper), obj3, "AppliesTo", obj4);
            if (<>o__5.<>p__4 == null)
            {
                CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) };
                <>o__5.<>p__4 = CallSite<Func<CallSite, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "CreateChannel", null, typeof(STSAuthHelper), argumentInfo));
            }
            object obj5 = <>o__5.<>p__4.Target(<>o__5.<>p__4, obj2);
            if (<>o__5.<>p__5 == null)
            {
                CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) };
                <>o__5.<>p__5 = CallSite<Func<CallSite, object, object, object>>.Create(Binder.InvokeMember(CSharpBinderFlags.None, "Issue", null, typeof(STSAuthHelper), argumentInfo));
            }
            object obj6 = <>o__5.<>p__5.Target(<>o__5.<>p__5, obj5, obj3);
            if (<>o__5.<>p__8 == null)
            {
                <>o__5.<>p__8 = CallSite<Func<CallSite, object, string>>.Create(Binder.Convert(CSharpBinderFlags.None, typeof(string), typeof(STSAuthHelper)));
            }
            if (<>o__5.<>p__7 == null)
            {
                CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) };
                <>o__5.<>p__7 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "OuterXml", typeof(STSAuthHelper), argumentInfo));
            }
            if (<>o__5.<>p__6 == null)
            {
                CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) };
                <>o__5.<>p__6 = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "TokenXml", typeof(STSAuthHelper), argumentInfo));
            }
            return <>o__5.<>p__8.Target(<>o__5.<>p__8, <>o__5.<>p__7.Target(<>o__5.<>p__7, <>o__5.<>p__6.Target(<>o__5.<>p__6, obj6)));
        }

        public static void PrepareSTSRequest(WebRequest request)
        {
            string str2;
            string cacheKey = GetCacheKey(request.RequestUri);
            if (MemoryCache.Instance.TryGetValue<string>(cacheKey, out str2))
            {
                request.Headers["X-NuGet-STS-Token"] = EncodeHeader(str2);
            }
        }

        private static void SetProperty(object instance, string propertyName, object value)
        {
            object[] parameters = new object[] { value };
            instance.GetType().GetProperty(propertyName).GetSetMethod().Invoke(instance, parameters);
        }

        public static bool TryRetrieveSTSToken(Uri requestUri, IHttpWebResponse response)
        {
            if (response.StatusCode != HttpStatusCode.Unauthorized)
            {
                return false;
            }
            string endPoint = GetSTSEndPoint(response);
            string realm = response.Headers["X-NuGet-STS-Realm"];
            if (string.IsNullOrEmpty(endPoint) || string.IsNullOrEmpty(realm))
            {
                return false;
            }
            string cacheKey = GetCacheKey(requestUri);
            MemoryCache.Instance.GetOrAdd<string>(cacheKey, () => GetSTSToken(requestUri, endPoint, realm), TimeSpan.FromMinutes(30.0), true);
            return true;
        }

        [CompilerGenerated]
        private static class <>o__5
        {
            public static CallSite<Func<CallSite, object, TrustVersion, object>> <>p__0;
            public static CallSite<Func<CallSite, object, string, object>> <>p__1;
            public static CallSite<Func<CallSite, object, string, object>> <>p__2;
            public static CallSite<Action<CallSite, Type, object, string, object>> <>p__3;
            public static CallSite<Func<CallSite, object, object>> <>p__4;
            public static CallSite<Func<CallSite, object, object, object>> <>p__5;
            public static CallSite<Func<CallSite, object, object>> <>p__6;
            public static CallSite<Func<CallSite, object, object>> <>p__7;
            public static CallSite<Func<CallSite, object, string>> <>p__8;
        }
    }
}

