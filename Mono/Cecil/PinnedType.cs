namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class PinnedType : TypeSpecification
    {
        public PinnedType(TypeReference type) : base(type)
        {
            Mixin.CheckType(type);
            base.etype = ElementType.Pinned;
        }

        public override bool IsValueType
        {
            get => 
                false;
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override bool IsPinned =>
            true;
    }
}

