namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    internal class ManifestDependencySet
    {
        [XmlAttribute("targetFramework")]
        public string TargetFramework { get; set; }

        [XmlElement("dependency")]
        public List<ManifestDependency> Dependencies { get; set; }
    }
}

