namespace Mono.Cecil
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ArrayDimension
    {
        private int? lower_bound;
        private int? upper_bound;
        public int? LowerBound
        {
            get => 
                this.lower_bound;
            set => 
                (this.lower_bound = value);
        }
        public int? UpperBound
        {
            get => 
                this.upper_bound;
            set => 
                (this.upper_bound = value);
        }
        public bool IsSized =>
            ((this.lower_bound != null) || (this.upper_bound != null));
        public ArrayDimension(int? lowerBound, int? upperBound)
        {
            this.lower_bound = lowerBound;
            this.upper_bound = upperBound;
        }

        public override string ToString() => 
            (!this.IsSized ? string.Empty : (this.lower_bound + "..." + this.upper_bound));
    }
}

