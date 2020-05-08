namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [Serializable]
    internal class AssemblyMetadata
    {
        public AssemblyMetadata(Dictionary<string, string> properties = null)
        {
            this.Properties = properties ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public string Name { get; set; }

        public SemanticVersion Version { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Company { get; set; }

        public string Copyright { get; set; }

        public Dictionary<string, string> Properties { get; private set; }
    }
}

