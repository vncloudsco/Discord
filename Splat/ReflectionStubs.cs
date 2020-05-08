namespace Splat
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static class ReflectionStubs
    {
        public static IEnumerable<object> GetCustomAttributes(this Type This, Type attributeType, bool inherit) => 
            This.GetTypeInfo().GetCustomAttributes(attributeType, inherit);

        public static EventInfo GetEvent(this Type This, string name, BindingFlags flags = 0)
        {
            TypeInfo typeInfo = This.GetTypeInfo();
            EventInfo declaredEvent = typeInfo.GetDeclaredEvent(name);
            return (((declaredEvent != null) || (!flags.HasFlag(BindingFlags.FlattenHierarchy) || (typeInfo.BaseType == null))) ? declaredEvent : typeInfo.BaseType.GetEvent(name, flags));
        }

        public static FieldInfo GetField(this Type This, string name, BindingFlags flags = 0)
        {
            TypeInfo typeInfo = This.GetTypeInfo();
            FieldInfo declaredField = typeInfo.GetDeclaredField(name);
            return (((declaredField != null) || (!flags.HasFlag(BindingFlags.FlattenHierarchy) || (typeInfo.BaseType == null))) ? declaredField : typeInfo.BaseType.GetField(name, flags));
        }

        public static IEnumerable<FieldInfo> GetFields(this Type This, BindingFlags flags = 0) => 
            This.GetTypeInfo().DeclaredFields;

        public static MethodInfo GetMethod(this Type This, string name, BindingFlags flags = 0)
        {
            TypeInfo typeInfo = This.GetTypeInfo();
            MethodInfo declaredMethod = typeInfo.GetDeclaredMethod(name);
            return (((declaredMethod != null) || (!flags.HasFlag(BindingFlags.FlattenHierarchy) || (typeInfo.BaseType == null))) ? declaredMethod : typeInfo.BaseType.GetMethod(name, flags));
        }

        public static MethodInfo GetMethod(this Type This, string methodName, Type[] paramTypes, BindingFlags flags = 0)
        {
            TypeInfo typeInfo = This.GetTypeInfo();
            MethodInfo info2 = Enumerable.FirstOrDefault<MethodInfo>(typeInfo.GetDeclaredMethods(methodName), x => Enumerable.All<bool>(Enumerable.Zip<Type, Type, bool>(paramTypes, from y in x.GetParameters() select y.ParameterType, (l, r) => l == r), y => y));
            return (((info2 != null) || (!flags.HasFlag(BindingFlags.FlattenHierarchy) || (typeInfo.BaseType == null))) ? info2 : typeInfo.BaseType.GetMethod(methodName, paramTypes, flags));
        }

        public static IEnumerable<MethodInfo> GetMethods(this Type This) => 
            This.GetTypeInfo().DeclaredMethods;

        public static IEnumerable<PropertyInfo> GetProperties(this Type This, BindingFlags flags = 0) => 
            This.GetTypeInfo().DeclaredProperties;

        public static PropertyInfo GetProperty(this Type This, string name, BindingFlags flags = 0)
        {
            TypeInfo typeInfo = This.GetTypeInfo();
            PropertyInfo declaredProperty = typeInfo.GetDeclaredProperty(name);
            return (((declaredProperty != null) || (!flags.HasFlag(BindingFlags.FlattenHierarchy) || (typeInfo.BaseType == null))) ? declaredProperty : typeInfo.BaseType.GetProperty(name, flags));
        }

        public static bool IsAssignableFrom(this Type This, Type anotherType) => 
            This.GetTypeInfo().IsAssignableFrom(anotherType.GetTypeInfo());
    }
}

