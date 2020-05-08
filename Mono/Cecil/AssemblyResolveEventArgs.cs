namespace Mono.Cecil
{
    using System;

    internal sealed class AssemblyResolveEventArgs : EventArgs
    {
        private readonly AssemblyNameReference reference;

        public AssemblyResolveEventArgs(AssemblyNameReference reference)
        {
            this.reference = reference;
        }

        public AssemblyNameReference AssemblyReference =>
            this.reference;
    }
}

