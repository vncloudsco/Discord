namespace Microsoft.Web.XmlTransform
{
    using System;

    internal class RemoveAll : Remove
    {
        public RemoveAll()
        {
            base.ApplyTransformToAllTargetNodes = true;
        }

        protected override void Apply()
        {
            base.RemoveNode();
        }
    }
}

