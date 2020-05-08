namespace Mono.Cecil.Cil
{
    using System;
    using System.IO;
    using System.Reflection;

    internal static class SymbolProvider
    {
        private static readonly string symbol_kind = ((Type.GetType("Mono.Runtime") != null) ? "Mdb" : "Pdb");
        private static ISymbolReaderProvider reader_provider;
        private static ISymbolWriterProvider writer_provider;

        public static ISymbolReaderProvider GetPlatformReaderProvider()
        {
            if (reader_provider != null)
            {
                return reader_provider;
            }
            Type platformType = GetPlatformType(GetProviderTypeName("ReaderProvider"));
            return ((platformType != null) ? (reader_provider = (ISymbolReaderProvider) Activator.CreateInstance(platformType)) : null);
        }

        private static AssemblyName GetPlatformSymbolAssemblyName()
        {
            AssemblyName name = typeof(SymbolProvider).Assembly.GetName();
            AssemblyName name2 = new AssemblyName {
                Name = "Mono.Cecil." + symbol_kind,
                Version = name.Version
            };
            name2.SetPublicKeyToken(name.GetPublicKeyToken());
            return name2;
        }

        private static Type GetPlatformType(string fullname)
        {
            Type type = Type.GetType(fullname);
            if (type != null)
            {
                return type;
            }
            AssemblyName platformSymbolAssemblyName = GetPlatformSymbolAssemblyName();
            type = Type.GetType(fullname + ", " + platformSymbolAssemblyName.FullName);
            if (type != null)
            {
                return type;
            }
            try
            {
                Assembly assembly = Assembly.Load(platformSymbolAssemblyName);
                if (assembly != null)
                {
                    return assembly.GetType(fullname);
                }
            }
            catch (FileNotFoundException)
            {
            }
            catch (FileLoadException)
            {
            }
            return null;
        }

        public static ISymbolWriterProvider GetPlatformWriterProvider()
        {
            if (writer_provider != null)
            {
                return writer_provider;
            }
            Type platformType = GetPlatformType(GetProviderTypeName("WriterProvider"));
            return ((platformType != null) ? (writer_provider = (ISymbolWriterProvider) Activator.CreateInstance(platformType)) : null);
        }

        private static string GetProviderTypeName(string name)
        {
            string[] strArray = new string[] { "Mono.Cecil.", symbol_kind, ".", symbol_kind, name };
            return string.Concat(strArray);
        }
    }
}

