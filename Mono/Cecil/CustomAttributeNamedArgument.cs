namespace Mono.Cecil
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct CustomAttributeNamedArgument
    {
        private readonly string name;
        private readonly CustomAttributeArgument argument;
        public string Name =>
            this.name;
        public CustomAttributeArgument Argument =>
            this.argument;
        public CustomAttributeNamedArgument(string name, CustomAttributeArgument argument)
        {
            Mixin.CheckName(name);
            this.name = name;
            this.argument = argument;
        }
    }
}

