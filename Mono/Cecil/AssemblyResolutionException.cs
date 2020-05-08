namespace Mono.Cecil
{
    using System;
    using System.IO;
    using System.Runtime.Serialization;

    [Serializable]
    internal class AssemblyResolutionException : FileNotFoundException
    {
        private readonly AssemblyNameReference reference;

        public AssemblyResolutionException(AssemblyNameReference reference) : base($"Failed to resolve assembly: '{reference}'")
        {
            this.reference = reference;
        }

        protected AssemblyResolutionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public AssemblyNameReference AssemblyReference =>
            this.reference;
    }
}

