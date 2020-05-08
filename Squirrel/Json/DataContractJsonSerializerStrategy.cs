namespace Squirrel.Json
{
    using Squirrel.Json.Reflection;
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [GeneratedCode("simple-json", "1.0.0")]
    internal class DataContractJsonSerializerStrategy : PocoJsonSerializerStrategy
    {
        public DataContractJsonSerializerStrategy()
        {
            base.GetCache = new ReflectionUtils.ThreadSafeDictionary<Type, IDictionary<string, ReflectionUtils.GetDelegate>>(new ReflectionUtils.ThreadSafeDictionaryValueFactory<Type, IDictionary<string, ReflectionUtils.GetDelegate>>(this.GetterValueFactory));
            base.SetCache = new ReflectionUtils.ThreadSafeDictionary<Type, IDictionary<string, KeyValuePair<Type, ReflectionUtils.SetDelegate>>>(new ReflectionUtils.ThreadSafeDictionaryValueFactory<Type, IDictionary<string, KeyValuePair<Type, ReflectionUtils.SetDelegate>>>(this.SetterValueFactory));
        }

        private static bool CanAdd(MemberInfo info, out string jsonKey)
        {
            jsonKey = null;
            if (ReflectionUtils.GetAttribute(info, typeof(IgnoreDataMemberAttribute)) != null)
            {
                return false;
            }
            DataMemberAttribute attribute = (DataMemberAttribute) ReflectionUtils.GetAttribute(info, typeof(DataMemberAttribute));
            if (attribute == null)
            {
                return false;
            }
            jsonKey = string.IsNullOrEmpty(attribute.Name) ? info.Name : attribute.Name;
            return true;
        }

        internal override IDictionary<string, ReflectionUtils.GetDelegate> GetterValueFactory(Type type)
        {
            string str;
            if (ReflectionUtils.GetAttribute(type, typeof(DataContractAttribute)) == null)
            {
                return base.GetterValueFactory(type);
            }
            IDictionary<string, ReflectionUtils.GetDelegate> dictionary = new Dictionary<string, ReflectionUtils.GetDelegate>();
            foreach (PropertyInfo info in ReflectionUtils.GetProperties(type))
            {
                if (info.CanRead && (!ReflectionUtils.GetGetterMethodInfo(info).IsStatic && CanAdd(info, out str)))
                {
                    dictionary[str] = ReflectionUtils.GetGetMethod(info);
                }
            }
            foreach (FieldInfo info2 in ReflectionUtils.GetFields(type))
            {
                if (!info2.IsStatic && CanAdd(info2, out str))
                {
                    dictionary[str] = ReflectionUtils.GetGetMethod(info2);
                }
            }
            return dictionary;
        }

        internal override IDictionary<string, KeyValuePair<Type, ReflectionUtils.SetDelegate>> SetterValueFactory(Type type)
        {
            string str;
            if (ReflectionUtils.GetAttribute(type, typeof(DataContractAttribute)) == null)
            {
                return base.SetterValueFactory(type);
            }
            IDictionary<string, KeyValuePair<Type, ReflectionUtils.SetDelegate>> dictionary = new Dictionary<string, KeyValuePair<Type, ReflectionUtils.SetDelegate>>();
            foreach (PropertyInfo info in ReflectionUtils.GetProperties(type))
            {
                if (info.CanWrite && (!ReflectionUtils.GetSetterMethodInfo(info).IsStatic && CanAdd(info, out str)))
                {
                    dictionary[str] = new KeyValuePair<Type, ReflectionUtils.SetDelegate>(info.PropertyType, ReflectionUtils.GetSetMethod(info));
                }
            }
            foreach (FieldInfo info2 in ReflectionUtils.GetFields(type))
            {
                if (!info2.IsInitOnly && (!info2.IsStatic && CanAdd(info2, out str)))
                {
                    dictionary[str] = new KeyValuePair<Type, ReflectionUtils.SetDelegate>(info2.FieldType, ReflectionUtils.GetSetMethod(info2));
                }
            }
            return dictionary;
        }
    }
}

