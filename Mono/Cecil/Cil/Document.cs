namespace Mono.Cecil.Cil
{
    using Mono;
    using System;

    internal sealed class Document
    {
        private string url;
        private byte type;
        private byte hash_algorithm;
        private byte language;
        private byte language_vendor;
        private byte[] hash;

        public Document(string url)
        {
            this.url = url;
            this.hash = Empty<byte>.Array;
        }

        public string Url
        {
            get => 
                this.url;
            set => 
                (this.url = value);
        }

        public DocumentType Type
        {
            get => 
                ((DocumentType) this.type);
            set => 
                (this.type = (byte) value);
        }

        public DocumentHashAlgorithm HashAlgorithm
        {
            get => 
                ((DocumentHashAlgorithm) this.hash_algorithm);
            set => 
                (this.hash_algorithm = (byte) value);
        }

        public DocumentLanguage Language
        {
            get => 
                ((DocumentLanguage) this.language);
            set => 
                (this.language = (byte) value);
        }

        public DocumentLanguageVendor LanguageVendor
        {
            get => 
                ((DocumentLanguageVendor) this.language_vendor);
            set => 
                (this.language_vendor = (byte) value);
        }

        public byte[] Hash
        {
            get => 
                this.hash;
            set => 
                (this.hash = value);
        }
    }
}

