﻿namespace Mono.Cecil.Metadata
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Row<T1, T2, T3, T4>
    {
        internal T1 Col1;
        internal T2 Col2;
        internal T3 Col3;
        internal T4 Col4;
        public Row(T1 col1, T2 col2, T3 col3, T4 col4)
        {
            this.Col1 = col1;
            this.Col2 = col2;
            this.Col3 = col3;
            this.Col4 = col4;
        }
    }
}

