namespace Squirrel.Json
{
    using Squirrel.Json.Reflection;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [GeneratedCode("simple-json", "1.0.0")]
    internal class PocoJsonSerializerStrategy : IJsonSerializerStrategy
    {
        internal IDictionary<Type, ReflectionUtils.ConstructorDelegate> ConstructorCache;
        internal IDictionary<Type, IDictionary<string, ReflectionUtils.GetDelegate>> GetCache;
        internal IDictionary<Type, IDictionary<string, KeyValuePair<Type, ReflectionUtils.SetDelegate>>> SetCache;
        internal static readonly Type[] EmptyTypes = new Type[0];
        internal static readonly Type[] ArrayConstructorParameterTypes = new Type[] { typeof(int) };
        private static readonly string[] Iso8601Format = new string[] { @"yyyy-MM-dd\THH:mm:ss.FFFFFFF\Z", @"yyyy-MM-dd\THH:mm:ss\Z", @"yyyy-MM-dd\THH:mm:ssK" };

        public PocoJsonSerializerStrategy()
        {
            this.ConstructorCache = new ReflectionUtils.ThreadSafeDictionary<Type, ReflectionUtils.ConstructorDelegate>(new ReflectionUtils.ThreadSafeDictionaryValueFactory<Type, ReflectionUtils.ConstructorDelegate>(this.ContructorDelegateFactory));
            this.GetCache = new ReflectionUtils.ThreadSafeDictionary<Type, IDictionary<string, ReflectionUtils.GetDelegate>>(new ReflectionUtils.ThreadSafeDictionaryValueFactory<Type, IDictionary<string, ReflectionUtils.GetDelegate>>(this.GetterValueFactory));
            this.SetCache = new ReflectionUtils.ThreadSafeDictionary<Type, IDictionary<string, KeyValuePair<Type, ReflectionUtils.SetDelegate>>>(new ReflectionUtils.ThreadSafeDictionaryValueFactory<Type, IDictionary<string, KeyValuePair<Type, ReflectionUtils.SetDelegate>>>(this.SetterValueFactory));
        }

        internal virtual ReflectionUtils.ConstructorDelegate ContructorDelegateFactory(Type key) => 
            ReflectionUtils.GetContructor(key, key.IsArray ? ArrayConstructorParameterTypes : EmptyTypes);

