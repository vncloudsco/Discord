namespace Mono.Cecil
{
    using System;
    using System.Collections.Generic;

    internal class DefaultAssemblyResolver : BaseAssemblyResolver
    {
        private readonly IDictionary<string, AssemblyDefinition> cache = new Dictionary<string, AssemblyDefinition>(StringComparer.Ordinal);

        protected void RegisterAssembly(AssemblyDefinition assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            string fullName = assembly.Name.FullName;
            if (!this.cache.ContainsKey(fullName))
            {
                this.cache[fullName] = assembly;
            }
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            AssemblyDefinition definition;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (!this.cache.TryGetValue(name.FullName, out definition))
            {
                definition = base.Resolve(name);
                this.cache[name.FullName] = definition;
            }
            return definition;
        }
    }
}

