namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;

    internal class Token
    {
        public Token(TokenCategory category, string value)
        {
            this.Category = category;
            this.Value = value;
        }

        public string Value { get; private set; }

        public TokenCategory Category { get; private set; }
    }
}

