namespace Mono.Cecil
{
    using System;

    internal sealed class SafeArrayMarshalInfo : MarshalInfo
    {
        internal VariantType element_type;

        public SafeArrayMarshalInfo() : base(NativeType.SafeArray)
        {
            this.element_type = VariantType.None;
        }

        public VariantType ElementType
        {
            get => 
                this.element_type;
            set => 
                (this.element_type = value);
        }
    }
}

