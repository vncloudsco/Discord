﻿namespace Mono.Cecil
{
    using System;

    [Flags]
    internal enum EventAttributes : ushort
    {
        None = 0,
        SpecialName = 0x200,
        RTSpecialName = 0x400
    }
}

