namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    internal class ManifestReferenceSet : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) => 
            ((this.References == null) ? Enumerable.Empty<ValidationResult>() : (from r in this.References select r.Validate(validationContext)));

        [XmlAttribute("targetFramework")]
        public string TargetFramework { get; set; }

        [XmlElement("reference")]
        public List<ManifestReference> References { get; set; }
    }
}

