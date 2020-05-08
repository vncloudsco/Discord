﻿namespace Mono.Cecil
{
    using System;

    internal enum MethodCallingConvention : byte
    {
        Default = 0,
        C = 1,
        StdCall = 2,
        ThisCall = 3,
        FastCall = 4,
        VarArg = 5,
        Generic = 0x10
    }
}

