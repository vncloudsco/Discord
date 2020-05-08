namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Globalization;
    using System.Xml;

    internal sealed class Match : Locator
    {
        protected override string ConstructPredicate()
        {
            base.EnsureArguments(1);
            string str = null;
            foreach (string str2 in base.Arguments)
            {
                XmlAttribute namedItem = base.CurrentElement.Attributes.GetNamedItem(str2) as XmlAttribute;
                if (namedItem == null)
                {
                    throw new XmlTransformationException(string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_MatchAttributeDoesNotExist, new object[] { str2 }));
                }
                object[] args = new object[] { namedItem.Name, namedItem.Value };
                string str3 = string.Format(CultureInfo.InvariantCulture, "@{0}='{1}'", args);
                str = (str != null) ? (str + " and " + str3) : str3;
            }
            return str;
        }
    }
}

