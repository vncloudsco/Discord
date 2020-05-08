namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Xml;
    using System.Xml.Linq;

    internal static class ManifestReader
    {
        private static readonly string[] RequiredElements = new string[] { "id", "version", "authors", "description" };

        private static List<ManifestDependency> ReadDependencies(XElement containerElement) => 
            Enumerable.Select(from element in containerElement.ElementsNoNamespace("dependency")
                let idElement = element.Attribute("id")
                where (idElement != null) && !string.IsNullOrEmpty(idElement.Value)
                select <>h__TransparentIdentifier0, delegate (<>f__AnonymousType2<XElement, XAttribute> <>h__TransparentIdentifier0) {
                ManifestDependency dependency1 = new ManifestDependency();
                dependency1.Id = <>h__TransparentIdentifier0.idElement.Value.SafeTrim();
                dependency1.Version = <>h__TransparentIdentifier0.element.GetOptionalAttributeValue("version", null).SafeTrim();
                return dependency1;
            }).ToList<ManifestDependency>();

        private static List<ManifestDependencySet> ReadDependencySets(XElement dependenciesElement)
        {
            if (!dependenciesElement.HasElements)
            {
                return new List<ManifestDependencySet>();
            }
            if (dependenciesElement.ElementsNoNamespace("dependency").Any<XElement>() && dependenciesElement.ElementsNoNamespace("group").Any<XElement>())
            {
                throw new InvalidDataException(NuGetResources.Manifest_DependenciesHasMixedElements);
            }
            List<ManifestDependency> list = ReadDependencies(dependenciesElement);
            if (list.Count <= 0)
            {
                return Enumerable.Select<XElement, ManifestDependencySet>(dependenciesElement.ElementsNoNamespace("group"), delegate (XElement element) {
                    ManifestDependencySet set1 = new ManifestDependencySet();
                    set1.TargetFramework = element.GetOptionalAttributeValue("targetFramework", null).SafeTrim();
                    set1.Dependencies = ReadDependencies(element);
                    return set1;
                }).ToList<ManifestDependencySet>();
            }
            ManifestDependencySet set1 = new ManifestDependencySet();
            set1.Dependencies = list;
            ManifestDependencySet item = set1;
            List<ManifestDependencySet> list1 = new List<ManifestDependencySet>();
            list1.Add(item);
            return list1;
        }

        private static List<ManifestFile> ReadFilesList(XElement xElement)
        {
            if (xElement == null)
            {
                return null;
            }
            List<ManifestFile> list = new List<ManifestFile>();
            foreach (XElement element in xElement.ElementsNoNamespace("file"))
            {
                XAttribute attribute = element.Attribute("src");
                if ((attribute != null) && !string.IsNullOrEmpty(attribute.Value))
                {
                    string target = element.GetOptionalAttributeValue("target", null).SafeTrim();
                    string exclude = element.GetOptionalAttributeValue("exclude", null).SafeTrim();
                    char[] trimChars = new char[] { ';' };
                    char[] separator = new char[] { ';' };
                    list.AddRange(Enumerable.Select<string, ManifestFile>(attribute.Value.Trim(trimChars).Split(separator), delegate (string source) {
                        ManifestFile file1 = new ManifestFile();
                        file1.Source = source.SafeTrim();
                        file1.Target = target.SafeTrim();
                        file1.Exclude = exclude.SafeTrim();
                        return file1;
                    }));
                }
            }
            return list;
        }

        private static List<ManifestFrameworkAssembly> ReadFrameworkAssemblies(XElement frameworkElement) => 
            (frameworkElement.HasElements ? Enumerable.Select(from element in frameworkElement.ElementsNoNamespace("frameworkAssembly")
                let assemblyNameAttribute = element.Attribute("assemblyName")
                where (assemblyNameAttribute != null) && !string.IsNullOrEmpty(assemblyNameAttribute.Value)
                select <>h__TransparentIdentifier0, delegate (<>f__AnonymousType1<XElement, XAttribute> <>h__TransparentIdentifier0) {
                ManifestFrameworkAssembly assembly1 = new ManifestFrameworkAssembly();
                assembly1.AssemblyName = <>h__TransparentIdentifier0.assemblyNameAttribute.Value.SafeTrim();
                assembly1.TargetFramework = <>h__TransparentIdentifier0.element.GetOptionalAttributeValue("targetFramework", null).SafeTrim();
                return assembly1;
            }).ToList<ManifestFrameworkAssembly>() : new List<ManifestFrameworkAssembly>(0));

        public static Manifest ReadManifest(XDocument document)
        {
            XElement xElement = document.Root.ElementsNoNamespace("metadata").FirstOrDefault<XElement>();
            if (xElement == null)
            {
                object[] args = new object[] { "metadata" };
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, NuGetResources.Manifest_RequiredElementMissing, args));
            }
            Manifest manifest1 = new Manifest();
            manifest1.Metadata = ReadMetadata(xElement);
            manifest1.Files = ReadFilesList(document.Root.ElementsNoNamespace("files").FirstOrDefault<XElement>());
            return manifest1;
        }

        private static ManifestMetadata ReadMetadata(XElement xElement)
        {
            ManifestMetadata manifestMetadata = new ManifestMetadata {
                DependencySets = new List<ManifestDependencySet>(),
                ReferenceSets = new List<ManifestReferenceSet>(),
                MinClientVersionString = xElement.GetOptionalAttributeValue("minClientVersion", null)
            };
            HashSet<string> allElements = new HashSet<string>();
            for (XNode node = xElement.FirstNode; node != null; node = node.NextNode)
            {
                XElement element = node as XElement;
                if (element != null)
                {
                    ReadMetadataValue(manifestMetadata, element, allElements);
                }
            }
            foreach (string str in RequiredElements)
            {
                if (!allElements.Contains(str))
                {
                    object[] args = new object[] { str };
                    throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, NuGetResources.Manifest_RequiredElementMissing, args));
                }
            }
            return manifestMetadata;
        }

        private static void ReadMetadataValue(ManifestMetadata manifestMetadata, XElement element, HashSet<string> allElements)
        {
            if (element.Value != null)
            {
                allElements.Add(element.Name.LocalName);
                string s = element.Value.SafeTrim();
                string localName = element.Name.LocalName;
                uint num = <PrivateImplementationDetails>.ComputeStringHash(localName);
                if (num <= 0x657b20cb)
                {
                    if (num <= 0x37386ae0)
                    {
                        if (num <= 0x1da2bf2f)
                        {
                            if (num == 0x10a44713)
                            {
                                if (localName == "summary")
                                {
                                    manifestMetadata.Summary = s;
                                }
                            }
                            else if ((num == 0x1da2bf2f) && (localName == "developmentDependency"))
                            {
                                manifestMetadata.DevelopmentDependency = XmlConvert.ToBoolean(s);
                            }
                        }
                        else if (num == 0x346f3b69)
                        {
                            if (localName == "description")
                            {
                                manifestMetadata.Description = s;
                            }
                        }
                        else if ((num == 0x37386ae0) && (localName == "id"))
                        {
                            manifestMetadata.Id = s;
                        }
                    }
                    else if (num <= 0x4671ae97)
                    {
                        if (num == 0x4333083f)
                        {
                            if (localName == "authors")
                            {
                                manifestMetadata.Authors = s;
                            }
                        }
                        else if ((num == 0x4671ae97) && (localName == "version"))
                        {
                            manifestMetadata.Version = s;
                        }
                    }
                    else if (num == 0x6392f7cb)
                    {
                        if (localName == "references")
                        {
                            manifestMetadata.ReferenceSets = ReadReferenceSets(element);
                        }
                    }
                    else if (num == 0x653d9d60)
                    {
                        if (localName == "dependencies")
                        {
                            manifestMetadata.DependencySets = ReadDependencySets(element);
                        }
                    }
                    else if ((num == 0x657b20cb) && (localName == "releaseNotes"))
                    {
                        manifestMetadata.ReleaseNotes = s;
                    }
                }
                else if (num <= 0xb9ef387b)
                {
                    if (num <= 0x86213ecf)
                    {
                        if (num == 0x670782dd)
                        {
                            if (localName == "licenseUrl")
                            {
                                manifestMetadata.LicenseUrl = s;
                            }
                        }
                        else if ((num == 0x86213ecf) && (localName == "projectUrl"))
                        {
                            manifestMetadata.ProjectUrl = s;
                        }
                    }
                    else if (num == 0x9865b509)
                    {
                        if (localName == "title")
                        {
                            manifestMetadata.Title = s;
                        }
                    }
                    else if (num == 0xb90ded3e)
                    {
                        if (localName == "copyright")
                        {
                            manifestMetadata.Copyright = s;
                        }
                    }
                    else if ((num == 0xb9ef387b) && (localName == "language"))
                    {
                        manifestMetadata.Language = s;
                    }
                }
                else if (num <= 0xe449a0c1)
                {
                    if (num == 0xc5acfe0f)
                    {
                        if (localName == "frameworkAssemblies")
                        {
                            manifestMetadata.FrameworkAssemblies = ReadFrameworkAssemblies(element);
                        }
                    }
                    else if ((num == 0xe449a0c1) && (localName == "iconUrl"))
                    {
                        manifestMetadata.IconUrl = s;
                    }
                }
                else if (num == 0xf04a521c)
                {
                    if (localName == "requireLicenseAcceptance")
                    {
                        manifestMetadata.RequireLicenseAcceptance = XmlConvert.ToBoolean(s);
                    }
                }
                else if (num == 0xf416eba0)
                {
                    if (localName == "tags")
                    {
                        manifestMetadata.Tags = s;
                    }
                }
                else if ((num == 0xf89daae5) && (localName == "owners"))
                {
                    manifestMetadata.Owners = s;
                }
            }
        }

        public static List<ManifestReference> ReadReference(XElement referenceElement, bool throwIfEmpty)
        {
            List<ManifestReference> list = Enumerable.Select(from element in referenceElement.ElementsNoNamespace("reference")
                let fileAttribute = element.Attribute("file")
                where (fileAttribute != null) && !string.IsNullOrEmpty(fileAttribute.Value)
                select <>h__TransparentIdentifier0, delegate (<>f__AnonymousType0<XElement, XAttribute> <>h__TransparentIdentifier0) {
                ManifestReference reference1 = new ManifestReference();
                reference1.File = <>h__TransparentIdentifier0.fileAttribute.Value.SafeTrim();
                return reference1;
            }).ToList<ManifestReference>();
            if (throwIfEmpty && (list.Count == 0))
            {
                throw new InvalidDataException(NuGetResources.Manifest_ReferencesIsEmpty);
            }
            return list;
        }

        private static List<ManifestReferenceSet> ReadReferenceSets(XElement referencesElement)
        {
            if (!referencesElement.HasElements)
            {
                return new List<ManifestReferenceSet>(0);
            }
            if (referencesElement.ElementsNoNamespace("group").Any<XElement>() && referencesElement.ElementsNoNamespace("reference").Any<XElement>())
            {
                throw new InvalidDataException(NuGetResources.Manifest_ReferencesHasMixedElements);
            }
            List<ManifestReference> list = ReadReference(referencesElement, false);
            if (list.Count <= 0)
            {
                return Enumerable.Select<XElement, ManifestReferenceSet>(referencesElement.ElementsNoNamespace("group"), delegate (XElement element) {
                    ManifestReferenceSet set1 = new ManifestReferenceSet();
                    set1.TargetFramework = element.GetOptionalAttributeValue("targetFramework", null).SafeTrim();
                    set1.References = ReadReference(element, true);
                    return set1;
                }).ToList<ManifestReferenceSet>();
            }
            ManifestReferenceSet set1 = new ManifestReferenceSet();
            set1.References = list;
            ManifestReferenceSet item = set1;
            List<ManifestReferenceSet> list1 = new List<ManifestReferenceSet>();
            list1.Add(item);
            return list1;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ManifestReader.<>c <>9 = new ManifestReader.<>c();
            public static Func<XElement, ManifestReferenceSet> <>9__4_0;
            public static Func<XElement, <>f__AnonymousType0<XElement, XAttribute>> <>9__5_0;
            public static Func<<>f__AnonymousType0<XElement, XAttribute>, bool> <>9__5_1;
            public static Func<<>f__AnonymousType0<XElement, XAttribute>, ManifestReference> <>9__5_2;
            public static Func<XElement, <>f__AnonymousType1<XElement, XAttribute>> <>9__6_0;
            public static Func<<>f__AnonymousType1<XElement, XAttribute>, bool> <>9__6_1;
            public static Func<<>f__AnonymousType1<XElement, XAttribute>, ManifestFrameworkAssembly> <>9__6_2;
            public static Func<XElement, ManifestDependencySet> <>9__7_0;
            public static Func<XElement, <>f__AnonymousType2<XElement, XAttribute>> <>9__8_0;
            public static Func<<>f__AnonymousType2<XElement, XAttribute>, bool> <>9__8_1;
            public static Func<<>f__AnonymousType2<XElement, XAttribute>, ManifestDependency> <>9__8_2;

            internal <>f__AnonymousType2<XElement, XAttribute> <ReadDependencies>b__8_0(XElement element) => 
                new { 
                    element = element,
                    idElement = element.Attribute("id")
                };

            internal bool <ReadDependencies>b__8_1(<>f__AnonymousType2<XElement, XAttribute> <>h__TransparentIdentifier0) => 
                ((<>h__TransparentIdentifier0.idElement != null) && !string.IsNullOrEmpty(<>h__TransparentIdentifier0.idElement.Value));

            internal ManifestDependency <ReadDependencies>b__8_2(<>f__AnonymousType2<XElement, XAttribute> <>h__TransparentIdentifier0)
            {
                ManifestDependency dependency1 = new ManifestDependency();
                dependency1.Id = <>h__TransparentIdentifier0.idElement.Value.SafeTrim();
                dependency1.Version = <>h__TransparentIdentifier0.element.GetOptionalAttributeValue("version", null).SafeTrim();
                return dependency1;
            }

            internal ManifestDependencySet <ReadDependencySets>b__7_0(XElement element)
            {
                ManifestDependencySet set1 = new ManifestDependencySet();
                set1.TargetFramework = element.GetOptionalAttributeValue("targetFramework", null).SafeTrim();
                set1.Dependencies = ManifestReader.ReadDependencies(element);
                return set1;
            }

            internal <>f__AnonymousType1<XElement, XAttribute> <ReadFrameworkAssemblies>b__6_0(XElement element) => 
                new { 
                    element = element,
                    assemblyNameAttribute = element.Attribute("assemblyName")
                };

            internal bool <ReadFrameworkAssemblies>b__6_1(<>f__AnonymousType1<XElement, XAttribute> <>h__TransparentIdentifier0) => 
                ((<>h__TransparentIdentifier0.assemblyNameAttribute != null) && !string.IsNullOrEmpty(<>h__TransparentIdentifier0.assemblyNameAttribute.Value));

            internal ManifestFrameworkAssembly <ReadFrameworkAssemblies>b__6_2(<>f__AnonymousType1<XElement, XAttribute> <>h__TransparentIdentifier0)
            {
                ManifestFrameworkAssembly assembly1 = new ManifestFrameworkAssembly();
                assembly1.AssemblyName = <>h__TransparentIdentifier0.assemblyNameAttribute.Value.SafeTrim();
                assembly1.TargetFramework = <>h__TransparentIdentifier0.element.GetOptionalAttributeValue("targetFramework", null).SafeTrim();
                return assembly1;
            }

            internal <>f__AnonymousType0<XElement, XAttribute> <ReadReference>b__5_0(XElement element) => 
                new { 
                    element = element,
                    fileAttribute = element.Attribute("file")
                };

            internal bool <ReadReference>b__5_1(<>f__AnonymousType0<XElement, XAttribute> <>h__TransparentIdentifier0) => 
                ((<>h__TransparentIdentifier0.fileAttribute != null) && !string.IsNullOrEmpty(<>h__TransparentIdentifier0.fileAttribute.Value));

            internal ManifestReference <ReadReference>b__5_2(<>f__AnonymousType0<XElement, XAttribute> <>h__TransparentIdentifier0)
            {
                ManifestReference reference1 = new ManifestReference();
                reference1.File = <>h__TransparentIdentifier0.fileAttribute.Value.SafeTrim();
                return reference1;
            }

            internal ManifestReferenceSet <ReadReferenceSets>b__4_0(XElement element)
            {
                ManifestReferenceSet set1 = new ManifestReferenceSet();
                set1.TargetFramework = element.GetOptionalAttributeValue("targetFramework", null).SafeTrim();
                set1.References = ManifestReader.ReadReference(element, true);
                return set1;
            }
        }
    }
}

