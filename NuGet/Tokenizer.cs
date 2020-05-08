namespace NuGet
{
    using System;
    using System.Globalization;
    using System.Text;

    internal class Tokenizer
    {
        private string _text;
        private int _index;

        public Tokenizer(string text)
        {
            this._text = text;
            this._index = 0;
        }

        private static bool IsWordChar(char ch)
        {
            UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(ch);
            return ((unicodeCategory == UnicodeCategory.LowercaseLetter) || ((unicodeCategory == UnicodeCategory.UppercaseLetter) || ((unicodeCategory == UnicodeCategory.TitlecaseLetter) || ((unicodeCategory == UnicodeCategory.OtherLetter) || ((unicodeCategory == UnicodeCategory.ModifierLetter) || ((unicodeCategory == UnicodeCategory.DecimalDigitNumber) || (unicodeCategory == UnicodeCategory.ConnectorPunctuation)))))));
        }

        private Token ParseText()
        {
            StringBuilder builder = new StringBuilder();
            while ((this._index < this._text.Length) && (this._text[this._index] != '$'))
            {
                builder.Append(this._text[this._index]);
                this._index++;
            }
            return new Token(TokenCategory.Text, builder.ToString());
        }

        private Token ParseTokenAfterDollarSign()
        {
            StringBuilder builder = new StringBuilder();
            while (this._index < this._text.Length)
            {
                char ch = this._text[this._index];
                if (ch == '$')
                {
                    this._index++;
                    return ((builder.Length != 0) ? new Token(TokenCategory.Variable, builder.ToString()) : new Token(TokenCategory.Text, "$"));
                }
                if (!IsWordChar(ch))
                {
                    builder.Insert(0, '$');
                    builder.Append(ch);
                    this._index++;
                    return new Token(TokenCategory.Text, builder.ToString());
                }
                builder.Append(ch);
                this._index++;
            }
            builder.Insert(0, '$');
            return new Token(TokenCategory.Text, builder.ToString());
        }

        public Token Read()
        {
            if (this._index >= this._text.Length)
            {
                return null;
            }
            if (this._text[this._index] != '$')
            {
                return this.ParseText();
            }
            this._index++;
            return this.ParseTokenAfterDollarSign();
        }
    }
}

