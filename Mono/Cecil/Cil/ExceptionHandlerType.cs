﻿namespace Mono.Cecil.Cil
{
    using System;

    internal enum ExceptionHandlerType
    {
        Catch = 0,
        Filter = 1,
        Finally = 2,
        Fault = 4
    }
}

