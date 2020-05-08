namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Xml;

    internal class XmlNodeContext
    {
        private XmlNode node;

        public XmlNodeContext(XmlNode node)
        {
            this.node = node;
        }

        public XmlNode Node =>
            this.node;

        public bool HasLineInfo =>
            (this.node is IXmlLineInfo);

        public int LineNumber
        {
            get
            {
                IXmlLineInfo node = this.node as IXmlLineInfo;
                return ((node == null) ? 0 : node.LineNumber);
            }
        }

        public int LinePosition
        {
            get
            {
                IXmlLineInfo node = this.node as IXmlLineInfo;
                return ((node == null) ? 0 : node.LinePosition);
            }
        }
    }
}

