namespace Mono.Cecil.Metadata
{
    using System;
    using System.Collections.Generic;

    internal sealed class RowEqualityComparer : IEqualityComparer<Row<string, string>>, IEqualityComparer<Row<uint, uint>>, IEqualityComparer<Row<uint, uint, uint>>
    {
        public bool Equals(Row<string, string> x, Row<string, string> y) => 
            ((x.Col1 == y.Col1) && (x.Col2 == y.Col2));

        public bool Equals(Row<uint, uint> x, Row<uint, uint> y) => 
            ((x.Col1 == y.Col1) && (x.Col2 == y.Col2));

        public bool Equals(Row<uint, uint, uint> x, Row<uint, uint, uint> y) => 
            ((x.Col1 == y.Col1) && ((x.Col2 == y.Col2) && (x.Col3 == y.Col3)));

        public int GetHashCode(Row<string, string> obj)
        {
            string str = obj.Col1;
            string str2 = obj.Col2;
            return (((str != null) ? str.GetHashCode() : 0) ^ ((str2 != null) ? str2.GetHashCode() : 0));
        }

        public int GetHashCode(Row<uint, uint> obj) => 
            (obj.Col1 ^ obj.Col2);

        public int GetHashCode(Row<uint, uint, uint> obj) => 
            ((obj.Col1 ^ obj.Col2) ^ obj.Col3);
    }
}

