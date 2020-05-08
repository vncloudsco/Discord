namespace Mono.Cecil
{
    using Mono;
    using System;

    internal sealed class AssemblyNameDefinition : AssemblyNameReference
    {
        internal AssemblyNameDefinition()
        {
            base.token = new MetadataToken(TokenType.Assembly, 1);
        }

        public AssemblyNameDefinition(string name, Version version) : base(name, version)
        {
            base.token = new MetadataToken(TokenType.Assembly, 1);
        }

        public override byte[] Hash =>
            Empty<byte>.Array;
    }
}

