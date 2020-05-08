namespace Mono.Cecil
{
    using System;

    internal enum TokenType : uint
    {
        Module = 0,
        TypeRef = 0x1000000,
        TypeDef = 0x2000000,
        Field = 0x4000000,
        Method = 0x6000000,
        Param = 0x8000000,
        InterfaceImpl = 0x9000000,
        MemberRef = 0xa000000,
        CustomAttribute = 0xc000000,
        Permission = 0xe000000,
        Signature = 0x11000000,
        Event = 0x14000000,
        Property = 0x17000000,
        ModuleRef = 0x1a000000,
        TypeSpec = 0x1b000000,
        Assembly = 0x20000000,
        AssemblyRef = 0x23000000,
        File = 0x26000000,
        ExportedType = 0x27000000,
        ManifestResource = 0x28000000,
        GenericParam = 0x2a000000,
        MethodSpec = 0x2b000000,
        String = 0x70000000
    }
}

