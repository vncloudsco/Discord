namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml;

    internal class XmlAttributePreservationDict
    {
        private List<string> orderedAttributes = new List<string>();
        private Dictionary<string, string> leadingSpaces = new Dictionary<string, string>();
        private string attributeNewLineString;
        private bool computedOneAttributePerLine;
        private bool oneAttributePerLine;

        private string ComputeAttributeNewLineString(XmlFormatter formatter)
        {
            string str = this.LookAheadForNewLineString();
            return ((str == null) ? formatter?.CurrentAttributeIndent : str);
        }

        private bool ComputeOneAttributePerLine()
        {
            bool flag2;
            if (this.leadingSpaces.Count <= 1)
            {
                return false;
            }
            bool flag = true;
            using (List<string>.Enumerator enumerator = this.orderedAttributes.GetEnumerator())
            {
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        string current = enumerator.Current;
                        if (flag)
                        {
                            flag = false;
                            continue;
                        }
                        if (!this.leadingSpaces.ContainsKey(current) || this.ContainsNewLine(this.leadingSpaces[current]))
                        {
                            continue;
                        }
                        flag2 = false;
                    }
                    else
                    {
                        return true;
                    }
                    break;
                }
            }
            return flag2;
        }

        private bool ContainsNewLine(string space) => 
            (space.IndexOf("\n", StringComparison.Ordinal) >= 0);

        private void EnsureAttributeNewLineString(XmlFormatter formatter)
        {
            this.GetAttributeNewLineString(formatter);
        }

        private int EnumerateAttributes(string elementStartTag, Action<int, int, string> onAttributeSpotted)
        {
            bool flag = elementStartTag.EndsWith("/>", StringComparison.Ordinal);
            string s = elementStartTag;
            if (!flag)
            {
                s = elementStartTag.Substring(0, elementStartTag.Length - 1) + "/>";
            }
            XmlTextReader reader = new XmlTextReader(new StringReader(s)) {
                Namespaces = false
            };
            reader.Read();
            for (bool flag2 = reader.MoveToFirstAttribute(); flag2; flag2 = reader.MoveToNextAttribute())
            {
                onAttributeSpotted(reader.LineNumber, reader.LinePosition, reader.Name);
            }
            int length = elementStartTag.Length;
            if (flag)
            {
                length--;
            }
            return length;
        }

        public string GetAttributeNewLineString(XmlFormatter formatter)
        {
            if (this.attributeNewLineString == null)
            {
                this.attributeNewLineString = this.ComputeAttributeNewLineString(formatter);
            }
            return this.attributeNewLineString;
        }

        private string LookAheadForNewLineString()
        {
            string str2;
            using (Dictionary<string, string>.ValueCollection.Enumerator enumerator = this.leadingSpaces.Values.GetEnumerator())
            {
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        string current = enumerator.Current;
                        if (!this.ContainsNewLine(current))
                        {
                            continue;
                        }
                        str2 = current;
                    }
                    else
                    {
                        return null;
                    }
                    break;
                }
            }
            return str2;
        }

        internal void ReadPreservationInfo(string elementStartTag)
        {
            WhitespaceTrackingTextReader whitespaceReader = new WhitespaceTrackingTextReader(new StringReader(elementStartTag));
            int characterPosition = this.EnumerateAttributes(elementStartTag, delegate (int line, int linePosition, string attributeName) {
                this.orderedAttributes.Add(attributeName);
                if (whitespaceReader.ReadToPosition(line, linePosition))
                {
                    this.leadingSpaces.Add(attributeName, whitespaceReader.PrecedingWhitespace);
                }
            });
            if (whitespaceReader.ReadToPosition(characterPosition))
            {
                this.leadingSpaces.Add(string.Empty, whitespaceReader.PrecedingWhitespace);
            }
        }

        internal void UpdatePreservationInfo(XmlAttributeCollection updatedAttributes, XmlFormatter formatter)
        {
            if (updatedAttributes.Count == 0)
            {
                if (this.orderedAttributes.Count > 0)
                {
                    this.leadingSpaces.Clear();
                    this.orderedAttributes.Clear();
                }
            }
            else
            {
                Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
                foreach (string str in this.orderedAttributes)
                {
                    dictionary[str] = false;
                }
                foreach (XmlAttribute attribute in updatedAttributes)
                {
                    if (!dictionary.ContainsKey(attribute.Name))
                    {
                        this.orderedAttributes.Add(attribute.Name);
                    }
                    dictionary[attribute.Name] = true;
                }
                bool flag = true;
                string str2 = null;
                foreach (string str3 in this.orderedAttributes)
                {
                    bool flag2 = dictionary[str3];
                    if (!flag2)
                    {
                        if (this.leadingSpaces.ContainsKey(str3))
                        {
                            string space = this.leadingSpaces[str3];
                            if (flag)
                            {
                                if (str2 == null)
                                {
                                    str2 = space;
                                }
                            }
                            else if (this.ContainsNewLine(space))
                            {
                                str2 = space;
                            }
                            this.leadingSpaces.Remove(str3);
                        }
                    }
                    else if (str2 != null)
                    {
                        if (flag || (!this.leadingSpaces.ContainsKey(str3) || !this.ContainsNewLine(this.leadingSpaces[str3])))
                        {
                            this.leadingSpaces[str3] = str2;
                        }
                        str2 = null;
                    }
                    else if (!this.leadingSpaces.ContainsKey(str3))
                    {
                        if (flag)
                        {
                            this.leadingSpaces[str3] = " ";
                        }
                        else if (this.OneAttributePerLine)
                        {
                            this.leadingSpaces[str3] = this.GetAttributeNewLineString(formatter);
                        }
                        else
                        {
                            this.EnsureAttributeNewLineString(formatter);
                        }
                    }
                    flag = flag && !flag2;
                }
            }
        }

        internal void WritePreservedAttributes(XmlAttributePreservingWriter writer, XmlAttributeCollection attributes)
        {
            string newLineString = null;
            if (this.attributeNewLineString != null)
            {
                newLineString = writer.SetAttributeNewLineString(this.attributeNewLineString);
            }
            try
            {
                foreach (string str2 in this.orderedAttributes)
                {
                    XmlAttribute attribute = attributes[str2];
                    if (attribute != null)
                    {
                        if (this.leadingSpaces.ContainsKey(str2))
                        {
                            writer.WriteAttributeWhitespace(this.leadingSpaces[str2]);
                        }
                        attribute.WriteTo(writer);
                    }
                }
                if (this.leadingSpaces.ContainsKey(string.Empty))
                {
                    writer.WriteAttributeTrailingWhitespace(this.leadingSpaces[string.Empty]);
                }
            }
            finally
            {
                if (newLineString != null)
                {
                    writer.SetAttributeNewLineString(newLineString);
                }
            }
        }

        private bool OneAttributePerLine
        {
            get
            {
                if (!this.computedOneAttributePerLine)
                {
                    this.computedOneAttributePerLine = true;
                    this.oneAttributePerLine = this.ComputeOneAttributePerLine();
                }
                return this.oneAttributePerLine;
            }
        }
    }
}

