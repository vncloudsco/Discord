namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.IO;
    using System.Text;

    internal class WhitespaceTrackingTextReader : PositionTrackingTextReader
    {
        private StringBuilder precedingWhitespace;

        public WhitespaceTrackingTextReader(TextReader reader) : base(reader)
        {
            this.precedingWhitespace = new StringBuilder();
        }

        private void AppendWhitespaceCharacter(int character)
        {
            this.precedingWhitespace.Append((char) character);
        }

        public override int Read()
        {
            int character = base.Read();
            this.UpdateWhitespaceTracking(character);
            return character;
        }

        private void ResetWhitespaceString()
        {
            this.precedingWhitespace = new StringBuilder();
        }

        private void UpdateWhitespaceTracking(int character)
        {
            if (char.IsWhiteSpace((char) character))
            {
                this.AppendWhitespaceCharacter(character);
            }
            else
            {
                this.ResetWhitespaceString();
            }
        }

        public string PrecedingWhitespace =>
            this.precedingWhitespace.ToString();
    }
}

