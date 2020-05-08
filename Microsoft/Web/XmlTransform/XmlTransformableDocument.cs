namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Xml;

    internal class XmlTransformableDocument : XmlFileInfoDocument, IXmlOriginalDocumentService
    {
        private XmlDocument xmlOriginal;

        private void CloneOriginalDocument()
        {
            this.xmlOriginal = (XmlDocument) this.Clone();
        }

        private bool IsXmlEqual(XmlDocument xmlOriginal, XmlDocument xmlTransformed) => 
            false;

        XmlNodeList IXmlOriginalDocumentService.SelectNodes(string xpath, XmlNamespaceManager nsmgr) => 
            this.xmlOriginal?.SelectNodes(xpath, nsmgr);

        internal void OnAfterChange()
        {
        }

        internal void OnBeforeChange()
        {
            if (this.xmlOriginal == null)
            {
                this.CloneOriginalDocument();
            }
        }

        public bool IsChanged =>
            ((this.xmlOriginal != null) ? !this.IsXmlEqual(this.xmlOriginal, this) : false);
    }
}

