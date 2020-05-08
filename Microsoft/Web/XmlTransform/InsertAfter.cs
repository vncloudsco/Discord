namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Globalization;

    internal class InsertAfter : InsertBase
    {
        protected override void Apply()
        {
            base.SiblingElement.ParentNode.InsertAfter(base.TransformNode, base.SiblingElement);
            object[] args = new object[] { base.TransformNode.Name };
            base.Log.LogMessage(MessageType.Verbose, string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_TransformMessageInsert, args), new object[0]);
        }
    }
}

