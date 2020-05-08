namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    internal abstract class AttributeTransform : Transform
    {
        private XmlNode transformAttributeSource;
        private XmlNodeList transformAttributes;
        private XmlNode targetAttributeSource;
        private XmlNodeList targetAttributes;

        protected AttributeTransform() : base(TransformFlags.ApplyTransformToAllTargetNodes)
        {
        }

        private XmlNodeList GetAttributesFrom(XmlNode node)
        {
            if ((base.Arguments == null) || (base.Arguments.Count == 0))
            {
                return this.GetAttributesFrom(node, "*", false);
            }
            if (base.Arguments.Count == 1)
            {
                return this.GetAttributesFrom(node, base.Arguments[0], true);
            }
            foreach (string str in base.Arguments)
            {
                this.GetAttributesFrom(node, str, true);
            }
            return this.GetAttributesFrom(node, base.Arguments, false);
        }

        private XmlNodeList GetAttributesFrom(XmlNode node, IList<string> arguments, bool warnIfEmpty)
        {
            string[] array = new string[arguments.Count];
            arguments.CopyTo(array, 0);
            string xpath = "@" + string.Join("|@", array);
            XmlNodeList list = node.SelectNodes(xpath);
            if ((list.Count == 0) && (warnIfEmpty && (arguments.Count == 1)))
            {
                object[] messageArgs = new object[] { arguments[0] };
                base.Log.LogWarning(SR.XMLTRANSFORMATION_TransformArgumentFoundNoAttributes, messageArgs);
            }
            return list;
        }

        private XmlNodeList GetAttributesFrom(XmlNode node, string argument, bool warnIfEmpty)
        {
            string[] arguments = new string[] { argument };
            return this.GetAttributesFrom(node, arguments, warnIfEmpty);
        }

        protected XmlNodeList TransformAttributes
        {
            get
            {
                if ((this.transformAttributes == null) || !ReferenceEquals(this.transformAttributeSource, base.TransformNode))
                {
                    this.transformAttributeSource = base.TransformNode;
                    this.transformAttributes = this.GetAttributesFrom(base.TransformNode);
                }
                return this.transformAttributes;
            }
        }

        protected XmlNodeList TargetAttributes
        {
            get
            {
                if ((this.targetAttributes == null) || !ReferenceEquals(this.targetAttributeSource, base.TargetNode))
                {
                    this.targetAttributeSource = base.TargetNode;
                    this.targetAttributes = this.GetAttributesFrom(base.TargetNode);
                }
                return this.targetAttributes;
            }
        }
    }
}

