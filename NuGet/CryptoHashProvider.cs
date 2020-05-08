namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;

    internal class CryptoHashProvider : IHashProvider
    {
        private const string SHA512HashAlgorithm = "SHA512";
        private const string SHA256HashAlgorithm = "SHA256";
        private readonly string _hashAlgorithm;

        public CryptoHashProvider() : this(null)
        {
        }

        public CryptoHashProvider(string hashAlgorithm)
        {
            if (string.IsNullOrEmpty(hashAlgorithm))
            {
                hashAlgorithm = "SHA512";
            }
            else if (!hashAlgorithm.Equals("SHA512", StringComparison.OrdinalIgnoreCase) && !hashAlgorithm.Equals("SHA256", StringComparison.OrdinalIgnoreCase))
            {
                object[] args = new object[] { hashAlgorithm };
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, NuGetResources.UnsupportedHashAlgorithm, args), "hashAlgorithm");
            }
            this._hashAlgorithm = hashAlgorithm;
        }

        public byte[] CalculateHash(Stream stream)
        {
            using (HashAlgorithm algorithm = this.GetHashAlgorithm())
            {
                return algorithm.ComputeHash(stream);
            }
        }

        public byte[] CalculateHash(byte[] data)
        {
            using (HashAlgorithm algorithm = this.GetHashAlgorithm())
            {
                return algorithm.ComputeHash(data);
            }
        }

        private HashAlgorithm GetHashAlgorithm() => 
            (!this._hashAlgorithm.Equals("SHA256", StringComparison.OrdinalIgnoreCase) ? (AllowOnlyFipsAlgorithms ? ((HashAlgorithm) new SHA512CryptoServiceProvider()) : ((HashAlgorithm) new SHA512Managed())) : (AllowOnlyFipsAlgorithms ? ((HashAlgorithm) new SHA256CryptoServiceProvider()) : ((HashAlgorithm) new SHA256Managed())));

        private static bool ReadFipsConfigValue()
        {
            Type type = typeof(CryptoConfig);
            if (type == null)
            {
                return false;
            }
            PropertyInfo property = type.GetProperty("AllowOnlyFipsAlgorithms", BindingFlags.Public | BindingFlags.Static);
            return ((property != null) && ((bool) property.GetValue(null, null)));
        }

        public bool VerifyHash(byte[] data, byte[] hash) => 
            this.CalculateHash(data).SequenceEqual<byte>(hash);

        private static bool AllowOnlyFipsAlgorithms =>
            ReadFipsConfigValue();
    }
}

