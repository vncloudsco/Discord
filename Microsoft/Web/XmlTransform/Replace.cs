namespace Microsoft.Web.XmlTransform
{
    using System;

    internal class Replace : Transform
    {
        protected override void Apply()
        {
            CommonErrors.ExpectNoArguments(base.Log, base.TransformNameShort, base.ArgumentString);
            CommonErrors.WarnIfMultipleTargets(base.Log, base.TransformNameShort, base.TargetNodes, base.ApplyTransformToAllTargetNodes);
            base.TargetNode.ParentNode.ReplaceChild(base.TransformNode, base.TargetNode);
            object[] messageArgs = new object[] { base.TargetNode.Name };
            base.Log.LogMessage(MessageType.Verbose, SR.XMLTRANSFORMATION_TransformMessageReplace, messageArgs);
        }
    }
}

