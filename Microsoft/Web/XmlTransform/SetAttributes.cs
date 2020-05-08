namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Xml;

    internal class SetAttributes : AttributeTransform
    {
        protected override void Apply()
        {
            foreach (XmlAttribute attribute in base.TransformAttributes)
            {
                XmlAttribute namedItem = base.TargetNode.Attributes.GetNamedItem(attribute.Name) as XmlAttribute;
                if (namedItem != null)
                {
                    namedItem.Value = attribute.Value;
                }
                else
                {
                    base.TargetNode.Attributes.Append((XmlAttribute) attribute.Clone());
                }
                object[] messageArgs = new object[] { attribute.Name };
                base.Log.LogMessage(MessageType.Verbose, SR.XMLTRANSFORMATION_TransformMessageSetAttribute, messageArgs);
            }
            if (base.TransformAttributes.Count <= 0)
            {
                base.Log.LogWarning(SR.XMLTRANSFORMATION_TransformMessageNoSetAttributes, new object[0]);
            }
            else
            {
                object[] messageArgs = new object[] { base.TransformAttributes.Count };
                base.Log.LogMessage(MessageType.Verbose, SR.XMLTRANSFORMATION_TransformMessageSetAttributes, messageArgs);
            }
        }
    }
}

