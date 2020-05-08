namespace Mono.Cecil
{
    using Mono.Cecil.Metadata;
    using System;

    internal sealed class RequiredModifierType : TypeSpecification, IModifierType
    {
        private TypeReference modifier_type;

        public RequiredModifierType(TypeReference modifierType, TypeReference type) : base(type)
        {
            Mixin.CheckModifier(modifierType, type);
            this.modifier_type = modifierType;
            base.etype = ElementType.CModReqD;
        }

        public TypeReference ModifierType
        {
            get => 
                this.modifier_type;
            set => 
                (this.modifier_type = value);
        }

        public override string Name =>
            (base.Name + this.Suffix);

        public override string FullName =>
            (base.FullName + this.Suffix);

        private string Suffix =>
            (" modreq(" + this.modifier_type + ")");

        public override bool IsValueType
        {
            get => 
                false;
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override bool IsRequiredModifier =>
            true;

        public override bool ContainsGenericParameter =>
            (this.modifier_type.ContainsGenericParameter || base.ContainsGenericParameter);
    }
}

