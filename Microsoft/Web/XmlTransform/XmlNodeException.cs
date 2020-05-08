namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Xml;

    [Serializable]
    internal sealed class XmlNodeException : XmlTransformationException
    {
        private XmlFileInfoDocument document;
        private IXmlLineInfo lineInfo;

        public XmlNodeException(Exception innerException, XmlNode node) : base(innerException.Message, innerException)
        {
            this.lineInfo = node as IXmlLineInfo;
            this.document = node.OwnerDocument as XmlFileInfoDocument;
        }

        public XmlNodeException(string message, XmlNode node) : base(message)
        {
            this.lineInfo = node as IXmlLineInfo;
            this.document = node.OwnerDocument as XmlFileInfoDocument;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("document", this.document);
            info.AddValue("lineInfo", this.lineInfo);
        }

        public static Exception Wrap(Exception ex, XmlNode node) => 
            (!(ex is XmlNodeException) ? new XmlNodeException(ex, node) : ex);

        public bool HasErrorInfo =>
            !ReferenceEquals(this.lineInfo, null);

        public string FileName =>
            this.document?.FileName;

        public int LineNumber =>
            ((this.lineInfo != null) ? this.lineInfo.LineNumber : 0);

        public int LinePosition =>
            ((this.lineInfo != null) ? this.lineInfo.LinePosition : 0);
    }
}

