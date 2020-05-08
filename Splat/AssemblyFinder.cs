namespace Splat
{
    using System;
    using System.Linq;
    using System.Reflection;

    internal static class AssemblyFinder
    {
        public static T AttemptToLoadType<T>(string fullTypeName)
        {
            Type type = typeof(AssemblyFinder);
            string[] strArray = new string[] { type.AssemblyQualifiedName.Replace(type.FullName + ", ", ""), type.AssemblyQualifiedName.Replace(type.FullName + ", ", "").Replace(".Portable", "") };
            foreach (AssemblyName name in (from x in strArray select new AssemblyName(x)).ToArray<AssemblyName>())
            {
                string typeName = fullTypeName + ", " + name.FullName;
                Type type2 = Type.GetType(typeName, false);
                if (type2 != null)
                {
                    return (T) Activator.CreateInstance(type2);
                }
            }
            return default(T);
        }
    }
}

