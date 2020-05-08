namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.IO;

    internal class PositionTrackingTextReader : TextReader
    {
        private const int newlineCharacter = 10;
        private TextReader internalReader;
        private int lineNumber = 1;
        private int linePosition = 1;
        private int characterPosition = 1;

        public PositionTrackingTextReader(TextReader textReader)
        {
            this.internalReader = textReader;
        }

        public override int Peek() => 
            this.internalReader.Peek();

        public override int Read()
        {
            int character = this.internalReader.Read();
            this.UpdatePosition(character);
            return character;
        }

        public bool ReadToPosition(int characterPosition)
        {
            while ((this.characterPosition < characterPosition) && (this.Peek() != -1))
            {
                this.Read();
            }
            return (this.characterPosition == characterPosition);
        }

        public bool ReadToPosition(int lineNumber, int linePosition)
        {
            while ((this.lineNumber < lineNumber) && (this.Peek() != -1))
            {
                this.ReadLine();
            }
            while ((this.linePosition < linePosition) && (this.Peek() != -1))
            {
                this.Read();
            }
            return ((this.lineNumber == lineNumber) && (this.linePosition == linePosition));
        }

        private void UpdatePosition(int character)
        {
            if (character != 10)
            {
                this.linePosition++;
            }
            else
            {
                this.lineNumber++;
                this.linePosition = 1;
            }
            this.characterPosition++;
        }
    }
}

