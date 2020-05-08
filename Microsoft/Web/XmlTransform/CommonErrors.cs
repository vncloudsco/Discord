namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Xml;

    internal static class CommonErrors
    {
        internal static void ExpectNoArguments(XmlTransformationLogger log, string transformName, string argumentString)
        {
            if (!string.IsNullOrEmpty(argumentString))
            {
                object[] messageArgs = new object[] { transformName };
                log.LogWarning(SR.XMLTRANSFORMATION_TransformDoesNotExpectArguments, messageArgs);
            }
        }

        internal static void WarnIfMultipleTargets(XmlTransformationLogger log, string transformName, XmlNodeList targetNodes, bool applyTransformToAllTargets)
        {
            if (targetNodes.Count > 1)
            {
                object[] messageArgs = new object[] { transformName };
                log.LogWarning(SR.XMLTRANSFORMATION_TransformOnlyAppliesOnce, messageArgs);
            }
        }
    }
}