        public virtual object DeserializeObject(object value, Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            string str = value as string;
            if ((type == typeof(Guid)) && string.IsNullOrEmpty(str))
            {
                return default(Guid);
            }
            if (value == null)
            {
                return null;
            }
            object obj2 = null;
            if (str == null)
            {
                if (value as bool)
                {
                    return value;
                }
            }
            else
            {
                if (str.Length != 0)
                {
                    Uri uri;
                    return (((type == typeof(DateTime)) || (ReflectionUtils.IsNullableType(type) && (Nullable.GetUnderlyingType(type) == typeof(DateTime)))) ? DateTime.ParseExact(str, Iso8601Format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal) : (((type == typeof(DateTimeOffset)) || (ReflectionUtils.IsNullableType(type) && (Nullable.GetUnderlyingType(type) == typeof(DateTimeOffset)))) ? ((object) DateTimeOffset.ParseExact(str, Iso8601Format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal)) : (((type == typeof(Guid)) || (ReflectionUtils.IsNullableType(type) && (Nullable.GetUnderlyingType(type) == typeof(Guid)))) ? ((object) new Guid(str)) : (!(type == typeof(Uri)) ? (!(type == typeof(string)) ? Convert.ChangeType(str, type, CultureInfo.InvariantCulture) : str) : ((!Uri.IsWellFormedUriString(str, UriKind.RelativeOrAbsolute) || !Uri.TryCreate(str, UriKind.RelativeOrAbsolute, out uri)) ? null : uri)))));
                }
                if (!(type == typeof(Guid)))
                {
                    obj2 = (!ReflectionUtils.IsNullableType(type) || !(Nullable.GetUnderlyingType(type) == typeof(Guid))) ? str : null;
                }
                else
                {
                    Guid guid = default(Guid);
                    obj2 = guid;
                }
                if (!ReflectionUtils.IsNullableType(type) && (Nullable.GetUnderlyingType(type) == typeof(Guid)))
                {
                    return str;
                }
            }
            bool flag = value is long;
            bool flag2 = value is double;
            if ((flag && (type == typeof(long))) || (flag2 && (type == typeof(double))))
            {
                return value;
            }
            if ((flag2 && (type != typeof(double))) || (flag && (type != typeof(long))))
            {
                obj2 = ((type == typeof(int)) || ((type == typeof(long)) || ((type == typeof(double)) || ((type == typeof(float)) || ((type == typeof(bool)) || ((type == typeof(decimal)) || ((type == typeof(byte)) || (type == typeof(short))))))))) ? Convert.ChangeType(value, type, CultureInfo.InvariantCulture) : value;
                return (!ReflectionUtils.IsNullableType(type) ? obj2 : ReflectionUtils.ToNullableType(obj2, type));
            }
            IDictionary<string, object> dictionary = value as IDictionary<string, object>;
            if (dictionary != null)
            {
                IDictionary<string, object> dictionary2 = dictionary;
                if (ReflectionUtils.IsTypeDictionary(type))
                {
                    Type[] genericTypeArguments = ReflectionUtils.GetGenericTypeArguments(type);
                    Type type2 = genericTypeArguments[0];
                    Type type3 = genericTypeArguments[1];
                    Type[] typeArguments = new Type[] { type2, type3 };
                    Type type4 = typeof(Dictionary<,>).MakeGenericType(typeArguments);
                    IDictionary dictionary3 = (IDictionary) this.ConstructorCache[type4](new object[0]);
                    foreach (KeyValuePair<string, object> pair in dictionary2)
                    {
                        dictionary3.Add(pair.Key, this.DeserializeObject(pair.Value, type3));
                    }
                    obj2 = dictionary3;
                }
                else if (type == typeof(object))
                {
                    obj2 = value;
                }
                else
                {
                    obj2 = this.ConstructorCache[type](new object[0]);
                    foreach (KeyValuePair<string, KeyValuePair<Type, ReflectionUtils.SetDelegate>> pair2 in this.SetCache[type])
                    {
                        object obj3;
                        if (dictionary2.TryGetValue(pair2.Key, out obj3))
                        {
                            obj3 = this.DeserializeObject(obj3, pair2.Value.Key);
                            pair2.Value.Value(obj2, obj3);
                        }
                    }
                }
            }
            else
            {
                IList<object> list = value as IList<object>;
                if (list != null)
                {
                    IList<object> list2 = list;
                    IList list3 = null;
                    if (type.IsArray)
                    {
                        object[] args = new object[] { list2.Count };
                        list3 = (IList) this.ConstructorCache[type](args);
                        int num = 0;
                        foreach (object obj4 in list2)
                        {
                            list3[num++] = this.DeserializeObject(obj4, type.GetElementType());
                        }
                    }
                    else if (ReflectionUtils.IsTypeGenericeCollectionInterface(type) || ReflectionUtils.IsAssignableFrom(typeof(IList), type))
                    {
                        Type genericListElementType = ReflectionUtils.GetGenericListElementType(type);
                        ReflectionUtils.ConstructorDelegate local1 = this.ConstructorCache[type];
                        ReflectionUtils.ConstructorDelegate local3 = local1;
                        if (local1 == null)
                        {
                            ReflectionUtils.ConstructorDelegate local2 = local1;
                            Type[] typeArguments = new Type[] { genericListElementType };
                            local3 = this.ConstructorCache[typeof(List<>).MakeGenericType(typeArguments)];
                        }
                        object[] args = new object[] { list2.Count };
                        list3 = (IList) local3(args);
                        foreach (object obj5 in list2)
                        {
                            list3.Add(this.DeserializeObject(obj5, genericListElementType));
                        }
                    }
                    obj2 = list3;
                }
            }
            return obj2;
        }

