namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    [XmlType("dependency")]
    internal class ManifestDependency
    {
        [Required(ErrorMessageResourceType=typeof(NuGetResources), ErrorMessageResourceName="Manifest_DependencyIdRequired"), XmlAttribute("id")]
        public string Id { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }
    }
}

