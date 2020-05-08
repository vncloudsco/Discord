namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class PackageSource : IEquatable<PackageSource>
    {
        private readonly int _hashCode;

        public PackageSource(string source) : this(source, source, true)
        {
        }

        public PackageSource(string source, string name) : this(source, name, true)
        {
        }

        public PackageSource(string source, string name, bool isEnabled) : this(source, name, isEnabled, false, true)
        {
        }

        public PackageSource(string source, string name, bool isEnabled, bool isOfficial, bool isPersistable = true)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.Name = name;
            this.Source = source;
            this.IsEnabled = isEnabled;
            this.IsOfficial = isOfficial;
            this.IsPersistable = isPersistable;
            this._hashCode = (this.Name.ToUpperInvariant().GetHashCode() * 0xc41) + this.Source.ToUpperInvariant().GetHashCode();
        }

        public PackageSource Clone()
        {
            PackageSource source1 = new PackageSource(this.Source, this.Name, this.IsEnabled, this.IsOfficial, this.IsPersistable);
            source1.UserName = this.UserName;
            source1.Password = this.Password;
            source1.IsPasswordClearText = this.IsPasswordClearText;
            source1.IsMachineWide = this.IsMachineWide;
            return source1;
        }

        public bool Equals(PackageSource other) => 
            ((other != null) ? (this.Name.Equals(other.Name, StringComparison.CurrentCultureIgnoreCase) && this.Source.Equals(other.Source, StringComparison.OrdinalIgnoreCase)) : false);

        public override bool Equals(object obj)
        {
            PackageSource other = obj as PackageSource;
            return ((other == null) ? base.Equals(obj) : this.Equals(other));
        }

        public override int GetHashCode() => 
            this._hashCode;

        public override string ToString() => 
            (this.Name + " [" + this.Source + "]");

        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public string Source { get; private set; }

        public bool IsOfficial { get; set; }

        public bool IsMachineWide { get; set; }

        public bool IsEnabled { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool IsPasswordClearText { get; set; }

        public bool IsPersistable { get; private set; }
    }
}

