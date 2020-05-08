namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;

    internal class XmlAttributePreservingWriter : XmlWriter
    {
        private XmlTextWriter xmlWriter;
        private AttributeTextWriter textWriter;

        public XmlAttributePreservingWriter(TextWriter textWriter)
        {
            this.textWriter = new AttributeTextWriter(textWriter);
            this.xmlWriter = new XmlTextWriter(this.textWriter);
        }

        public XmlAttributePreservingWriter(Stream w, Encoding encoding) : this((encoding == null) ? new StreamWriter(w) : new StreamWriter(w, encoding))
        {
        }

        public XmlAttributePreservingWriter(string fileName, Encoding encoding) : this((encoding == null) ? new StreamWriter(fileName) : new StreamWriter(fileName, false, encoding))
        {
        }

        public override void Close()
        {
            this.xmlWriter.Close();
        }

        public override void Flush()
        {
            this.xmlWriter.Flush();
        }

        private bool IsOnlyWhitespace(string whitespace)
        {
            foreach (char ch in whitespace)
            {
                if (!char.IsWhiteSpace(ch))
                {
                    return false;
                }
            }
            return true;
        }

        public override string LookupPrefix(string ns) => 
            this.xmlWriter.LookupPrefix(ns);

        public string SetAttributeNewLineString(string newLineString)
        {
            string attributeNewLineString = this.textWriter.AttributeNewLineString;
            if ((newLineString == null) && (this.xmlWriter.Settings != null))
            {
                newLineString = this.xmlWriter.Settings.NewLineChars;
            }
            if (newLineString == null)
            {
                newLineString = "\r\n";
            }
            this.textWriter.AttributeNewLineString = newLineString;
            return attributeNewLineString;
        }

        public void WriteAttributeTrailingWhitespace(string whitespace)
        {
            if (this.WriteState == System.Xml.WriteState.Attribute)
            {
                this.WriteEndAttribute();
            }
            else if (this.WriteState != System.Xml.WriteState.Element)
            {
                throw new InvalidOperationException();
            }
            this.textWriter.Write(whitespace);
        }

        public void WriteAttributeWhitespace(string whitespace)
        {
            if (this.WriteState == System.Xml.WriteState.Attribute)
            {
                this.WriteEndAttribute();
            }
            else if (this.WriteState != System.Xml.WriteState.Element)
            {
                throw new InvalidOperationException();
            }
            this.textWriter.AttributeLeadingWhitespace = whitespace;
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            this.xmlWriter.WriteBase64(buffer, index, count);
        }

        public override void WriteCData(string text)
        {
            this.xmlWriter.WriteCData(text);
        }

        public override void WriteCharEntity(char ch)
        {
            this.xmlWriter.WriteCharEntity(ch);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            this.xmlWriter.WriteChars(buffer, index, count);
        }

        public override void WriteComment(string text)
        {
            this.textWriter.StartComment();
            this.xmlWriter.WriteComment(text);
            this.textWriter.EndComment();
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
            this.xmlWriter.WriteDocType(name, pubid, sysid, subset);
        }

        public override void WriteEndAttribute()
        {
            this.xmlWriter.WriteEndAttribute();
            this.textWriter.EndAttribute();
        }

        public override void WriteEndDocument()
        {
            this.xmlWriter.WriteEndDocument();
        }

        public override void WriteEndElement()
        {
            this.xmlWriter.WriteEndElement();
        }

        public override void WriteEntityRef(string name)
        {
            this.xmlWriter.WriteEntityRef(name);
        }

        public override void WriteFullEndElement()
        {
            this.xmlWriter.WriteFullEndElement();
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            this.xmlWriter.WriteProcessingInstruction(name, text);
        }

        public override void WriteRaw(string data)
        {
            this.xmlWriter.WriteRaw(data);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            this.xmlWriter.WriteRaw(buffer, index, count);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            this.textWriter.StartAttribute();
            this.xmlWriter.WriteStartAttribute(prefix, localName, ns);
        }

        public override void WriteStartDocument()
        {
            this.xmlWriter.WriteStartDocument();
        }

        public override void WriteStartDocument(bool standalone)
        {
            this.xmlWriter.WriteStartDocument(standalone);
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            this.xmlWriter.WriteStartElement(prefix, localName, ns);
        }

        public override void WriteString(string text)
        {
            this.xmlWriter.WriteString(text);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            this.xmlWriter.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteWhitespace(string ws)
        {
            this.xmlWriter.WriteWhitespace(ws);
        }

        public override System.Xml.WriteState WriteState =>
            this.xmlWriter.WriteState;

        private class AttributeTextWriter : TextWriter
        {
            private State state;
            private StringBuilder writeBuffer;
            private TextWriter baseWriter;
            private string leadingWhitespace;
            private int lineNumber;
            private int linePosition;
            private int maxLineLength;
            private string newLineString;

            public AttributeTextWriter(TextWriter baseWriter) : base(CultureInfo.InvariantCulture)
            {
                this.lineNumber = 1;
                this.linePosition = 1;
                this.maxLineLength = 160;
                this.newLineString = "\r\n";
                this.baseWriter = baseWriter;
            }

            private void ChangeState(State newState)
            {
                if (this.state != newState)
                {
                    State state = this.state;
                    this.state = newState;
                    if (this.StateRequiresBuffer(newState))
                    {
                        this.CreateBuffer();
                    }
                    else if (this.StateRequiresBuffer(state))
                    {
                        this.FlushBuffer();
                    }
                }
            }

            public override void Close()
            {
                this.baseWriter.Close();
            }

            private void CreateBuffer()
            {
                if (this.writeBuffer == null)
                {
                    this.writeBuffer = new StringBuilder();
                }
            }

            public void EndAttribute()
            {
                this.WriteQueuedAttribute();
            }

            public void EndComment()
            {
                this.ChangeState(State.Writing);
            }

            public override void Flush()
            {
                this.baseWriter.Flush();
            }

            private void FlushBuffer()
            {
                if (this.writeBuffer != null)
                {
                    State state = this.state;
                    try
                    {
                        this.state = State.FlushingBuffer;
                        this.Write(this.writeBuffer.ToString());
                        this.writeBuffer = null;
                    }
                    finally
                    {
                        this.state = state;
                    }
                }
            }

            private void ReallyWriteCharacter(char value)
            {
                this.baseWriter.Write(value);
                if (value != '\n')
                {
                    this.linePosition++;
                }
                else
                {
                    this.lineNumber++;
                    this.linePosition = 1;
                }
            }

            public void StartAttribute()
            {
                this.ChangeState(State.WaitingForAttributeLeadingSpace);
            }

            public void StartComment()
            {
                this.ChangeState(State.WritingComment);
            }

            private bool StateRequiresBuffer(State state) => 
                ((state == State.Buffering) || (state == State.ReadingAttribute));

            private void UpdateState(char value)
            {
                if (this.state != State.WritingComment)
                {
                    char ch = value;
                    if (ch == ' ')
                    {
                        if (this.state == State.Writing)
                        {
                            this.ChangeState(State.Buffering);
                        }
                    }
                    else if (ch != '/')
                    {
                        if (ch != '>')
                        {
                            if (this.state == State.Buffering)
                            {
                                this.ChangeState(State.Writing);
                            }
                        }
                        else if (this.state == State.Buffering)
                        {
                            string str = this.writeBuffer.ToString();
                            if (str.EndsWith(" /", StringComparison.Ordinal))
                            {
                                this.writeBuffer.Remove(str.LastIndexOf(' '), 1);
                            }
                            this.ChangeState(State.Writing);
                        }
                    }
                }
            }

            public override void Write(char value)
            {
                this.UpdateState(value);
                switch (this.state)
                {
                    case State.Writing:
                    case State.FlushingBuffer:
                    case State.WritingComment:
                        break;

                    case State.WaitingForAttributeLeadingSpace:
                        if (value != ' ')
                        {
                            break;
                        }
                        this.ChangeState(State.ReadingAttribute);
                        return;

                    case State.ReadingAttribute:
                    case State.Buffering:
                        this.writeBuffer.Append(value);
                        return;

                    default:
                        return;
                }
                this.ReallyWriteCharacter(value);
            }

            private void WriteQueuedAttribute()
            {
                if (this.leadingWhitespace != null)
                {
                    this.writeBuffer.Insert(0, this.leadingWhitespace);
                    this.leadingWhitespace = null;
                }
                else if (((this.linePosition + this.writeBuffer.Length) + 1) > this.MaxLineLength)
                {
                    this.writeBuffer.Insert(0, this.AttributeNewLineString);
                }
                else
                {
                    this.writeBuffer.Insert(0, ' ');
                }
                this.ChangeState(State.Writing);
            }

            public string AttributeLeadingWhitespace
            {
                set => 
                    (this.leadingWhitespace = value);
            }

            public string AttributeNewLineString
            {
                get => 
                    this.newLineString;
                set => 
                    (this.newLineString = value);
            }

            public int MaxLineLength
            {
                get => 
                    this.maxLineLength;
                set => 
                    (this.maxLineLength = value);
            }

            public override System.Text.Encoding Encoding =>
                this.baseWriter.Encoding;

            private enum State
            {
                Writing,
                WaitingForAttributeLeadingSpace,
                ReadingAttribute,
                Buffering,
                FlushingBuffer,
                WritingComment
            }
        }
    }
}

