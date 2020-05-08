namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.IO;
    using System.Text;
    using System.Xml;

    internal class XmlFileInfoDocument : XmlDocument, IDisposable
    {
        private Encoding _textEncoding;
        private XmlTextReader _reader;
        private XmlAttributePreservationProvider _preservationProvider;
        private bool _firstLoad = true;
        private string _fileName;
        private int _lineNumberOffset;
        private int _linePositionOffset;

        internal XmlNode CloneNodeFromOtherDocument(XmlNode element)
        {
            XmlTextReader reader = this._reader;
            string str = this._fileName;
            XmlNode node = null;
            try
            {
                IXmlLineInfo info = element as IXmlLineInfo;
                if (info == null)
                {
                    this._fileName = null;
                    this._reader = null;
                    node = this.ReadNode(new XmlTextReader(new StringReader(element.OuterXml)));
                }
                else
                {
                    this._reader = new XmlTextReader(new StringReader(element.OuterXml));
                    this._lineNumberOffset = info.LineNumber - 1;
                    this._linePositionOffset = info.LinePosition - 2;
                    this._fileName = element.OwnerDocument.BaseURI;
                    node = this.ReadNode(this._reader);
                }
            }
            finally
            {
                this._lineNumberOffset = 0;
                this._linePositionOffset = 0;
                this._fileName = str;
                this._reader = reader;
            }
            return node;
        }

        public override XmlAttribute CreateAttribute(string prefix, string localName, string namespaceURI) => 
            (!this.HasErrorInfo ? base.CreateAttribute(prefix, localName, namespaceURI) : new XmlFileInfoAttribute(prefix, localName, namespaceURI, this));

        public override XmlElement CreateElement(string prefix, string localName, string namespaceURI) => 
            (!this.HasErrorInfo ? base.CreateElement(prefix, localName, namespaceURI) : new XmlFileInfoElement(prefix, localName, namespaceURI, this));

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._reader != null)
            {
                this._reader.Close();
                this._reader = null;
            }
            if (this._preservationProvider != null)
            {
                this._preservationProvider.Close();
                this._preservationProvider = null;
            }
        }

        ~XmlFileInfoDocument()
        {
            this.Dispose(false);
        }

        private XmlElement FindContainingElement(XmlNode node)
        {
            while ((node != null) && !(node is XmlElement))
            {
                node = node.ParentNode;
            }
            return (node as XmlElement);
        }

        private Encoding GetEncodingFromStream(Stream stream)
        {
            Encoding bigEndianUnicode = null;
            if (stream.CanSeek)
            {
                byte[] buffer = new byte[3];
                stream.Read(buffer, 0, buffer.Length);
                if ((buffer[0] == 0xef) && ((buffer[1] == 0xbb) && (buffer[2] == 0xbf)))
                {
                    bigEndianUnicode = Encoding.UTF8;
                }
                else if ((buffer[0] == 0xfe) && (buffer[1] == 0xff))
                {
                    bigEndianUnicode = Encoding.BigEndianUnicode;
                }
                else if ((buffer[0] == 0xff) && (buffer[1] == 0xfe))
                {
                    bigEndianUnicode = Encoding.Unicode;
                }
                else if ((buffer[0] == 0x2b) && ((buffer[1] == 0x2f) && (buffer[2] == 0x76)))
                {
                    bigEndianUnicode = Encoding.UTF7;
                }
                stream.Seek(0L, SeekOrigin.Begin);
            }
            return bigEndianUnicode;
        }

        internal bool IsNewNode(XmlNode node)
        {
            XmlFileInfoElement element = this.FindContainingElement(node) as XmlFileInfoElement;
            return ((element != null) && !element.IsOriginal);
        }

        public override void Load(string filename)
        {
            this.LoadFromFileName(filename);
            this._firstLoad = false;
        }

        public override void Load(XmlReader reader)
        {
            this._reader = reader as XmlTextReader;
            if (this._reader != null)
            {
                this._fileName = this._reader.BaseURI;
            }
            base.Load(reader);
            if (this._reader != null)
            {
                this._textEncoding = this._reader.Encoding;
            }
            this._firstLoad = false;
        }

        private void LoadFromFileName(string filename)
        {
            this._fileName = filename;
            StreamReader textReader = null;
            try
            {
                if (base.PreserveWhitespace)
                {
                    this._preservationProvider = new XmlAttributePreservationProvider(filename);
                }
                textReader = new StreamReader(filename, true);
                this.LoadFromTextReader(textReader);
            }
            finally
            {
                if (this._preservationProvider != null)
                {
                    this._preservationProvider.Close();
                    this._preservationProvider = null;
                }
                if (textReader != null)
                {
                    textReader.Close();
                }
            }
        }

        private void LoadFromTextReader(TextReader textReader)
        {
            StreamReader reader = textReader as StreamReader;
            if (reader != null)
            {
                FileStream baseStream = reader.BaseStream as FileStream;
                if (baseStream != null)
                {
                    this._fileName = baseStream.Name;
                }
                this._textEncoding = this.GetEncodingFromStream(reader.BaseStream);
            }
            this._reader = new XmlTextReader(this._fileName, textReader);
            base.Load(this._reader);
            if (this._textEncoding == null)
            {
                this._textEncoding = this._reader.Encoding;
            }
        }

        public override void Save(Stream w)
        {
            XmlWriter writer = null;
            try
            {
                if (base.PreserveWhitespace)
                {
                    XmlFormatter.Format(this);
                    writer = new XmlAttributePreservingWriter(w, this.TextEncoding);
                }
                else
                {
                    XmlTextWriter writer2 = new XmlTextWriter(w, this.TextEncoding) {
                        Formatting = Formatting.Indented
                    };
                    writer = writer2;
                }
                this.WriteTo(writer);
            }
            finally
            {
                if (writer != null)
                {
                    writer.Flush();
                }
            }
        }

        public override void Save(string filename)
        {
            XmlWriter w = null;
            try
            {
                if (base.PreserveWhitespace)
                {
                    XmlFormatter.Format(this);
                    w = new XmlAttributePreservingWriter(filename, this.TextEncoding);
                }
                else
                {
                    XmlTextWriter writer2 = new XmlTextWriter(filename, this.TextEncoding) {
                        Formatting = Formatting.Indented
                    };
                    w = writer2;
                }
                this.WriteTo(w);
            }
            finally
            {
                if (w != null)
                {
                    w.Flush();
                    w.Close();
                }
            }
        }

        internal bool HasErrorInfo =>
            !ReferenceEquals(this._reader, null);

        internal string FileName =>
            this._fileName;

        private int CurrentLineNumber =>
            ((this._reader != null) ? (this._reader.LineNumber + this._lineNumberOffset) : 0);

        private int CurrentLinePosition =>
            ((this._reader != null) ? (this._reader.LinePosition + this._linePositionOffset) : 0);

        private bool FirstLoad =>
            this._firstLoad;

        private XmlAttributePreservationProvider PreservationProvider =>
            this._preservationProvider;

        private Encoding TextEncoding
        {
            get
            {
                if (this._textEncoding != null)
                {
                    return this._textEncoding;
                }
                if (this.HasChildNodes)
                {
                    XmlDeclaration firstChild = this.FirstChild as XmlDeclaration;
                    if (firstChild != null)
                    {
                        string encoding = firstChild.Encoding;
                        if (encoding.Length > 0)
                        {
                            return Encoding.GetEncoding(encoding);
                        }
                    }
                }
                return null;
            }
        }

        private class XmlFileInfoAttribute : XmlAttribute, IXmlLineInfo
        {
            private int lineNumber;
            private int linePosition;

            internal XmlFileInfoAttribute(string prefix, string localName, string namespaceUri, XmlFileInfoDocument document) : base(prefix, localName, namespaceUri, document)
            {
                this.lineNumber = document.CurrentLineNumber;
                this.linePosition = document.CurrentLinePosition;
            }

            public bool HasLineInfo() => 
                true;

            public int LineNumber =>
                this.lineNumber;

            public int LinePosition =>
                this.linePosition;
        }

        private class XmlFileInfoElement : XmlElement, IXmlLineInfo, IXmlFormattableAttributes
        {
            private int lineNumber;
            private int linePosition;
            private bool isOriginal;
            private XmlAttributePreservationDict preservationDict;

            internal XmlFileInfoElement(string prefix, string localName, string namespaceUri, XmlFileInfoDocument document) : base(prefix, localName, namespaceUri, document)
            {
                this.lineNumber = document.CurrentLineNumber;
                this.linePosition = document.CurrentLinePosition;
                this.isOriginal = document.FirstLoad;
                if (document.PreservationProvider != null)
                {
                    this.preservationDict = document.PreservationProvider.GetDictAtPosition(this.lineNumber, this.linePosition - 1);
                }
                if (this.preservationDict == null)
                {
                    this.preservationDict = new XmlAttributePreservationDict();
                }
            }

            public bool HasLineInfo() => 
                true;

            void IXmlFormattableAttributes.FormatAttributes(XmlFormatter formatter)
            {
                this.preservationDict.UpdatePreservationInfo(this.Attributes, formatter);
            }

            private void WriteAttributesTo(XmlWriter w)
            {
                XmlAttributeCollection attributes = this.Attributes;
                for (int i = 0; i < attributes.Count; i++)
                {
                    attributes[i].WriteTo(w);
                }
            }

            private void WritePreservedAttributesTo(XmlAttributePreservingWriter preservingWriter)
            {
                this.preservationDict.WritePreservedAttributes(preservingWriter, this.Attributes);
            }

            public override void WriteTo(XmlWriter w)
            {
                string prefix = this.Prefix;
                if (!string.IsNullOrEmpty(this.NamespaceURI))
                {
                    prefix = w.LookupPrefix(this.NamespaceURI);
                    if (prefix == null)
                    {
                        prefix = this.Prefix;
                    }
                }
                w.WriteStartElement(prefix, this.LocalName, this.NamespaceURI);
                XmlAttributePreservingWriter preservingWriter = w as XmlAttributePreservingWriter;
                if ((preservingWriter != null) && (this.preservationDict != null))
                {
                    this.WritePreservedAttributesTo(preservingWriter);
                }
                else
                {
                    this.WriteAttributesTo(w);
                }
                if (base.IsEmpty)
                {
                    w.WriteEndElement();
                }
                else
                {
                    this.WriteContentTo(w);
                    w.WriteFullEndElement();
                }
            }

            public int LineNumber =>
                this.lineNumber;

            public int LinePosition =>
                this.linePosition;

            public bool IsOriginal =>
                this.isOriginal;

            string IXmlFormattableAttributes.AttributeIndent =>
                this.preservationDict.GetAttributeNewLineString(null);
        }
    }
}

