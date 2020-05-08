namespace Mono.Cecil
{
    using Mono;
    using System;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;

    internal class AssemblyNameReference : IMetadataScope, IMetadataTokenProvider
    {
        private string name;
        private string culture;
        private System.Version version;
        private uint attributes;
        private byte[] public_key;
        private byte[] public_key_token;
        private AssemblyHashAlgorithm hash_algorithm;
        private byte[] hash;
        internal Mono.Cecil.MetadataToken token;
        private string full_name;

        internal AssemblyNameReference()
        {
        }

        public AssemblyNameReference(string name, System.Version version)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            this.name = name;
            this.version = version;
            this.hash_algorithm = AssemblyHashAlgorithm.None;
            this.token = new Mono.Cecil.MetadataToken(TokenType.AssemblyRef);
        }

        private byte[] HashPublicKey()
        {
            System.Security.Cryptography.HashAlgorithm algorithm = (this.hash_algorithm != AssemblyHashAlgorithm.Reserved) ? ((System.Security.Cryptography.HashAlgorithm) SHA1.Create()) : ((System.Security.Cryptography.HashAlgorithm) MD5.Create());
            using (algorithm)
            {
                return algorithm.ComputeHash(this.public_key);
            }
        }

        public static AssemblyNameReference Parse(string fullName)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException("fullName");
            }
            if (fullName.Length == 0)
            {
                throw new ArgumentException("Name can not be empty");
            }
            AssemblyNameReference reference = new AssemblyNameReference();
            string[] strArray = fullName.Split(new char[] { ',' });
            for (int i = 0; i < strArray.Length; i++)
            {
                string str = strArray[i].Trim();
                if (i == 0)
                {
                    reference.Name = str;
                }
                else
                {
                    string[] strArray2 = str.Split(new char[] { '=' });
                    if (strArray2.Length != 2)
                    {
                        throw new ArgumentException("Malformed name");
                    }
                    string str3 = strArray2[0].ToLowerInvariant();
                    if (str3 != null)
                    {
                        if (str3 == "version")
                        {
                            reference.Version = new System.Version(strArray2[1]);
                        }
                        else if (str3 == "culture")
                        {
                            reference.Culture = strArray2[1];
                        }
                        else if (str3 == "publickeytoken")
                        {
                            string str2 = strArray2[1];
                            if (str2 != "null")
                            {
                                reference.PublicKeyToken = new byte[str2.Length / 2];
                                for (int j = 0; j < reference.PublicKeyToken.Length; j++)
                                {
                                    reference.PublicKeyToken[j] = byte.Parse(str2.Substring(j * 2, 2), NumberStyles.HexNumber);
                                }
                            }
                        }
                    }
                }
            }
            return reference;
        }

        public override string ToString() => 
            this.FullName;

        public string Name
        {
            get => 
                this.name;
            set
            {
                this.name = value;
                this.full_name = null;
            }
        }

        public string Culture
        {
            get => 
                this.culture;
            set
            {
                this.culture = value;
                this.full_name = null;
            }
        }

        public System.Version Version
        {
            get => 
                this.version;
            set
            {
                this.version = value;
                this.full_name = null;
            }
        }

        public AssemblyAttributes Attributes
        {
            get => 
                ((AssemblyAttributes) this.attributes);
            set => 
                (this.attributes = (uint) value);
        }

        public bool HasPublicKey
        {
            get => 
                this.attributes.GetAttributes(1);
            set => 
                (this.attributes = this.attributes.SetAttributes(1, value));
        }

        public bool IsSideBySideCompatible
        {
            get => 
                this.attributes.GetAttributes(0);
            set => 
                (this.attributes = this.attributes.SetAttributes(0, value));
        }

        public bool IsRetargetable
        {
            get => 
                this.attributes.GetAttributes(0x100);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x100, value));
        }

        public bool IsWindowsRuntime
        {
            get => 
                this.attributes.GetAttributes(0x200);
            set => 
                (this.attributes = this.attributes.SetAttributes(0x200, value));
        }

        public byte[] PublicKey
        {
            get => 
                (this.public_key ?? Empty<byte>.Array);
            set
            {
                this.public_key = value;
                this.HasPublicKey = !this.public_key.IsNullOrEmpty<byte>();
                this.public_key_token = Empty<byte>.Array;
                this.full_name = null;
            }
        }

        public byte[] PublicKeyToken
        {
            get
            {
                if (this.public_key_token.IsNullOrEmpty<byte>() && !this.public_key.IsNullOrEmpty<byte>())
                {
                    byte[] sourceArray = this.HashPublicKey();
                    byte[] destinationArray = new byte[8];
                    Array.Copy(sourceArray, sourceArray.Length - 8, destinationArray, 0, 8);
                    Array.Reverse(destinationArray, 0, 8);
                    this.public_key_token = destinationArray;
                }
                return (this.public_key_token ?? Empty<byte>.Array);
            }
            set
            {
                this.public_key_token = value;
                this.full_name = null;
            }
        }

        public virtual Mono.Cecil.MetadataScopeType MetadataScopeType =>
            Mono.Cecil.MetadataScopeType.AssemblyNameReference;

        public string FullName
        {
            get
            {
                string str;
                if (this.full_name != null)
                {
                    return this.full_name;
                }
                StringBuilder builder = new StringBuilder();
                builder.Append(this.name);
                if (this.version != null)
                {
                    builder.Append(", ");
                    builder.Append("Version=");
                    builder.Append(this.version.ToString());
                }
                builder.Append(", ");
                builder.Append("Culture=");
                builder.Append(string.IsNullOrEmpty(this.culture) ? "neutral" : this.culture);
                builder.Append(", ");
                builder.Append("PublicKeyToken=");
                byte[] publicKeyToken = this.PublicKeyToken;
                if (publicKeyToken.IsNullOrEmpty<byte>() || (publicKeyToken.Length <= 0))
                {
                    builder.Append("null");
                }
                else
                {
                    for (int i = 0; i < publicKeyToken.Length; i++)
                    {
                        builder.Append(publicKeyToken[i].ToString("x2"));
                    }
                }
                this.full_name = str = builder.ToString();
                return str;
            }
        }

        public AssemblyHashAlgorithm HashAlgorithm
        {
            get => 
                this.hash_algorithm;
            set => 
                (this.hash_algorithm = value);
        }

        public virtual byte[] Hash
        {
            get => 
                this.hash;
            set => 
                (this.hash = value);
        }

        public Mono.Cecil.MetadataToken MetadataToken
        {
            get => 
                this.token;
            set => 
                (this.token = value);
        }
    }
}

