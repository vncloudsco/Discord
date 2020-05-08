namespace Mono.Cecil
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct MetadataToken
    {
        private readonly uint token;
        public static readonly MetadataToken Zero;
        public uint RID =>
            (this.token & 0xffffff);
        public Mono.Cecil.TokenType TokenType =>
            (((Mono.Cecil.TokenType) this.token) & ((Mono.Cecil.TokenType) (-16777216)));
        public MetadataToken(uint token)
        {
            this.token = token;
        }

        public MetadataToken(Mono.Cecil.TokenType type) : this(type, 0)
        {
        }

        public MetadataToken(Mono.Cecil.TokenType type, uint rid)
        {
            this.token = ((uint) type) | rid;
        }

        public MetadataToken(Mono.Cecil.TokenType type, int rid)
        {
            this.token = (uint) (type | ((Mono.Cecil.TokenType) rid));
        }

        public int ToInt32() => 
            ((int) this.token);

        public uint ToUInt32() => 
            this.token;

        public override int GetHashCode() => 
            ((int) this.token);

        public override bool Equals(object obj) => 
            ((obj is MetadataToken) && (((MetadataToken) obj).token == this.token));

        public static bool operator ==(MetadataToken one, MetadataToken other) => 
            (one.token == other.token);

        public static bool operator !=(MetadataToken one, MetadataToken other) => 
            (one.token != other.token);

        public override string ToString() => 
            $"[{this.TokenType}:0x{this.RID.ToString("x4")}]";

        static MetadataToken()
        {
            Zero = new MetadataToken(0);
        }
    }
}

