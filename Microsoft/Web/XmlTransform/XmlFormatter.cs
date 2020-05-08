namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml;

    internal class XmlFormatter
    {
        private XmlFileInfoDocument document;
        private string originalFileName;
        private LinkedList<string> indents = new LinkedList<string>();
        private LinkedList<string> attributeIndents = new LinkedList<string>();
        private string currentIndent = string.Empty;
        private string currentAttributeIndent;
        private string oneTab;
        private string defaultTab = "\t";
        private XmlNode currentNode;
        private XmlNode previousNode;

        private XmlFormatter(XmlFileInfoDocument document)
        {
            this.document = document;
            this.originalFileName = document.FileName;
        }

        private string ComputeCurrentAttributeIndent()
        {
            string str = this.LookForSiblingIndent(this.CurrentNode);
            return ((str == null) ? (this.CurrentIndent + this.OneTab) : str);
        }

        private string ComputeCurrentIndent()
        {
            string str = this.LookAheadForIndent();
            return ((str == null) ? (this.PreviousIndent + this.OneTab) : str);
        }

        private string ComputeOneTab()
        {
            if (this.indents.Count <= 0)
            {
                return this.DefaultTab;
            }
            LinkedListNode<string> last = this.indents.Last;
            for (LinkedListNode<string> node2 = last.Previous; node2 != null; node2 = last.Previous)
            {
                if (last.Value.StartsWith(node2.Value, StringComparison.Ordinal))
                {
                    return last.Value.Substring(node2.Value.Length);
                }
                last = node2;
            }
            return this.ConvertIndentToTab(last.Value);
        }

        private string ConvertIndentToTab(string indent)
        {
            for (int i = 0; i < (indent.Length - 1); i++)
            {
                char ch = indent[i];
                if ((ch != '\n') && (ch != '\r'))
                {
                    return indent.Substring(i + 1);
                }
            }
            return this.DefaultTab;
        }

        private int EnsureNodeIndent(XmlNode node, bool indentBeforeEnd)
        {
            int num = 0;
            if (this.NeedsIndent(node, this.PreviousNode))
            {
                if (indentBeforeEnd)
                {
                    this.InsertIndentBeforeEnd(node);
                }
                else
                {
                    this.InsertIndentBefore(node);
                    num = 1;
                }
            }
            return num;
        }

        private int FindLastNewLine(string whitespace)
        {
            int num = whitespace.Length - 1;
            while (true)
            {
                if (num < 0)
                {
                    return -1;
                }
                char ch = whitespace[num];
                switch (ch)
                {
                    case '\t':
                        break;

                    case '\n':
                        return (((num <= 0) || (whitespace[num - 1] != '\r')) ? num : (num - 1));

                    case '\v':
                    case '\f':
                        goto TR_0001;

                    case '\r':
                        return num;

                    default:
                        if (ch == ' ')
                        {
                            break;
                        }
                        goto TR_0001;
                }
                num--;
            }
        TR_0001:
            return -1;
        }

        public static void Format(XmlDocument document)
        {
            XmlFileInfoDocument parentNode = document as XmlFileInfoDocument;
            if (parentNode != null)
            {
                new XmlFormatter(parentNode).FormatLoop(parentNode);
            }
        }

        private void FormatAttributes(XmlNode node)
        {
            IXmlFormattableAttributes attributes = node as IXmlFormattableAttributes;
            if (attributes != null)
            {
                attributes.FormatAttributes(this);
            }
        }

        private void FormatLoop(XmlNode parentNode)
        {
            for (int i = 0; i < parentNode.ChildNodes.Count; i++)
            {
                XmlNode node = parentNode.ChildNodes[i];
                this.CurrentNode = node;
                XmlNodeType nodeType = node.NodeType;
                switch (nodeType)
                {
                    case XmlNodeType.Element:
                        i += this.HandleElement(node);
                        break;

                    case XmlNodeType.Entity:
                    case XmlNodeType.Comment:
                        i += this.EnsureNodeIndent(node, false);
                        break;

                    case XmlNodeType.Whitespace:
                        i += this.HandleWhiteSpace(node);
                        break;

                    default:
                        break;
                }
            }
        }

        private string GetIndentFromWhiteSpace(XmlNode node)
        {
            string outerXml = node.OuterXml;
            int startIndex = this.FindLastNewLine(outerXml);
            return ((startIndex < 0) ? null : outerXml.Substring(startIndex));
        }

        private int HandleElement(XmlNode node)
        {
            int num = this.HandleStartElement(node);
            this.ReorderNewItemsAtEnd(node);
            this.FormatLoop(node);
            this.CurrentNode = node;
            return (num + this.HandleEndElement(node));
        }

        private int HandleEndElement(XmlNode node)
        {
            int num = 0;
            this.PopIndent();
            if (!((XmlElement) node).IsEmpty)
            {
                num = this.EnsureNodeIndent(node, true);
            }
            return num;
        }

        private int HandleStartElement(XmlNode node)
        {
            int num = this.EnsureNodeIndent(node, false);
            this.FormatAttributes(node);
            this.PushIndent();
            return num;
        }

        private int HandleWhiteSpace(XmlNode node)
        {
            int num = 0;
            if (this.IsWhiteSpace(this.PreviousNode))
            {
                XmlNode previousNode = this.PreviousNode;
                if ((this.FindLastNewLine(node.OuterXml) < 0) && (this.FindLastNewLine(this.PreviousNode.OuterXml) >= 0))
                {
                    previousNode = node;
                }
                previousNode.ParentNode.RemoveChild(previousNode);
                num = -1;
            }
            string indentFromWhiteSpace = this.GetIndentFromWhiteSpace(node);
            if (indentFromWhiteSpace != null)
            {
                this.SetIndent(indentFromWhiteSpace);
            }
            return num;
        }

        private void InsertIndentBefore(XmlNode node)
        {
            node.ParentNode.InsertBefore(this.document.CreateWhitespace(this.CurrentIndent), node);
        }

        private void InsertIndentBeforeEnd(XmlNode node)
        {
            node.AppendChild(this.document.CreateWhitespace(this.CurrentIndent));
        }

        private bool IsNewNode(XmlNode node) => 
            ((node != null) && this.document.IsNewNode(node));

        public bool IsText(XmlNode node) => 
            ((node != null) && (node.NodeType == XmlNodeType.Text));

        private bool IsWhiteSpace(XmlNode node) => 
            ((node != null) && (node.NodeType == XmlNodeType.Whitespace));

        private string LookAheadForIndent()
        {
            string str2;
            if (this.currentNode.ParentNode == null)
            {
                return null;
            }
            using (IEnumerator enumerator = this.currentNode.ParentNode.ChildNodes.GetEnumerator())
            {
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        XmlNode current = (XmlNode) enumerator.Current;
                        if (!this.IsWhiteSpace(current) || (current.NextSibling == null))
                        {
                            continue;
                        }
                        string outerXml = current.OuterXml;
                        int startIndex = this.FindLastNewLine(outerXml);
                        if (startIndex < 0)
                        {
                            continue;
                        }
                        str2 = outerXml.Substring(startIndex);
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

        private string LookForSiblingIndent(XmlNode currentNode)
        {
            string str2;
            bool flag = true;
            string attributeIndent = null;
            using (IEnumerator enumerator = currentNode.ParentNode.ChildNodes.GetEnumerator())
            {
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        XmlNode current = (XmlNode) enumerator.Current;
                        if (ReferenceEquals(current, currentNode))
                        {
                            flag = false;
                        }
                        else
                        {
                            IXmlFormattableAttributes attributes = current as IXmlFormattableAttributes;
                            if (attributes != null)
                            {
                                attributeIndent = attributes.AttributeIndent;
                            }
                        }
                        if (flag || (attributeIndent == null))
                        {
                            continue;
                        }
                        str2 = attributeIndent;
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

        private bool NeedsIndent(XmlNode node, XmlNode previousNode) => 
            (!this.IsWhiteSpace(previousNode) && (!this.IsText(previousNode) && (this.IsNewNode(node) || this.IsNewNode(previousNode))));

        private void PopIndent()
        {
            if (this.indents.Count <= 0)
            {
                throw new InvalidOperationException();
            }
            this.currentIndent = this.indents.Last.Value;
            this.indents.RemoveLast();
            this.currentAttributeIndent = this.attributeIndents.Last.Value;
            this.attributeIndents.RemoveLast();
        }

        private void PushIndent()
        {
            this.indents.AddLast(new LinkedListNode<string>(this.CurrentIndent));
            this.currentIndent = null;
            this.attributeIndents.AddLast(new LinkedListNode<string>(this.currentAttributeIndent));
            this.currentAttributeIndent = null;
        }

        private void ReorderNewItemsAtEnd(XmlNode node)
        {
            if (!this.IsNewNode(node))
            {
                XmlNode lastChild = node.LastChild;
                if ((lastChild != null) && (lastChild.NodeType != XmlNodeType.Whitespace))
                {
                    XmlNode oldChild = null;
                    while (true)
                    {
                        if (lastChild != null)
                        {
                            XmlNodeType nodeType = lastChild.NodeType;
                            if (nodeType != XmlNodeType.Element)
                            {
                                if (nodeType == XmlNodeType.Whitespace)
                                {
                                    oldChild = lastChild;
                                }
                            }
                            else if (this.IsNewNode(lastChild))
                            {
                                lastChild = lastChild.PreviousSibling;
                                continue;
                            }
                        }
                        if (oldChild != null)
                        {
                            node.RemoveChild(oldChild);
                            node.AppendChild(oldChild);
                        }
                        break;
                    }
                }
            }
        }

        private void SetIndent(string indent)
        {
            if ((this.currentIndent == null) || !this.currentIndent.Equals(indent))
            {
                this.currentIndent = indent;
                this.oneTab = null;
                this.currentAttributeIndent = null;
            }
        }

        private XmlNode CurrentNode
        {
            get => 
                this.currentNode;
            set
            {
                this.previousNode = this.currentNode;
                this.currentNode = value;
            }
        }

        private XmlNode PreviousNode =>
            this.previousNode;

        private string PreviousIndent =>
            this.indents.Last.Value;

        private string CurrentIndent
        {
            get
            {
                if (this.currentIndent == null)
                {
                    this.currentIndent = this.ComputeCurrentIndent();
                }
                return this.currentIndent;
            }
        }

        public string CurrentAttributeIndent
        {
            get
            {
                if (this.currentAttributeIndent == null)
                {
                    this.currentAttributeIndent = this.ComputeCurrentAttributeIndent();
                }
                return this.currentAttributeIndent;
            }
        }

        private string OneTab
        {
            get
            {
                if (this.oneTab == null)
                {
                    this.oneTab = this.ComputeOneTab();
                }
                return this.oneTab;
            }
        }

        public string DefaultTab
        {
            get => 
                this.defaultTab;
            set => 
                (this.defaultTab = value);
        }
    }
}

