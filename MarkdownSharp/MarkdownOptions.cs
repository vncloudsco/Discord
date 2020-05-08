namespace MarkdownSharp
{
    using System;
    using System.Runtime.CompilerServices;

    internal class MarkdownOptions
    {
        public bool AutoHyperlink { get; set; }

        public bool AutoNewlines { get; set; }

        public string EmptyElementSuffix { get; set; }

        public bool EncodeProblemUrlCharacters { get; set; }

        public bool LinkEmails { get; set; }

        public bool StrictBoldItalic { get; set; }
    }
}

