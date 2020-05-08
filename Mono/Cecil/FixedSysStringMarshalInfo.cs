namespace Mono.Cecil
{
    using System;

    internal sealed class FixedSysStringMarshalInfo : MarshalInfo
    {
        internal int size;

        public FixedSysStringMarshalInfo() : base(NativeType.FixedSysString)
        {
            this.size = -1;
        }

        public int Size
        {
            get => 
                this.size;
            set => 
                (this.size = value);
        }
    }
}

