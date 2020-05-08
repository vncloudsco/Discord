﻿namespace Mono.Cecil
{
    using System;

    internal interface IConstantProvider : IMetadataTokenProvider
    {
        bool HasConstant { get; set; }

        object Constant { get; set; }
    }
}

