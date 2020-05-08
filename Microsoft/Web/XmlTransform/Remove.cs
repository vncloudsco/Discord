namespace Microsoft.Web.XmlTransform
{
    using System;

    internal class Remove : Transform
    {
        protected override void Apply()
        {
            CommonErrors.WarnIfMultipleTargets(base.Log, base.TransformNameShort, base.TargetNodes, base.ApplyTransformToAllTargetNodes);
            this.RemoveNode();
        }

        protected void RemoveNode()
        {
            CommonErrors.ExpectNoArguments(base.Log, base.TransformNameShort, base.ArgumentString);
            base.TargetNode.ParentNode.RemoveChild(base.TargetNode);
            object[] messageArgs = new object[] { base.TargetNode.Name };
            base.Log.LogMessage(MessageType.Verbose, SR.XMLTRANSFORMATION_TransformMessageRemove, messageArgs);
        }
    }
}

