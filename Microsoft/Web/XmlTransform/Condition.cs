namespace Microsoft.Web.XmlTransform
{
    using System;

    internal sealed class Condition : Locator
    {
        protected override string ConstructPredicate()
        {
            base.EnsureArguments(1, 1);
            return base.Arguments[0];
        }
    }
}

