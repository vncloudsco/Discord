namespace NuGet.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal class RemoteAssembly : MarshalByRefObject, IAssembly
    {
        private static readonly Dictionary<Tuple<string, string>, Assembly> _assemblyCache = new Dictionary<Tuple<string, string>, Assembly>();
        private readonly List<IAssembly> _referencedAssemblies = new List<IAssembly>();

        private static RemoteAssembly CopyAssemblyProperties(AssemblyName assemblyName, RemoteAssembly assembly)
        {
            assembly.Name = assemblyName.Name;
            assembly.Version = assemblyName.Version;
            assembly.PublicKeyToken = assemblyName.GetPublicKeyTokenString();
            string str = assemblyName.CultureInfo.ToString();
            assembly.Culture = string.IsNullOrEmpty(str) ? "neutral" : str;
            return assembly;
        }

        public void Load(string path)
        {
            Assembly assembly;
            Tuple<string, string> key = Tuple.Create<string, string>(Path.GetFileName(path).ToUpperInvariant(), AssemblyName.GetAssemblyName(path).FullName);
            if (!_assemblyCache.TryGetValue(key, out assembly))
            {
                assembly = Assembly.ReflectionOnlyLoadFrom(path);
                _assemblyCache[key] = assembly;
            }
            CopyAssemblyProperties(assembly.GetName(), this);
            foreach (AssemblyName name in assembly.GetReferencedAssemblies())
            {
                RemoteAssembly assembly2 = new RemoteAssembly();
                this._referencedAssemblies.Add(CopyAssemblyProperties(name, assembly2));
            }
        }

        internal static IAssembly LoadAssembly(string path, AppDomain domain)
        {
            if (!ReferenceEquals(domain, AppDomain.CurrentDomain))
            {
                RemoteAssembly local1 = domain.CreateInstance<RemoteAssembly>();
                local1.Load(path);
                return local1;
            }
            RemoteAssembly assembly1 = new RemoteAssembly();
            assembly1.Load(path);
            return assembly1;
        }

        public string Name { get; private set; }

        public System.Version Version { get; private set; }

        public string PublicKeyToken { get; private set; }

        public string Culture { get; private set; }

        public IEnumerable<IAssembly> ReferencedAssemblies =>
            this._referencedAssemblies;
    }
}