        internal virtual IDictionary<string, ReflectionUtils.GetDelegate> GetterValueFactory(Type type)
        {
            IDictionary<string, ReflectionUtils.GetDelegate> dictionary = new Dictionary<string, ReflectionUtils.GetDelegate>();
            foreach (PropertyInfo info in ReflectionUtils.GetProperties(type))
            {
                if (info.CanRead)
                {
                    MethodInfo getterMethodInfo = ReflectionUtils.GetGetterMethodInfo(info);
                    if (!getterMethodInfo.IsStatic && getterMethodInfo.IsPublic)
                    {
                        dictionary[this.MapClrMemberNameToJsonFieldName(info.Name)] = ReflectionUtils.GetGetMethod(info);
                    }
                }
            }
            foreach (FieldInfo info3 in ReflectionUtils.GetFields(type))
            {
                if (!info3.IsStatic && info3.IsPublic)
                {
                    dictionary[this.MapClrMemberNameToJsonFieldName(info3.Name)] = ReflectionUtils.GetGetMethod(info3);
                }
            }
            return dictionary;
        }

        protected virtual string MapClrMemberNameToJsonFieldName(string clrPropertyName) => 
            clrPropertyName;

        protected virtual object SerializeEnum(Enum p) => 
            Convert.ToDouble(p, CultureInfo.InvariantCulture);

        internal virtual IDictionary<string, KeyValuePair<Type, ReflectionUtils.SetDelegate>> SetterValueFactory(Type type)
        {
            IDictionary<string, KeyValuePair<Type, ReflectionUtils.SetDelegate>> dictionary = new Dictionary<string, KeyValuePair<Type, ReflectionUtils.SetDelegate>>();
            foreach (PropertyInfo info in ReflectionUtils.GetProperties(type))
            {
                if (info.CanWrite)
                {
                    MethodInfo setterMethodInfo = ReflectionUtils.GetSetterMethodInfo(info);
                    if (!setterMethodInfo.IsStatic && setterMethodInfo.IsPublic)
                    {
                        dictionary[this.MapClrMemberNameToJsonFieldName(info.Name)] = new KeyValuePair<Type, ReflectionUtils.SetDelegate>(info.PropertyType, ReflectionUtils.GetSetMethod(info));
                    }
                }
            }
            foreach (FieldInfo info3 in ReflectionUtils.GetFields(type))
            {
                if (!info3.IsInitOnly && (!info3.IsStatic && info3.IsPublic))
                {
                    dictionary[this.MapClrMemberNameToJsonFieldName(info3.Name)] = new KeyValuePair<Type, ReflectionUtils.SetDelegate>(info3.FieldType, ReflectionUtils.GetSetMethod(info3));
                }
            }
            return dictionary;
        }

        protected virtual bool TrySerializeKnownTypes(object input, out object output)
        {
            bool flag = true;
            switch (input)
            {
                case (DateTime _):
                    output = ((DateTime) input).ToUniversalTime().ToString(Iso8601Format[0], CultureInfo.InvariantCulture);
                    break;

                case (DateTimeOffset _):
                    output = ((DateTimeOffset) input).ToUniversalTime().ToString(Iso8601Format[0], CultureInfo.InvariantCulture);
                    break;

                case (Guid _):
                    output = ((Guid) input).ToString("D");
                    break;

                case (Uri _):
                    output = input.ToString();
                    break;

                default:
                {
                    Enum p = input as Enum;
                    if (p != 0)
                    {
                        output = this.SerializeEnum(p);
                    }
                    else
                    {
                        flag = false;
                        output = null;
                    }
                    break;
                }
            }
            return flag;
        }

        public virtual bool TrySerializeNonPrimitiveObject(object input, out object output) => 
            (this.TrySerializeKnownTypes(input, out output) || this.TrySerializeUnknownTypes(input, out output));

        protected virtual bool TrySerializeUnknownTypes(object input, out object output)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            output = null;
            Type type = input.GetType();
            if (type.FullName == null)
            {
                return false;
            }
            IDictionary<string, object> dictionary = new JsonObject();
            foreach (KeyValuePair<string, ReflectionUtils.GetDelegate> pair in this.GetCache[type])
            {
                if (pair.Value != null)
                {
                    dictionary.Add(this.MapClrMemberNameToJsonFieldName(pair.Key), pair.Value(input));
                }
            }
            output = dictionary;
            return true;
        }
    }
}

