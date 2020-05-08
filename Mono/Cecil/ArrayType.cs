namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using Mono.Collections.Generic;
    using System;
    using System.Text;

    internal sealed class ArrayType : TypeSpecification
    {
        private Collection<ArrayDimension> dimensions;

        public ArrayType(TypeReference type) : base(type)
        {
            Mixin.CheckType(type);
            base.etype = ElementType.Array;
        }

        public ArrayType(TypeReference type, int rank) : this(type)
        {
            Mixin.CheckType(type);
            if (rank != 1)
            {
                this.dimensions = new Collection<ArrayDimension>(rank);
                for (int i = 0; i < rank; i++)
                {
                    ArrayDimension item = new ArrayDimension();
                    this.dimensions.Add(item);
                }
                base.etype = ElementType.Array;
            }
        }

        public Collection<ArrayDimension> Dimensions
        {
            get
            {
                if (this.dimensions == null)
                {
                    this.dimensions = new Collection<ArrayDimension>();
                    ArrayDimension item = new ArrayDimension();
                    this.dimensions.Add(item);
                }
                return this.dimensions;
            }
        }

        public int Rank =>
            ((this.dimensions == null) ? 1 : this.dimensions.Count);

        public bool IsVector =>
            ((this.dimensions != null) ? ((this.dimensions.Count <= 1) ? !this.dimensions[0].IsSized : false) : true);

        public override bool IsValueType
        {
            get => 
                false;
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override string Name =>
            (base.Name + this.Suffix);

        public override string FullName =>
            (base.FullName + this.Suffix);

        private string Suffix
        {
            get
            {
                if (this.IsVector)
                {
                    return "[]";
                }
                StringBuilder builder = new StringBuilder();
                builder.Append("[");
                for (int i = 0; i < this.dimensions.Count; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(",");
                    }
                    builder.Append(this.dimensions[i].ToString());
                }
                builder.Append("]");
                return builder.ToString();
            }
        }

        public override bool IsArray =>
            true;
    }
}

