namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml;

    internal abstract class Locator
    {
        private string argumentString;
        private IList<string> arguments;
        private string parentPath;
        private XmlElementContext context;
        private XmlTransformationLogger logger;

        protected Locator()
        {
        }

        protected string AppendStep(string basePath, string stepNodeTest) => 
            this.AppendStep(basePath, XPathAxis.Child, stepNodeTest, string.Empty);

        protected string AppendStep(string basePath, XPathAxis stepAxis, string stepNodeTest) => 
            this.AppendStep(basePath, stepAxis, stepNodeTest, string.Empty);

        protected string AppendStep(string basePath, string stepNodeTest, string predicate) => 
            this.AppendStep(basePath, XPathAxis.Child, stepNodeTest, predicate);

        protected string AppendStep(string basePath, XPathAxis stepAxis, string stepNodeTest, string predicate) => 
            (this.EnsureTrailingSlash(basePath) + this.GetAxisString(stepAxis) + stepNodeTest + this.EnsureBracketedPredicate(predicate));

        internal string ConstructParentPath(string parentPath, XmlElementContext context, string argumentString)
        {
            string str = string.Empty;
            if ((this.parentPath == null) && ((this.context == null) && (this.argumentString == null)))
            {
                try
                {
                    this.parentPath = parentPath;
                    this.context = context;
                    this.argumentString = argumentString;
                    str = this.ParentPath;
                }
                finally
                {
                    this.parentPath = null;
                    this.context = null;
                    this.argumentString = null;
                    this.arguments = null;
                    this.ReleaseLogger();
                }
            }
            return str;
        }

        protected virtual string ConstructPath() => 
            this.AppendStep(this.ParentPath, this.NextStepAxis, this.NextStepNodeTest, this.ConstructPredicate());

        internal string ConstructPath(string parentPath, XmlElementContext context, string argumentString)
        {
            string str = string.Empty;
            if ((this.parentPath == null) && ((this.context == null) && (this.argumentString == null)))
            {
                try
                {
                    this.parentPath = parentPath;
                    this.context = context;
                    this.argumentString = argumentString;
                    str = this.ConstructPath();
                }
                finally
                {
                    this.parentPath = null;
                    this.context = null;
                    this.argumentString = null;
                    this.arguments = null;
                    this.ReleaseLogger();
                }
            }
            return str;
        }

        protected virtual string ConstructPredicate() => 
            string.Empty;

        protected void EnsureArguments()
        {
            this.EnsureArguments(1);
        }

        protected void EnsureArguments(int min)
        {
            if ((this.Arguments == null) || (this.Arguments.Count < min))
            {
                object[] args = new object[] { base.GetType().Name, min };
                throw new XmlTransformationException(string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_RequiresMinimumArguments, args));
            }
        }

        protected void EnsureArguments(int min, int max)
        {
            if ((min == max) && ((this.Arguments == null) || (this.Arguments.Count != min)))
            {
                object[] args = new object[] { base.GetType().Name, min };
                throw new XmlTransformationException(string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_RequiresExactArguments, args));
            }
            this.EnsureArguments(min);
            if (this.Arguments.Count > max)
            {
                object[] args = new object[] { base.GetType().Name };
                throw new XmlTransformationException(string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_TooManyArguments, args));
            }
        }

        private string EnsureBracketedPredicate(string predicate)
        {
            if (string.IsNullOrEmpty(predicate))
            {
                return string.Empty;
            }
            if (!predicate.StartsWith("[", StringComparison.Ordinal))
            {
                predicate = "[" + predicate;
            }
            if (!predicate.EndsWith("]", StringComparison.Ordinal))
            {
                predicate = predicate + "]";
            }
            return predicate;
        }

        private string EnsureTrailingSlash(string basePath)
        {
            if (!basePath.EndsWith("/", StringComparison.Ordinal))
            {
                basePath = basePath + "/";
            }
            return basePath;
        }

        private string GetAxisString(XPathAxis stepAxis)
        {
            switch (stepAxis)
            {
                case XPathAxis.Child:
                    return string.Empty;

                case XPathAxis.Descendant:
                    return "descendant::";

                case XPathAxis.Parent:
                    return "parent::";

                case XPathAxis.Ancestor:
                    return "ancestor::";

                case XPathAxis.FollowingSibling:
                    return "following-sibling::";

                case XPathAxis.PrecedingSibling:
                    return "preceding-sibling::";

                case XPathAxis.Following:
                    return "following::";

                case XPathAxis.Preceding:
                    return "preceding::";

                case XPathAxis.Self:
                    return "self::";

                case XPathAxis.DescendantOrSelf:
                    return "/";

                case XPathAxis.AncestorOrSelf:
                    return "ancestor-or-self::";
            }
            return string.Empty;
        }

        private void ReleaseLogger()
        {
            if (this.logger != null)
            {
                this.logger.CurrentReferenceNode = null;
                this.logger = null;
            }
        }

        protected virtual string ParentPath =>
            this.parentPath;

        protected XmlNode CurrentElement =>
            this.context.Element;

        protected virtual string NextStepNodeTest =>
            ((string.IsNullOrEmpty(this.CurrentElement.NamespaceURI) || !string.IsNullOrEmpty(this.CurrentElement.Prefix)) ? this.CurrentElement.Name : ("_defaultNamespace:" + this.CurrentElement.LocalName));

        protected virtual XPathAxis NextStepAxis =>
            XPathAxis.Child;

        protected XmlTransformationLogger Log
        {
            get
            {
                if (this.logger == null)
                {
                    this.logger = this.context.GetService<XmlTransformationLogger>();
                    if (this.logger != null)
                    {
                        this.logger.CurrentReferenceNode = this.context.LocatorAttribute;
                    }
                }
                return this.logger;
            }
        }

        protected string ArgumentString =>
            this.argumentString;

        protected IList<string> Arguments
        {
            get
            {
                if ((this.arguments == null) && (this.argumentString != null))
                {
                    this.arguments = XmlArgumentUtility.SplitArguments(this.argumentString);
                }
                return this.arguments;
            }
        }
    }
}

