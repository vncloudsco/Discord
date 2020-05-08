namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class SentinelType : TypeSpecification
    {
        public SentinelType(TypeReference type) : base(type)
        {
            Mixin.CheckType(type);
            base.etype = ElementType.Sentinel;
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

        public override bool IsSentinel =>
            true;
    }
}

