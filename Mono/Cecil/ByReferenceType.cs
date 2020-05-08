namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class ByReferenceType : TypeSpecification
    {
        public ByReferenceType(TypeReference type) : base(type)
        {
            Mixin.CheckType(type);
            base.etype = ElementType.ByRef;
        }

        public override string Name =>
            (base.Name + "&");

        public override string FullName =>
            (base.FullName + "&");

        public override bool IsValueType
        {
            get => 
                false;
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override bool IsByReference =>
            true;
    }
}

