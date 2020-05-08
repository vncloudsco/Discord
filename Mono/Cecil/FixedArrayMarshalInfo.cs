namespace Mono.Cecil
{
    using System;

    internal sealed class FixedArrayMarshalInfo : MarshalInfo
    {
        internal NativeType element_type;
        internal int size;

        public FixedArrayMarshalInfo() : base(NativeType.FixedArray)
        {
            this.element_type = NativeType.None;
        }

        public NativeType ElementType
        {
            get => 
                this.element_type;
            set => 
                (this.element_type = value);
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

