namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    [XmlType("frameworkAssembly")]
    internal class ManifestFrameworkAssembly
    {
        [Required(ErrorMessageResourceType=typeof(NuGetResources), ErrorMessageResourceName="Manifest_AssemblyNameRequired"), XmlAttribute("assemblyName")]
        public string AssemblyName { get; set; }

        [XmlAttribute("targetFramework")]
        public string TargetFramework { get; set; }
    }
}

