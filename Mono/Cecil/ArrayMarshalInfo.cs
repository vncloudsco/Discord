namespace Mono.Cecil
{
    using System;

    internal sealed class ArrayMarshalInfo : MarshalInfo
    {
        internal NativeType element_type;
        internal int size_parameter_index;
        internal int size;
        internal int size_parameter_multiplier;

        public ArrayMarshalInfo() : base(NativeType.Array)
        {
            this.element_type = NativeType.None;
            this.size_parameter_index = -1;
            this.size = -1;
            this.size_parameter_multiplier = -1;
        }

        public NativeType ElementType
        {
            get => 
                this.element_type;
            set => 
                (this.element_type = value);
        }

        public int SizeParameterIndex
        {
            get => 
                this.size_parameter_index;
            set => 
                (this.size_parameter_index = value);
        }

        public int Size
        {
            get => 
                this.size;
            set => 
                (this.size = value);
        }

        public int SizeParameterMultiplier
        {
            get => 
                this.size_parameter_multiplier;
            set => 
                (this.size_parameter_multiplier = value);
        }
    }
}

