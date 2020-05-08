namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Globalization;

    internal class InsertBefore : InsertBase
    {
        protected override void Apply()
        {
            base.SiblingElement.ParentNode.InsertBefore(base.TransformNode, base.SiblingElement);
            object[] args = new object[] { base.TransformNode.Name };
            base.Log.LogMessage(MessageType.Verbose, string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_TransformMessageInsert, args), new object[0]);
        }
    }
}

