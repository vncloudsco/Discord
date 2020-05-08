namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Xml;
    using System.Xml.Schema;

    internal static class ManifestSchemaUtility
    {
        internal const string SchemaVersionV1 = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd";
        internal const string SchemaVersionV2 = "http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd";
        internal const string SchemaVersionV3 = "http://schemas.microsoft.com/packaging/2011/10/nuspec.xsd";
        internal const string SchemaVersionV4 = "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd";
        internal const string SchemaVersionV5 = "http://schemas.microsoft.com/packaging/2013/01/nuspec.xsd";
        internal const string SchemaVersionV6 = "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd";
        private static readonly string[] VersionToSchemaMappings = new string[] { "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd", "http://schemas.microsoft.com/packaging/2011/08/nuspec.xsd", "http://schemas.microsoft.com/packaging/2011/10/nuspec.xsd", "http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd", "http://schemas.microsoft.com/packaging/2013/01/nuspec.xsd", "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd" };
        private static ConcurrentDictionary<string, XmlSchemaSet> _manifestSchemaSetCache = new ConcurrentDictionary<string, XmlSchemaSet>(StringComparer.OrdinalIgnoreCase);

        public static XmlSchemaSet GetManifestSchemaSet(string schemaNamespace) => 
            _manifestSchemaSetCache.GetOrAdd(schemaNamespace, delegate (string schema) {
                string str;
                using (StreamReader reader = new StreamReader(typeof(Manifest).Assembly.GetManifestResourceStream("NuGet.Authoring.nuspec.xsd")))
                {
                    string format = reader.ReadToEnd();
                    object[] args = new object[] { schema };
                    str = string.Format(CultureInfo.InvariantCulture, format, args);
                }
                using (StringReader reader2 = new StringReader(str))
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.DtdProcessing = DtdProcessing.Prohibit;
                    settings.XmlResolver = null;
                    XmlSchemaSet set1 = new XmlSchemaSet();
                    set1.Add(schema, XmlReader.Create(reader2, settings));
                    return set1;
                }
            });

        public static string GetSchemaNamespace(int version)
        {
            if ((version > 0) && (version <= VersionToSchemaMappings.Length))
            {
                return VersionToSchemaMappings[version - 1];
            }
            object[] args = new object[] { version };
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.UnknownSchemaVersion, args));
        }

        public static int GetVersionFromNamespace(string @namespace) => 
            (Math.Max(0, Array.IndexOf<string>(VersionToSchemaMappings, @namespace)) + 1);

        public static bool IsKnownSchema(string schemaNamespace) => 
            VersionToSchemaMappings.Contains<string>(schemaNamespace, StringComparer.OrdinalIgnoreCase);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ManifestSchemaUtility.<>c <>9 = new ManifestSchemaUtility.<>c();
            public static Func<string, XmlSchemaSet> <>9__10_0;

            internal XmlSchemaSet <GetManifestSchemaSet>b__10_0(string schema)
            {
                string str;
                using (StreamReader reader = new StreamReader(typeof(Manifest).Assembly.GetManifestResourceStream("NuGet.Authoring.nuspec.xsd")))
                {
                    string format = reader.ReadToEnd();
                    object[] args = new object[] { schema };
                    str = string.Format(CultureInfo.InvariantCulture, format, args);
                }
                using (StringReader reader2 = new StringReader(str))
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.DtdProcessing = DtdProcessing.Prohibit;
                    settings.XmlResolver = null;
                    XmlSchemaSet set1 = new XmlSchemaSet();
                    set1.Add(schema, XmlReader.Create(reader2, settings));
                    return set1;
                }
            }
        }
    }
}

