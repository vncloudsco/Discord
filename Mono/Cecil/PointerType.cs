namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class PointerType : TypeSpecification
    {
        public PointerType(TypeReference type) : base(type)
        {
            Mixin.CheckType(type);
            base.etype = ElementType.Ptr;
        }

        public override string Name =>
            (base.Name + "*");

        public override string FullName =>
            (base.FullName + "*");

        public override bool IsValueType
        {
            get => 
                false;
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override bool IsPointer =>
            true;
    }
}

