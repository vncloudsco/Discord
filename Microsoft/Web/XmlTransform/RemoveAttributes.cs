namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Xml;

    internal class RemoveAttributes : AttributeTransform
    {
        protected override void Apply()
        {
            foreach (XmlAttribute attribute in base.TargetAttributes)
            {
                base.TargetNode.Attributes.Remove(attribute);
                object[] messageArgs = new object[] { attribute.Name };
                base.Log.LogMessage(MessageType.Verbose, SR.XMLTRANSFORMATION_TransformMessageRemoveAttribute, messageArgs);
            }
            if (base.TargetAttributes.Count <= 0)
            {
                base.Log.LogWarning(base.TargetNode, SR.XMLTRANSFORMATION_TransformMessageNoRemoveAttributes, new object[0]);
            }
            else
            {
                object[] messageArgs = new object[] { base.TargetAttributes.Count };
                base.Log.LogMessage(MessageType.Verbose, SR.XMLTRANSFORMATION_TransformMessageRemoveAttributes, messageArgs);
            }
        }
    }
}

