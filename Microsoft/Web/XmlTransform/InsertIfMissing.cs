namespace Microsoft.Web.XmlTransform
{
    using System;

    internal class InsertIfMissing : Insert
    {
        protected override void Apply()
        {
            CommonErrors.ExpectNoArguments(base.Log, base.TransformNameShort, base.ArgumentString);
            if ((base.TargetChildNodes == null) || (base.TargetChildNodes.Count == 0))
            {
                base.TargetNode.AppendChild(base.TransformNode);
                object[] messageArgs = new object[] { base.TransformNode.Name };
                base.Log.LogMessage(MessageType.Verbose, SR.XMLTRANSFORMATION_TransformMessageInsert, messageArgs);
            }
        }
    }
}

