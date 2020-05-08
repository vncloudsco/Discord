namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Xml.Serialization;

    [XmlType("file")]
    internal class ManifestFile : IValidatableObject
    {
        private static readonly char[] _invalidTargetChars;
        private static readonly char[] _invalidSourceCharacters;

        static ManifestFile()
        {
            char[] second = new char[] { '\\', '/', '.' };
            _invalidTargetChars = Path.GetInvalidFileNameChars().Except<char>(second).ToArray<char>();
            _invalidSourceCharacters = Path.GetInvalidPathChars();
        }

        [IteratorStateMachine(typeof(<Validate>d__14))]
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrEmpty(this.Source) && (this.Source.IndexOfAny(_invalidSourceCharacters) != -1))
            {
                object[] args = new object[] { this.Source };
                yield return new ValidationResult(string.Format(CultureInfo.CurrentCulture, NuGetResources.Manifest_SourceContainsInvalidCharacters, args));
            }
            if (!string.IsNullOrEmpty(this.Target) && (this.Target.IndexOfAny(_invalidTargetChars) != -1))
            {
                object[] args = new object[] { this.Target };
                yield return new ValidationResult(string.Format(CultureInfo.CurrentCulture, NuGetResources.Manifest_TargetContainsInvalidCharacters, args));
            }
            while (true)
            {
                if (!string.IsNullOrEmpty(this.Exclude) && (this.Exclude.IndexOfAny(_invalidSourceCharacters) != -1))
                {
                    object[] args = new object[] { this.Exclude };
                    yield return new ValidationResult(string.Format(CultureInfo.CurrentCulture, NuGetResources.Manifest_ExcludeContainsInvalidCharacters, args));
                }
            }
        }

        [Required(ErrorMessageResourceType=typeof(NuGetResources), ErrorMessageResourceName="Manifest_RequiredMetadataMissing"), XmlAttribute("src")]
        public string Source { get; set; }

        [XmlAttribute("target")]
        public string Target { get; set; }

        [XmlAttribute("exclude")]
        public string Exclude { get; set; }

    }
}

