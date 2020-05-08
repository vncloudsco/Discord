namespace NuGet
{
    using NuGet.Runtime;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    internal static class AssemblyMetadataExtractor
    {
        public static void ExtractMetadata(PackageBuilder builder, string assemblyPath)
        {
            AssemblyMetadata metadata = GetMetadata(assemblyPath);
            builder.Version = metadata.Version;
            builder.Title = metadata.Title;
            builder.Description = metadata.Description;
            builder.Copyright = metadata.Copyright;
            if (!builder.Authors.Any<string>() && !string.IsNullOrEmpty(metadata.Company))
            {
                builder.Authors.Add(metadata.Company);
            }
            builder.Properties.AddRange<KeyValuePair<string, string>>(metadata.Properties);
            if (builder.Properties.ContainsKey("id"))
            {
                builder.Id = builder.Properties["id"];
            }
            else
            {
                builder.Id = metadata.Name;
            }
        }

        public static AssemblyMetadata GetMetadata(string assemblyPath)
        {
            AssemblyMetadata metadata;
            AppDomainSetup setup1 = new AppDomainSetup();
            setup1.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            AppDomainSetup info = setup1;
            AppDomain domain = AppDomain.CreateDomain("metadata", AppDomain.CurrentDomain.Evidence, info);
            try
            {
                metadata = domain.CreateInstance<MetadataExtractor>().GetMetadata(assemblyPath);
            }
            finally
            {
                AppDomain.Unload(domain);
            }
            return metadata;
        }

        private sealed class MetadataExtractor : MarshalByRefObject
        {
            private static string GetAttributeValueOrDefault<T>(IList<CustomAttributeData> attributes) where T: Attribute
            {
                string str2;
                using (IEnumerator<CustomAttributeData> enumerator = attributes.GetEnumerator())
                {
                    while (true)
                    {
                        if (enumerator.MoveNext())
                        {
                            CustomAttributeData current = enumerator.Current;
                            if (!(current.Constructor.DeclaringType == typeof(T)))
                            {
                                continue;
                            }
                            string str = current.ConstructorArguments[0].Value.ToString();
                            if (string.IsNullOrEmpty(str))
                            {
                                continue;
                            }
                            str2 = str;
                        }
                        else
                        {
                            return null;
                        }
                        break;
                    }
                }
                return str2;
            }

            public AssemblyMetadata GetMetadata(string path)
            {
                AssemblyMetadata metadata;
                AssemblyResolver resolver = new AssemblyResolver(path);
                AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(resolver.ReflectionOnlyAssemblyResolve);
                try
                {
                    SemanticVersion version;
                    Assembly target = Assembly.ReflectionOnlyLoadFrom(path);
                    AssemblyName name = target.GetName();
                    IList<CustomAttributeData> customAttributes = CustomAttributeData.GetCustomAttributes(target);
                    if (!SemanticVersion.TryParse(GetAttributeValueOrDefault<AssemblyInformationalVersionAttribute>(customAttributes), out version))
                    {
                        version = new SemanticVersion(name.Version);
                    }
                    AssemblyMetadata metadata1 = new AssemblyMetadata(GetProperties(customAttributes));
                    metadata1.Name = name.Name;
                    metadata1.Version = version;
                    metadata1.Title = GetAttributeValueOrDefault<AssemblyTitleAttribute>(customAttributes);
                    metadata1.Company = GetAttributeValueOrDefault<AssemblyCompanyAttribute>(customAttributes);
                    metadata1.Description = GetAttributeValueOrDefault<AssemblyDescriptionAttribute>(customAttributes);
                    metadata1.Copyright = GetAttributeValueOrDefault<AssemblyCopyrightAttribute>(customAttributes);
                    metadata = metadata1;
                }
                finally
                {
                    AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= new ResolveEventHandler(resolver.ReflectionOnlyAssemblyResolve);
                }
                return metadata;
            }

            private static Dictionary<string, string> GetProperties(IList<CustomAttributeData> attributes)
            {
                Func<CustomAttributeData, bool> <>9__0;
                Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                string attributeName = typeof(AssemblyMetadataAttribute).FullName;
                Func<CustomAttributeData, bool> func2 = <>9__0;
                if (<>9__0 == null)
                {
                    Func<CustomAttributeData, bool> local1 = <>9__0;
                    func2 = <>9__0 = x => (x.Constructor.DeclaringType.FullName == attributeName) && (x.ConstructorArguments.Count == 2);
                }
                foreach (CustomAttributeData local2 in Enumerable.Where<CustomAttributeData>(attributes, func2))
                {
                    string str = local2.ConstructorArguments[0].Value.ToString();
                    CustomAttributeTypedArgument argument = local2.ConstructorArguments[1];
                    string str2 = argument.Value.ToString();
                    if (!string.IsNullOrEmpty(str) && !string.IsNullOrEmpty(str2))
                    {
                        dictionary[str] = str2;
                    }
                }
                return dictionary;
            }

            private class AssemblyResolver
            {
                private readonly string _lookupPath;

                public AssemblyResolver(string assemblyPath)
                {
                    this._lookupPath = Path.GetDirectoryName(assemblyPath);
                }

                public Assembly ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
                {
                    AssemblyName name = new AssemblyName(AppDomain.CurrentDomain.ApplyPolicy(args.Name));
                    string path = Path.Combine(this._lookupPath, name.Name + ".dll");
                    return (File.Exists(path) ? Assembly.ReflectionOnlyLoadFrom(path) : Assembly.ReflectionOnlyLoad(name.FullName));
                }
            }
        }
    }
}

