namespace Microsoft.Web.XmlTransform
{
    using System;

    internal class Insert : Transform
    {
        public Insert() : base(TransformFlags.UseParentAsTargetNode, MissingTargetMessage.Error)
        {
        }

        protected override void Apply()
        {
            CommonErrors.ExpectNoArguments(base.Log, base.TransformNameShort, base.ArgumentString);
            base.TargetNode.AppendChild(base.TransformNode);
            object[] messageArgs = new object[] { base.TransformNode.Name };
            base.Log.LogMessage(MessageType.Verbose, SR.XMLTRANSFORMATION_TransformMessageInsert, messageArgs);
        }
    }
}

