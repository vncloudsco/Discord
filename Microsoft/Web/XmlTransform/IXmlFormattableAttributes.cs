namespace Microsoft.Web.XmlTransform
{
    using System;

    internal interface IXmlFormattableAttributes
    {
        void FormatAttributes(XmlFormatter formatter);

        string AttributeIndent { get; }
    }
}

