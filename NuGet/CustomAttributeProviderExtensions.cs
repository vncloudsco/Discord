namespace NuGet
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal static class CustomAttributeProviderExtensions
    {
        public static T GetCustomAttribute<T>(this ICustomAttributeProvider attributeProvider) => 
            ((T) attributeProvider.GetCustomAttribute(typeof(T)));

        public static object GetCustomAttribute(this ICustomAttributeProvider attributeProvider, Type type) => 
            attributeProvider.GetCustomAttributes(type, false).FirstOrDefault<object>();
    }
}

