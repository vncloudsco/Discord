namespace NuGet
{
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Text;

    internal class VersionSpec : IVersionSpec
    {
        public VersionSpec()
        {
        }

        public VersionSpec(SemanticVersion version)
        {
            this.IsMinInclusive = true;
            this.IsMaxInclusive = true;
            this.MinVersion = version;
            this.MaxVersion = version;
        }

        public override string ToString()
        {
            if ((this.MinVersion != null) && (this.IsMinInclusive && ((this.MaxVersion == null) && !this.IsMaxInclusive)))
            {
                return this.MinVersion.ToString();
            }
            if ((this.MinVersion != null) && ((this.MaxVersion != null) && ((this.MinVersion == this.MaxVersion) && (this.IsMinInclusive && this.IsMaxInclusive))))
            {
                return ("[" + this.MinVersion + "]");
            }
            StringBuilder builder = new StringBuilder();
            builder.Append(this.IsMinInclusive ? '[' : '(');
            object[] args = new object[] { this.MinVersion, this.MaxVersion };
            builder.AppendFormat(CultureInfo.InvariantCulture, "{0}, {1}", args);
            builder.Append(this.IsMaxInclusive ? ']' : ')');
            return builder.ToString();
        }

        public SemanticVersion MinVersion { get; set; }

        public bool IsMinInclusive { get; set; }

        public SemanticVersion MaxVersion { get; set; }

        public bool IsMaxInclusive { get; set; }
    }
}

