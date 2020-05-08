namespace Squirrel.Json.Reflection
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [GeneratedCode("reflection-utils", "1.0.0")]
    internal class ReflectionUtils
    {
        private static readonly object[] EmptyObjects = new object[0];

        public static BinaryExpression Assign(Expression left, Expression right)
        {
            Type[] typeArguments = new Type[] { left.Type };
            MethodInfo method = typeof(Assigner).MakeGenericType(typeArguments).GetMethod("Assign");
            return Expression.Add(left, right, method);
        }

        public static Attribute GetAttribute(MemberInfo info, Type type) => 
            (((info == null) || ((type == null) || !Attribute.IsDefined(info, type))) ? null : Attribute.GetCustomAttribute(info, type));

        public static Attribute GetAttribute(Type objectType, Type attributeType) => 
            (((objectType == null) || ((attributeType == null) || !Attribute.IsDefined(objectType, attributeType))) ? null : Attribute.GetCustomAttribute(objectType, attributeType));

        public static ConstructorDelegate GetConstructorByExpression(ConstructorInfo constructorInfo)
        {
            ParameterInfo[] parameters = constructorInfo.GetParameters();
            ParameterExpression array = Expression.Parameter(typeof(object[]), "args");
            Expression[] arguments = new Expression[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                Expression index = Expression.Constant(i);
                Type parameterType = parameters[i].ParameterType;
                arguments[i] = Expression.Convert(Expression.ArrayIndex(array, index), parameterType);
            }
            ParameterExpression[] expressionArray1 = new ParameterExpression[] { array };
            Func<object[], object> compiledLambda = Expression.Lambda<Func<object[], object>>(Expression.New(constructorInfo, arguments), expressionArray1).Compile();
            return args => compiledLambda(args);
        }

        public static ConstructorDelegate GetConstructorByExpression(Type type, params Type[] argsType)
        {
            ConstructorInfo constructorInfo = GetConstructorInfo(type, argsType);
            return ((constructorInfo == null) ? null : GetConstructorByExpression(constructorInfo));
        }

        public static ConstructorDelegate GetConstructorByReflection(ConstructorInfo constructorInfo) => 
            args => constructorInfo.Invoke(args);

        public static ConstructorDelegate GetConstructorByReflection(Type type, params Type[] argsType)
        {
            ConstructorInfo constructorInfo = GetConstructorInfo(type, argsType);
            return ((constructorInfo == null) ? null : GetConstructorByReflection(constructorInfo));
        }

        public static ConstructorInfo GetConstructorInfo(Type type, params Type[] argsType)
        {
            ConstructorInfo info2;
            using (IEnumerator<ConstructorInfo> enumerator = GetConstructors(type).GetEnumerator())
            {
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        ConstructorInfo current = enumerator.Current;
                        ParameterInfo[] parameters = current.GetParameters();
                        if (argsType.Length != parameters.Length)
                        {
                            continue;
                        }
                        int index = 0;
                        bool flag = true;
                        ParameterInfo[] infoArray2 = current.GetParameters();
                        int num2 = 0;
                        while (true)
                        {
                            if (num2 < infoArray2.Length)
                            {
                                if (!(infoArray2[num2].ParameterType != argsType[index]))
                                {
                                    num2++;
                                    continue;
                                }
                                flag = false;
                            }
                            if (!flag)
                            {
                                break;
                            }
                            return current;
                        }
                        continue;
                    }
                    else
                    {
                        return null;
                    }
                    break;
                }
            }
            return info2;
        }

        public static IEnumerable<ConstructorInfo> GetConstructors(Type type) => 
            type.GetConstructors();

        public static ConstructorDelegate GetContructor(ConstructorInfo constructorInfo) => 
            GetConstructorByExpression(constructorInfo);

        public static ConstructorDelegate GetContructor(Type type, params Type[] argsType) => 
            GetConstructorByExpression(type, argsType);

        public static IEnumerable<FieldInfo> GetFields(Type type) => 
            type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

        public static Type GetGenericListElementType(Type type)
        {
            Type type3;
            using (IEnumerator<Type> enumerator = type.GetInterfaces().GetEnumerator())
            {
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        Type current = enumerator.Current;
                        if (!IsTypeGeneric(current) || !(current.GetGenericTypeDefinition() == typeof(IList<>)))
                        {
                            continue;
                        }
                        type3 = GetGenericTypeArguments(current)[0];
                    }
                    else
                    {
                        return GetGenericTypeArguments(type)[0];
                    }
                    break;
                }
            }
            return type3;
        }

        public static Type[] GetGenericTypeArguments(Type type) => 
            type.GetGenericArguments();

        public static GetDelegate GetGetMethod(FieldInfo fieldInfo) => 
            GetGetMethodByExpression(fieldInfo);

        public static GetDelegate GetGetMethod(PropertyInfo propertyInfo) => 
            GetGetMethodByExpression(propertyInfo);

        public static GetDelegate GetGetMethodByExpression(FieldInfo fieldInfo)
        {
            ParameterExpression expression = Expression.Parameter(typeof(object), "instance");
            ParameterExpression[] parameters = new ParameterExpression[] { expression };
            GetDelegate compiled = Expression.Lambda<GetDelegate>(Expression.Convert(Expression.Field(Expression.Convert(expression, fieldInfo.DeclaringType), fieldInfo), typeof(object)), parameters).Compile();
            return source => compiled(source);
        }

        public static GetDelegate GetGetMethodByExpression(PropertyInfo propertyInfo)
        {
            MethodInfo getterMethodInfo = GetGetterMethodInfo(propertyInfo);
            ParameterExpression expression = Expression.Parameter(typeof(object), "instance");
            ParameterExpression[] parameters = new ParameterExpression[] { expression };
            Func<object, object> compiled = Expression.Lambda<Func<object, object>>(Expression.TypeAs(Expression.Call(!IsValueType(propertyInfo.DeclaringType) ? Expression.TypeAs(expression, propertyInfo.DeclaringType) : Expression.Convert(expression, propertyInfo.DeclaringType), getterMethodInfo), typeof(object)), parameters).Compile();
            return source => compiled(source);
        }

        public static GetDelegate GetGetMethodByReflection(FieldInfo fieldInfo) => 
            source => fieldInfo.GetValue(source);

        public static GetDelegate GetGetMethodByReflection(PropertyInfo propertyInfo)
        {
            MethodInfo methodInfo = GetGetterMethodInfo(propertyInfo);
            return source => methodInfo.Invoke(source, EmptyObjects);
        }

        public static MethodInfo GetGetterMethodInfo(PropertyInfo propertyInfo) => 
            propertyInfo.GetGetMethod(true);

        public static IEnumerable<PropertyInfo> GetProperties(Type type) => 
            type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

        public static SetDelegate GetSetMethod(FieldInfo fieldInfo) => 
            GetSetMethodByExpression(fieldInfo);

        public static SetDelegate GetSetMethod(PropertyInfo propertyInfo) => 
            GetSetMethodByExpression(propertyInfo);

        public static SetDelegate GetSetMethodByExpression(FieldInfo fieldInfo)
        {
            ParameterExpression expression = Expression.Parameter(typeof(object), "instance");
            ParameterExpression expression2 = Expression.Parameter(typeof(object), "value");
            ParameterExpression[] parameters = new ParameterExpression[] { expression, expression2 };
            Action<object, object> compiled = Expression.Lambda<Action<object, object>>(Assign(Expression.Field(Expression.Convert(expression, fieldInfo.DeclaringType), fieldInfo), Expression.Convert(expression2, fieldInfo.FieldType)), parameters).Compile();
            return delegate (object source, object val) {
                compiled(source, val);
            };
        }

        public static SetDelegate GetSetMethodByExpression(PropertyInfo propertyInfo)
        {
            MethodInfo setterMethodInfo = GetSetterMethodInfo(propertyInfo);
            ParameterExpression expression = Expression.Parameter(typeof(object), "instance");
            ParameterExpression expression2 = Expression.Parameter(typeof(object), "value");
            UnaryExpression instance = !IsValueType(propertyInfo.DeclaringType) ? Expression.TypeAs(expression, propertyInfo.DeclaringType) : Expression.Convert(expression, propertyInfo.DeclaringType);
            UnaryExpression expression4 = !IsValueType(propertyInfo.PropertyType) ? Expression.TypeAs(expression2, propertyInfo.PropertyType) : Expression.Convert(expression2, propertyInfo.PropertyType);
            Expression[] arguments = new Expression[] { expression4 };
            ParameterExpression[] parameters = new ParameterExpression[] { expression, expression2 };
            Action<object, object> compiled = Expression.Lambda<Action<object, object>>(Expression.Call(instance, setterMethodInfo, arguments), parameters).Compile();
            return delegate (object source, object val) {
                compiled(source, val);
            };
        }

        public static SetDelegate GetSetMethodByReflection(FieldInfo fieldInfo) => 
            delegate (object source, object value) {
                fieldInfo.SetValue(source, value);
            };

        public static SetDelegate GetSetMethodByReflection(PropertyInfo propertyInfo)
        {
            MethodInfo methodInfo = GetSetterMethodInfo(propertyInfo);
            return delegate (object source, object value) {
                object[] parameters = new object[] { value };
                methodInfo.Invoke(source, parameters);
            };
        }

        public static MethodInfo GetSetterMethodInfo(PropertyInfo propertyInfo) => 
            propertyInfo.GetSetMethod(true);

        public static Type GetTypeInfo(Type type) => 
            type;

        public static bool IsAssignableFrom(Type type1, Type type2) => 
            GetTypeInfo(type1).IsAssignableFrom(GetTypeInfo(type2));

        public static bool IsNullableType(Type type) => 
            (GetTypeInfo(type).IsGenericType && (type.GetGenericTypeDefinition() == typeof(Nullable<>)));

        public static bool IsTypeDictionary(Type type) => 
            (!typeof(IDictionary).IsAssignableFrom(type) ? (GetTypeInfo(type).IsGenericType ? (type.GetGenericTypeDefinition() == typeof(IDictionary<,>)) : false) : true);

        public static bool IsTypeGeneric(Type type) => 
            GetTypeInfo(type).IsGenericType;

        public static bool IsTypeGenericeCollectionInterface(Type type)
        {
            if (!IsTypeGeneric(type))
            {
                return false;
            }
            Type genericTypeDefinition = type.GetGenericTypeDefinition();
            return ((genericTypeDefinition == typeof(IList<>)) || ((genericTypeDefinition == typeof(ICollection<>)) || (genericTypeDefinition == typeof(IEnumerable<>))));
        }

        public static bool IsValueType(Type type) => 
            GetTypeInfo(type).IsValueType;

        public static object ToNullableType(object obj, Type nullableType) => 
            ((obj == null) ? null : Convert.ChangeType(obj, Nullable.GetUnderlyingType(nullableType), CultureInfo.InvariantCulture));

        private static class Assigner<T>
        {
            public static T Assign(ref T left, T right)
            {
                T local;
                left = local = right;
                return local;
            }
        }

        public delegate object ConstructorDelegate(params object[] args);

        public delegate object GetDelegate(object source);

        public delegate void SetDelegate(object source, object value);

        public sealed class ThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
        {
            private readonly object _lock;
            private readonly ReflectionUtils.ThreadSafeDictionaryValueFactory<TKey, TValue> _valueFactory;
            private Dictionary<TKey, TValue> _dictionary;

            public ThreadSafeDictionary(ReflectionUtils.ThreadSafeDictionaryValueFactory<TKey, TValue> valueFactory)
            {
                this._lock = new object();
                this._valueFactory = valueFactory;
            }

            public void Add(KeyValuePair<TKey, TValue> item)
            {
                throw new NotImplementedException();
            }

            public void Add(TKey key, TValue value)
            {
                throw new NotImplementedException();
            }

            private TValue AddValue(TKey key)
            {
                TValue local3;
                TValue local = this._valueFactory(key);
                object obj2 = this._lock;
                lock (obj2)
                {
                    if (this._dictionary != null)
                    {
                        TValue local2;
                        if (!this._dictionary.TryGetValue(key, out local2))
                        {
                            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(this._dictionary) {
                                [key] = local
                            };
                            this._dictionary = dictionary;
                        }
                        else
                        {
                            return local2;
                        }
                    }
                    else
                    {
                        this._dictionary = new Dictionary<TKey, TValue>();
                        this._dictionary[key] = local;
                    }
                    return local;
                }
                return local3;
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(KeyValuePair<TKey, TValue> item)
            {
                throw new NotImplementedException();
            }

            public bool ContainsKey(TKey key) => 
                this._dictionary.ContainsKey(key);

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            private TValue Get(TKey key)
            {
                TValue local;
                return ((this._dictionary != null) ? (this._dictionary.TryGetValue(key, out local) ? local : this.AddValue(key)) : this.AddValue(key));
            }

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => 
                this._dictionary.GetEnumerator();

            public bool Remove(TKey key)
            {
                throw new NotImplementedException();
            }

            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator() => 
                this._dictionary.GetEnumerator();

            public bool TryGetValue(TKey key, out TValue value)
            {
                value = this[key];
                return true;
            }

            public ICollection<TKey> Keys =>
                this._dictionary.Keys;

            public ICollection<TValue> Values =>
                this._dictionary.Values;

            public TValue this[TKey key]
            {
                get => 
                    this.Get(key);
                set
                {
                    throw new NotImplementedException();
                }
            }

            public int Count =>
                this._dictionary.Count;

            public bool IsReadOnly
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public delegate TValue ThreadSafeDictionaryValueFactory<TKey, TValue>(TKey key);
    }
}

