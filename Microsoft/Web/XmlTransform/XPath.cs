namespace Microsoft.Web.XmlTransform
{
    using System;

    internal sealed class XPath : Locator
    {
        protected override string ConstructPath()
        {
            base.EnsureArguments(1, 1);
            string basePath = base.Arguments[0];
            if (!basePath.StartsWith("/", StringComparison.Ordinal))
            {
                basePath = base.AppendStep(base.ParentPath, this.NextStepNodeTest);
                basePath = base.AppendStep(basePath, base.Arguments[0]).Replace("/./", "/");
            }
            return basePath;
        }

        protected override string ParentPath =>
            this.ConstructPath();
    }
}

