namespace NuGet.Runtime
{
    using NuGet;
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml.Linq;

    internal class AssemblyBinding : IEquatable<AssemblyBinding>
    {
        private const string Namespace = "urn:schemas-microsoft-com:asm.v1";
        private string _oldVersion;
        private string _culture;

        internal AssemblyBinding()
        {
        }

        public AssemblyBinding(IAssembly assembly)
        {
            this.Name = assembly.Name;
            this.PublicKeyToken = assembly.PublicKeyToken;
            this.NewVersion = assembly.Version.ToString();
            this.AssemblyNewVersion = assembly.Version;
            this.Culture = assembly.Culture;
        }

        public bool Equals(AssemblyBinding other) => 
            (SafeEquals(this.Name, other.Name) && (SafeEquals(this.PublicKeyToken, other.PublicKeyToken) && (SafeEquals(this.Culture, other.Culture) && SafeEquals(this.ProcessorArchitecture, other.ProcessorArchitecture))));

        public override bool Equals(object obj)
        {
            AssemblyBinding other = obj as AssemblyBinding;
            return ((other == null) ? base.Equals(obj) : this.Equals(other));
        }

        public override int GetHashCode()
        {
            HashCodeCombiner combiner1 = new HashCodeCombiner();
            combiner1.AddObject(this.Name);
            combiner1.AddObject(this.PublicKeyToken);
            combiner1.AddObject(this.Culture);
            combiner1.AddObject(this.ProcessorArchitecture);
            return combiner1.CombinedHash;
        }

        public static XName GetQualifiedName(string name) => 
            XName.Get(name, "urn:schemas-microsoft-com:asm.v1");

        public static AssemblyBinding Parse(XContainer dependentAssembly)
        {
            if (dependentAssembly == null)
            {
                throw new ArgumentNullException("dependentAssembly");
            }
            AssemblyBinding binding = new AssemblyBinding();
            XElement element = dependentAssembly.Element(GetQualifiedName("assemblyIdentity"));
            if (element != null)
            {
                binding.Name = element.Attribute("name").Value;
                binding.Culture = element.GetOptionalAttributeValue("culture", null);
                binding.PublicKeyToken = element.GetOptionalAttributeValue("publicKeyToken", null);
                binding.ProcessorArchitecture = element.GetOptionalAttributeValue("processorArchitecture", null);
            }
            XElement element2 = dependentAssembly.Element(GetQualifiedName("bindingRedirect"));
            if (element2 != null)
            {
                binding.OldVersion = element2.Attribute("oldVersion").Value;
                binding.NewVersion = element2.Attribute("newVersion").Value;
            }
            XElement element3 = dependentAssembly.Element(GetQualifiedName("codeBase"));
            if (element3 != null)
            {
                binding.CodeBaseHref = element3.Attribute("href").Value;
                binding.CodeBaseVersion = element3.Attribute("version").Value;
            }
            XElement element4 = dependentAssembly.Element(GetQualifiedName("publisherPolicy"));
            if (element4 != null)
            {
                binding.PublisherPolicy = element4.Attribute("apply").Value;
            }
            return binding;
        }

        private static bool SafeEquals(object a, object b) => 
            (((a == null) || (b == null)) ? ((a == null) && (b == null)) : a.Equals(b));

        public override string ToString() => 
            this.ToXElement().ToString();

        public XElement ToXElement()
        {
            object[] content = new object[4];
            content[0] = new XAttribute("name", this.Name);
            content[1] = new XAttribute("publicKeyToken", this.PublicKeyToken);
            content[2] = new XAttribute("culture", this.Culture);
            object[] objArray4 = new object[2];
            object[] objArray5 = new object[2];
            content[3] = new XAttribute("processorArchitecture", this.ProcessorArchitecture ?? string.Empty);
            objArray5[0] = new XElement(GetQualifiedName("assemblyIdentity"), content);
            object[] objArray2 = new object[] { new XAttribute("oldVersion", this.OldVersion), new XAttribute("newVersion", this.NewVersion) };
            object[] local2 = objArray5;
            local2[1] = new XElement(GetQualifiedName("bindingRedirect"), objArray2);
            XElement element = new XElement(GetQualifiedName("dependentAssembly"), local2);
            if (!string.IsNullOrEmpty(this.PublisherPolicy))
            {
                element.Add(new XElement(GetQualifiedName("publisherPolicy"), new XAttribute("apply", this.PublisherPolicy)));
            }
            if (!string.IsNullOrEmpty(this.CodeBaseHref))
            {
                object[] objArray3 = new object[] { new XAttribute("href", this.CodeBaseHref), new XAttribute("version", this.CodeBaseVersion) };
                element.Add(new XElement(GetQualifiedName("codeBase"), objArray3));
            }
            element.RemoveAttributes(a => string.IsNullOrEmpty(a.Value));
            return element;
        }

        public string Name { get; private set; }

        public string Culture
        {
            get => 
                (this._culture ?? "neutral");
            set => 
                (this._culture = value);
        }

        public string PublicKeyToken { get; private set; }

        public string ProcessorArchitecture { get; private set; }

        public string NewVersion { get; private set; }

        public string OldVersion
        {
            get => 
                (this._oldVersion ?? ("0.0.0.0-" + this.NewVersion));
            set => 
                (this._oldVersion = value);
        }

        public Version AssemblyNewVersion { get; private set; }

        public string CodeBaseHref { get; private set; }

        public string CodeBaseVersion { get; private set; }

        public string PublisherPolicy { get; private set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly AssemblyBinding.<>c <>9 = new AssemblyBinding.<>c();
            public static Func<XAttribute, bool> <>9__43_0;

            internal bool <ToXElement>b__43_0(XAttribute a) => 
                string.IsNullOrEmpty(a.Value);
        }
    }
}

