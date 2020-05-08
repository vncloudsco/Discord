namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Globalization;
    using System.Xml;

    internal abstract class InsertBase : Transform
    {
        private XmlElement siblingElement;

        internal InsertBase() : base(TransformFlags.UseParentAsTargetNode, MissingTargetMessage.Error)
        {
        }

        protected XmlElement SiblingElement
        {
            get
            {
                if (this.siblingElement == null)
                {
                    if ((base.Arguments == null) || (base.Arguments.Count == 0))
                    {
                        object[] args = new object[] { base.GetType().Name };
                        throw new XmlTransformationException(string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_InsertMissingArgument, args));
                    }
                    if (base.Arguments.Count > 1)
                    {
                        object[] args = new object[] { base.GetType().Name };
                        throw new XmlTransformationException(string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_InsertTooManyArguments, args));
                    }
                    string xpath = base.Arguments[0];
                    XmlNodeList list = base.TargetNode.SelectNodes(xpath);
                    if (list.Count == 0)
                    {
                        throw new XmlTransformationException(string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_InsertBadXPath, new object[] { xpath }));
                    }
                    this.siblingElement = list[0] as XmlElement;
                    if (this.siblingElement == null)
                    {
                        throw new XmlTransformationException(string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_InsertBadXPathResult, new object[] { xpath }));
                    }
                }
                return this.siblingElement;
            }
        }
    }
}

