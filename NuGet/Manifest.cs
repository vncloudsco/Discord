namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [XmlType("package")]
    internal class Manifest
    {
        private const string SchemaVersionAttributeName = "schemaVersion";

        public Manifest()
        {
            this.Metadata = new ManifestMetadata();
        }

        private static void CheckSchemaVersion(XDocument document)
        {
            XElement metadataElement = GetMetadataElement(document);
            if (metadataElement != null)
            {
                XAttribute attribute = metadataElement.Attribute("schemaVersion");
                if (attribute != null)
                {
                    attribute.Remove();
                }
                string packageId = GetPackageId(metadataElement);
                if (!ManifestSchemaUtility.IsKnownSchema(document.Root.Name.Namespace.NamespaceName))
                {
                    object[] args = new object[] { packageId, typeof(Manifest).Assembly.GetName().Version };
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.IncompatibleSchema, args));
                }
            }
        }

        private static string ConvertUrlToStringSafe(Uri url)
        {
            if (url != null)
            {
                string str = url.OriginalString.SafeTrim();
                if (!string.IsNullOrEmpty(str))
                {
                    return str;
                }
            }
            return null;
        }

        public static Manifest Create(IPackageMetadata metadata)
        {
            ManifestMetadata metadata1 = new ManifestMetadata();
            metadata1.Id = metadata.Id.SafeTrim();
            metadata1.Version = metadata.Version.ToStringSafe();
            metadata1.Title = metadata.Title.SafeTrim();
            metadata1.Authors = GetCommaSeparatedString(metadata.Authors);
            Manifest manifest1 = new Manifest();
            Manifest manifest2 = new Manifest();
            metadata1.Owners = GetCommaSeparatedString(metadata.Owners) ?? GetCommaSeparatedString(metadata.Authors);
            ManifestMetadata local3 = metadata1;
            ManifestMetadata local4 = metadata1;
            local4.Tags = string.IsNullOrEmpty(metadata.Tags) ? null : metadata.Tags.SafeTrim();
            ManifestMetadata local2 = local4;
            local2.LicenseUrl = ConvertUrlToStringSafe(metadata.LicenseUrl);
            local2.ProjectUrl = ConvertUrlToStringSafe(metadata.ProjectUrl);
            local2.IconUrl = ConvertUrlToStringSafe(metadata.IconUrl);
            local2.RequireLicenseAcceptance = metadata.RequireLicenseAcceptance;
            local2.DevelopmentDependency = metadata.DevelopmentDependency;
            local2.Description = metadata.Description.SafeTrim();
            local2.Copyright = metadata.Copyright.SafeTrim();
            local2.Summary = metadata.Summary.SafeTrim();
            local2.ReleaseNotes = metadata.ReleaseNotes.SafeTrim();
            local2.Language = metadata.Language.SafeTrim();
            local2.DependencySets = CreateDependencySets(metadata);
            local2.FrameworkAssemblies = CreateFrameworkAssemblies(metadata);
            local2.ReferenceSets = CreateReferenceSets(metadata);
            local2.MinClientVersionString = metadata.MinClientVersion.ToStringSafe();
            manifest2.Metadata = local2;
            return manifest2;
        }

        private static List<ManifestDependency> CreateDependencies(ICollection<PackageDependency> dependencies) => 
            ((dependencies != null) ? Enumerable.Select<PackageDependency, ManifestDependency>(dependencies, delegate (PackageDependency dependency) {
                ManifestDependency dependency1 = new ManifestDependency();
                dependency1.Id = dependency.Id.SafeTrim();
                dependency1.Version = dependency.VersionSpec.ToStringSafe();
                return dependency1;
            }).ToList<ManifestDependency>() : new List<ManifestDependency>(0));

        private static List<ManifestDependencySet> CreateDependencySets(IPackageMetadata metadata) => 
            (!metadata.DependencySets.IsEmpty<PackageDependencySet>() ? Enumerable.Select<PackageDependencySet, ManifestDependencySet>(metadata.DependencySets, delegate (PackageDependencySet dependencySet) {
                ManifestDependencySet set1 = new ManifestDependencySet();
                ManifestDependencySet set2 = new ManifestDependencySet();
                set2.TargetFramework = (dependencySet.TargetFramework != null) ? VersionUtility.GetFrameworkString(dependencySet.TargetFramework) : null;
                ManifestDependencySet local1 = set2;
                local1.Dependencies = CreateDependencies(dependencySet.Dependencies);
                return local1;
            }).ToList<ManifestDependencySet>() : null);

        private static List<ManifestFrameworkAssembly> CreateFrameworkAssemblies(IPackageMetadata metadata) => 
            (!metadata.FrameworkAssemblies.IsEmpty<FrameworkAssemblyReference>() ? Enumerable.Select<FrameworkAssemblyReference, ManifestFrameworkAssembly>(metadata.FrameworkAssemblies, delegate (FrameworkAssemblyReference reference) {
                ManifestFrameworkAssembly assembly1 = new ManifestFrameworkAssembly();
                assembly1.AssemblyName = reference.AssemblyName;
                assembly1.TargetFramework = string.Join(", ", Enumerable.Select<FrameworkName, string>(reference.SupportedFrameworks, new Func<FrameworkName, string>(VersionUtility.GetFrameworkString)));
                return assembly1;
            }).ToList<ManifestFrameworkAssembly>() : null);

        private static List<ManifestReference> CreateReferences(PackageReferenceSet referenceSet) => 
            ((referenceSet.References != null) ? Enumerable.Select<string, ManifestReference>(referenceSet.References, delegate (string reference) {
                ManifestReference reference1 = new ManifestReference();
                reference1.File = reference.SafeTrim();
                return reference1;
            }).ToList<ManifestReference>() : new List<ManifestReference>());

        private static List<ManifestReferenceSet> CreateReferenceSets(IPackageMetadata metadata) => 
            Enumerable.Select<PackageReferenceSet, ManifestReferenceSet>(metadata.PackageAssemblyReferences, delegate (PackageReferenceSet referenceSet) {
                ManifestReferenceSet set1 = new ManifestReferenceSet();
                ManifestReferenceSet set2 = new ManifestReferenceSet();
                set2.TargetFramework = (referenceSet.TargetFramework != null) ? VersionUtility.GetFrameworkString(referenceSet.TargetFramework) : null;
                ManifestReferenceSet local1 = set2;
                local1.References = CreateReferences(referenceSet);
                return local1;
            }).ToList<ManifestReferenceSet>();

        private static ValidationContext CreateValidationContext(object value) => 
            new ValidationContext(value, NullServiceProvider.Instance, new Dictionary<object, object>());

        private static string GetCommaSeparatedString(IEnumerable<string> values) => 
            (((values == null) || !values.Any<string>()) ? null : string.Join(",", values));

        private static XElement GetMetadataElement(XDocument document)
        {
            XName name = XName.Get("metadata", document.Root.Name.Namespace.NamespaceName);
            return document.Root.Element(name);
        }

        private static string GetPackageId(XElement metadataElement)
        {
            XName name = XName.Get("id", metadataElement.Document.Root.Name.NamespaceName);
            XElement element = metadataElement.Element(name);
            return element?.Value;
        }

        private static string GetSchemaNamespace(XDocument document)
        {
            string namespaceName = "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd";
            XNamespace namespace2 = document.Root.Name.Namespace;
            if ((namespace2 != null) && !string.IsNullOrEmpty(namespace2.NamespaceName))
            {
                namespaceName = namespace2.NamespaceName;
            }
            return namespaceName;
        }

        public static Manifest ReadFrom(Stream stream, bool validateSchema) => 
            ReadFrom(stream, NullPropertyProvider.Instance, validateSchema);

        public static Manifest ReadFrom(Stream stream, IPropertyProvider propertyProvider, bool validateSchema)
        {
            XDocument document = !ReferenceEquals(propertyProvider, NullPropertyProvider.Instance) ? XDocument.Parse(Preprocessor.Process(stream, propertyProvider, true)) : XmlUtility.LoadSafe(stream, true);
            string schemaNamespace = GetSchemaNamespace(document);
            foreach (XElement local1 in document.Descendants())
            {
                local1.Name = XName.Get(local1.Name.LocalName, schemaNamespace);
            }
            CheckSchemaVersion(document);
            if (validateSchema)
            {
                ValidateManifestSchema(document, schemaNamespace);
            }
            Manifest manifest = ManifestReader.ReadManifest(document);
            Validate(manifest);
            return manifest;
        }

        public void Save(Stream stream)
        {
            this.Save(stream, true, 1);
        }

        public void Save(Stream stream, bool validate)
        {
            this.Save(stream, validate, 1);
        }

        public void Save(Stream stream, int minimumManifestVersion)
        {
            this.Save(stream, true, minimumManifestVersion);
        }

        public void Save(Stream stream, bool validate, int minimumManifestVersion)
        {
            if (validate)
            {
                Validate(this);
            }
            string schemaNamespace = ManifestSchemaUtility.GetSchemaNamespace(Math.Max(minimumManifestVersion, ManifestVersionUtility.GetManifestVersion(this.Metadata)));
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", schemaNamespace);
            new XmlSerializer(typeof(Manifest), schemaNamespace).Serialize(stream, this, namespaces);
        }

        private static bool TryValidate(object value, ICollection<ValidationResult> results)
        {
            if (value == null)
            {
                return true;
            }
            IEnumerable enumerable = value as IEnumerable;
            if (enumerable != null)
            {
                foreach (object obj1 in enumerable)
                {
                    Validator.TryValidateObject(obj1, CreateValidationContext(obj1), results);
                }
            }
            return Validator.TryValidateObject(value, CreateValidationContext(value), results);
        }

        internal static void Validate(Manifest manifest)
        {
            List<ValidationResult> results = new List<ValidationResult>();
            TryValidate(manifest.Metadata, results);
            TryValidate(manifest.Files, results);
            if (manifest.Metadata.DependencySets != null)
            {
                TryValidate(from d in manifest.Metadata.DependencySets select d.Dependencies, results);
            }
            TryValidate(manifest.Metadata.ReferenceSets, results);
            if (results.Any<ValidationResult>())
            {
                throw new ValidationException(string.Join(Environment.NewLine, (IEnumerable<string>) (from r in results select r.ErrorMessage)));
            }
            ValidateDependencySets(manifest.Metadata);
        }

        private static void ValidateDependencySets(IPackageMetadata metadata)
        {
            using (IEnumerator<PackageDependencySet> enumerator = metadata.DependencySets.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (PackageDependency dependency in enumerator.Current.Dependencies)
                    {
                        if (!set.Add(dependency.Id))
                        {
                            object[] args = new object[] { metadata.Id, dependency.Id };
                            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.DuplicateDependenciesDefined, args));
                        }
                        ValidateDependencyVersion(dependency);
                    }
                }
            }
        }

        private static void ValidateDependencyVersion(PackageDependency dependency)
        {
            if ((dependency.VersionSpec != null) && ((dependency.VersionSpec.MinVersion != null) && (dependency.VersionSpec.MaxVersion != null)))
            {
                if ((!dependency.VersionSpec.IsMaxInclusive || !dependency.VersionSpec.IsMinInclusive) && (dependency.VersionSpec.MaxVersion == dependency.VersionSpec.MinVersion))
                {
                    object[] args = new object[] { dependency.Id };
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.DependencyHasInvalidVersion, args));
                }
                if (dependency.VersionSpec.MaxVersion < dependency.VersionSpec.MinVersion)
                {
                    object[] args = new object[] { dependency.Id };
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.DependencyHasInvalidVersion, args));
                }
            }
        }

        private static void ValidateManifestSchema(XDocument document, string schemaNamespace)
        {
            XmlSchemaSet manifestSchemaSet = ManifestSchemaUtility.GetManifestSchemaSet(schemaNamespace);
            document.Validate(manifestSchemaSet, delegate (object sender, ValidationEventArgs e) {
                if (e.Severity == XmlSeverityType.Error)
                {
                    throw new InvalidOperationException(e.Message);
                }
            });
        }

        [XmlElement("metadata", IsNullable=false)]
        public ManifestMetadata Metadata { get; set; }

        [XmlArray("files")]
        public List<ManifestFile> Files { get; set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly Manifest.<>c <>9 = new Manifest.<>c();
            public static Func<PackageReferenceSet, ManifestReferenceSet> <>9__19_0;
            public static Func<string, ManifestReference> <>9__20_0;
            public static Func<PackageDependencySet, ManifestDependencySet> <>9__21_0;
            public static Func<PackageDependency, ManifestDependency> <>9__22_0;
            public static Func<FrameworkAssemblyReference, ManifestFrameworkAssembly> <>9__23_0;
            public static ValidationEventHandler <>9__25_0;
            public static Func<ManifestDependencySet, IEnumerable<ManifestDependency>> <>9__29_0;
            public static Func<ValidationResult, string> <>9__29_1;

            internal ManifestDependency <CreateDependencies>b__22_0(PackageDependency dependency)
            {
                ManifestDependency dependency1 = new ManifestDependency();
                dependency1.Id = dependency.Id.SafeTrim();
                dependency1.Version = dependency.VersionSpec.ToStringSafe();
                return dependency1;
            }

            internal ManifestDependencySet <CreateDependencySets>b__21_0(PackageDependencySet dependencySet)
            {
                ManifestDependencySet set1 = new ManifestDependencySet();
                ManifestDependencySet set2 = new ManifestDependencySet();
                set2.TargetFramework = (dependencySet.TargetFramework != null) ? VersionUtility.GetFrameworkString(dependencySet.TargetFramework) : null;
                ManifestDependencySet local1 = set2;
                local1.Dependencies = Manifest.CreateDependencies(dependencySet.Dependencies);
                return local1;
            }

            internal ManifestFrameworkAssembly <CreateFrameworkAssemblies>b__23_0(FrameworkAssemblyReference reference)
            {
                ManifestFrameworkAssembly assembly1 = new ManifestFrameworkAssembly();
                assembly1.AssemblyName = reference.AssemblyName;
                assembly1.TargetFramework = string.Join(", ", Enumerable.Select<FrameworkName, string>(reference.SupportedFrameworks, new Func<FrameworkName, string>(VersionUtility.GetFrameworkString)));
                return assembly1;
            }

            internal ManifestReference <CreateReferences>b__20_0(string reference)
            {
                ManifestReference reference1 = new ManifestReference();
                reference1.File = reference.SafeTrim();
                return reference1;
            }

            internal ManifestReferenceSet <CreateReferenceSets>b__19_0(PackageReferenceSet referenceSet)
            {
                ManifestReferenceSet set1 = new ManifestReferenceSet();
                ManifestReferenceSet set2 = new ManifestReferenceSet();
                set2.TargetFramework = (referenceSet.TargetFramework != null) ? VersionUtility.GetFrameworkString(referenceSet.TargetFramework) : null;
                ManifestReferenceSet local1 = set2;
                local1.References = Manifest.CreateReferences(referenceSet);
                return local1;
            }

            internal IEnumerable<ManifestDependency> <Validate>b__29_0(ManifestDependencySet d) => 
                d.Dependencies;

            internal string <Validate>b__29_1(ValidationResult r) => 
                r.ErrorMessage;

            internal void <ValidateManifestSchema>b__25_0(object sender, ValidationEventArgs e)
            {
                if (e.Severity == XmlSeverityType.Error)
                {
                    throw new InvalidOperationException(e.Message);
                }
            }
        }

        private class NullServiceProvider : IServiceProvider
        {
            private static readonly IServiceProvider _instance = new Manifest.NullServiceProvider();

            public object GetService(Type serviceType) => 
                null;

            public static IServiceProvider Instance =>
                _instance;
        }
    }
}

