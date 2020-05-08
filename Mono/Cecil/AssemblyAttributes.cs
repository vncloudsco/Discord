namespace Mono.Cecil
{
    using System;

    [Flags]
    internal enum AssemblyAttributes : uint
    {
        PublicKey = 1,
        SideBySideCompatible = 0,
        Retargetable = 0x100,
        WindowsRuntime = 0x200,
        DisableJITCompileOptimizer = 0x4000,
        EnableJITCompileTracking = 0x8000
    }
}

