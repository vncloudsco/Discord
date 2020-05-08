namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Xml;

    internal interface IXmlOriginalDocumentService
    {
        XmlNodeList SelectNodes(string path, XmlNamespaceManager nsmgr);
    }
}

