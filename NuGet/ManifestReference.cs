namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    [XmlType("reference")]
    internal class ManifestReference : IValidatableObject, IEquatable<ManifestReference>
    {
        private static readonly char[] _referenceFileInvalidCharacters = Path.GetInvalidFileNameChars();

        public bool Equals(ManifestReference other) => 
            ((other != null) && string.Equals(this.File, other.File, StringComparison.OrdinalIgnoreCase));

        public override int GetHashCode() => 
            ((this.File == null) ? 0 : this.File.GetHashCode());

        [IteratorStateMachine(typeof(<Validate>d__5))]
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrEmpty(this.File))
            {
                string[] memberNames = new string[] { "File" };
                yield return new ValidationResult(NuGetResources.Manifest_RequiredElementMissing, memberNames);
            }
            else
            {
                if (this.File.IndexOfAny(_referenceFileInvalidCharacters) == -1)
                {
                    yield break;
                }
                object[] args = new object[] { this.File };
                yield return new ValidationResult(string.Format(CultureInfo.CurrentCulture, NuGetResources.Manifest_InvalidReferenceFile, args));
            }
        }

        [XmlAttribute("file"), Required(ErrorMessageResourceType=typeof(NuGetResources), ErrorMessageResourceName="Manifest_RequiredMetadataMissing")]
        public string File { get; set; }

    }
}

