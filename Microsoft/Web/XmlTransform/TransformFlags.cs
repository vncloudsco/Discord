namespace Microsoft.Web.XmlTransform
{
    using System;

    [Flags]
    internal enum TransformFlags
    {
        None,
        ApplyTransformToAllTargetNodes,
        UseParentAsTargetNode
    }
}

