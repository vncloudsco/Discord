namespace Mono.Cecil
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct CustomAttributeArgument
    {
        private readonly TypeReference type;
        private readonly object value;
        public TypeReference Type =>
            this.type;
        public object Value =>
            this.value;
        public CustomAttributeArgument(TypeReference type, object value)
        {
            Mixin.CheckType(type);
            this.type = type;
            this.value = value;
        }
    }
}

