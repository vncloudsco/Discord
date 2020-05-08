namespace Mono.Cecil
{
    using System;

    internal class MarshalInfo
    {
        internal Mono.Cecil.NativeType native;

        public MarshalInfo(Mono.Cecil.NativeType native)
        {
            this.native = native;
        }

        public Mono.Cecil.NativeType NativeType
        {
            get => 
                this.native;
            set => 
                (this.native = value);
        }
    }
}

