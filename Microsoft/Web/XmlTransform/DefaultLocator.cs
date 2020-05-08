namespace Microsoft.Web.XmlTransform
{
    internal sealed class DefaultLocator : Locator
    {
        private static DefaultLocator instance;

        internal static DefaultLocator Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DefaultLocator();
                }
                return instance;
            }
        }
    }
}

