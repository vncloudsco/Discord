namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Xml;

    internal class XmlElementContext : XmlNodeContext
    {
        private XmlElementContext parentContext;
        private string xpath;
        private string parentXPath;
        private XmlDocument xmlTargetDoc;
        private IServiceProvider serviceProvider;
        private XmlNode transformNodes;
        private XmlNodeList targetNodes;
        private XmlNodeList targetParents;
        private XmlAttribute transformAttribute;
        private XmlAttribute locatorAttribute;
        private XmlNamespaceManager namespaceManager;
        private static Regex nameAndArgumentsRegex;

        public XmlElementContext(XmlElementContext parent, XmlElement element, XmlDocument xmlTargetDoc, IServiceProvider serviceProvider) : base(element)
        {
            this.parentContext = parent;
            this.xmlTargetDoc = xmlTargetDoc;
            this.serviceProvider = serviceProvider;
        }

        private string ConstructParentXPath()
        {
            string str3;
            try
            {
                string str;
                str3 = this.CreateLocator(out str).ConstructParentPath((this.parentContext == null) ? string.Empty : this.parentContext.XPath, this, str);
            }
            catch (Exception exception)
            {
                throw this.WrapException(exception);
            }
            return str3;
        }

        public Transform ConstructTransform(out string argumentString)
        {
            Transform transform;
            try
            {
                transform = this.CreateObjectFromAttribute<Transform>(out argumentString, out this.transformAttribute);
            }
            catch (Exception exception)
            {
                throw this.WrapException(exception);
            }
            return transform;
        }

        private string ConstructXPath()
        {
            string str3;
            try
            {
                string str;
                str3 = this.CreateLocator(out str).ConstructPath((this.parentContext == null) ? string.Empty : this.parentContext.XPath, this, str);
            }
            catch (Exception exception)
            {
                throw this.WrapException(exception);
            }
            return str3;
        }

        private XmlNode CreateCloneInTargetDocument(XmlNode sourceNode)
        {
            XmlNode node;
            XmlFileInfoDocument targetDocument = this.TargetDocument as XmlFileInfoDocument;
            if (targetDocument != null)
            {
                node = targetDocument.CloneNodeFromOtherDocument(sourceNode);
            }
            else
            {
                XmlReader reader = new XmlTextReader(new StringReader(sourceNode.OuterXml));
                node = this.TargetDocument.ReadNode(reader);
            }
            this.ScrubTransformAttributesAndNamespaces(node);
            return node;
        }

        private Locator CreateLocator(out string argumentString)
        {
            Locator locator = this.CreateObjectFromAttribute<Locator>(out argumentString, out this.locatorAttribute);
            if (locator == null)
            {
                argumentString = null;
                locator = new DefaultLocator();
            }
            return locator;
        }

        private ObjectType CreateObjectFromAttribute<ObjectType>(out string argumentString, out XmlAttribute objectAttribute) where ObjectType: class
        {
            objectAttribute = this.Element.Attributes.GetNamedItem(typeof(ObjectType).Name, XmlTransformation.TransformNamespace) as XmlAttribute;
            try
            {
                if (objectAttribute != null)
                {
                    string str = this.ParseNameAndArguments(objectAttribute.Value, out argumentString);
                    if (!string.IsNullOrEmpty(str))
                    {
                        return this.GetService<NamedTypeFactory>().Construct<ObjectType>(str);
                    }
                }
            }
            catch (Exception exception)
            {
                throw this.WrapException(exception, objectAttribute);
            }
            argumentString = null;
            return default(ObjectType);
        }

        private bool ExistedInOriginal(string xpath)
        {
            IXmlOriginalDocumentService service = this.GetService<IXmlOriginalDocumentService>();
            if (service == null)
            {
                return false;
            }
            XmlNodeList list = service.SelectNodes(xpath, this.GetNamespaceManager());
            return ((list != null) && (list.Count > 0));
        }

        private XmlNamespaceManager GetNamespaceManager()
        {
            if (this.namespaceManager == null)
            {
                XmlNodeList list = this.Element.SelectNodes("namespace::*");
                if (list.Count <= 0)
                {
                    this.namespaceManager = new XmlNamespaceManager(this.GetParentNameTable());
                }
                else
                {
                    this.namespaceManager = new XmlNamespaceManager(this.Element.OwnerDocument.NameTable);
                    foreach (XmlAttribute attribute in list)
                    {
                        string prefix = string.Empty;
                        int index = attribute.Name.IndexOf(':');
                        prefix = (index < 0) ? "_defaultNamespace" : attribute.Name.Substring(index + 1);
                        this.namespaceManager.AddNamespace(prefix, attribute.Value);
                    }
                }
            }
            return this.namespaceManager;
        }

        private XmlNameTable GetParentNameTable() => 
            ((this.parentContext != null) ? this.parentContext.GetNamespaceManager().NameTable : this.Element.OwnerDocument.NameTable);

        public T GetService<T>() where T: class
        {
            if (this.serviceProvider != null)
            {
                return (this.serviceProvider.GetService(typeof(T)) as T);
            }
            return default(T);
        }

        private XmlNodeList GetTargetNodes(string xpath)
        {
            this.GetNamespaceManager();
            return this.TargetDocument.SelectNodes(xpath, this.GetNamespaceManager());
        }

        internal bool HasTargetNode(out XmlElementContext failedContext, out bool existedInOriginal)
        {
            failedContext = null;
            existedInOriginal = false;
            if (this.TargetNodes.Count != 0)
            {
                return true;
            }
            failedContext = this;
            while ((failedContext.parentContext != null) && (failedContext.parentContext.TargetNodes.Count == 0))
            {
                failedContext = failedContext.parentContext;
            }
            existedInOriginal = this.ExistedInOriginal(failedContext.XPath);
            return false;
        }

        internal bool HasTargetParent(out XmlElementContext failedContext, out bool existedInOriginal)
        {
            failedContext = null;
            existedInOriginal = false;
            if (this.TargetParents.Count != 0)
            {
                return true;
            }
            failedContext = this;
            while ((failedContext.parentContext != null) && (!string.IsNullOrEmpty(failedContext.parentContext.ParentXPath) && (failedContext.parentContext.TargetParents.Count == 0)))
            {
                failedContext = failedContext.parentContext;
            }
            existedInOriginal = this.ExistedInOriginal(failedContext.XPath);
            return false;
        }

        private string ParseNameAndArguments(string name, out string arguments)
        {
            arguments = null;
            Match match = this.NameAndArgumentsRegex.Match(name);
            if (!match.Success)
            {
                throw new XmlTransformationException(SR.XMLTRANSFORMATION_BadAttributeValue);
            }
            if (match.Groups["arguments"].Success)
            {
                CaptureCollection captures = match.Groups["arguments"].Captures;
                if ((captures.Count == 1) && !string.IsNullOrEmpty(captures[0].Value))
                {
                    arguments = captures[0].Value;
                }
            }
            return match.Groups["name"].Captures[0].Value;
        }

        private void ScrubTransformAttributesAndNamespaces(XmlNode node)
        {
            if (node.Attributes != null)
            {
                List<XmlAttribute> list = new List<XmlAttribute>();
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    if (attribute.NamespaceURI == XmlTransformation.TransformNamespace)
                    {
                        list.Add(attribute);
                        continue;
                    }
                    if (!attribute.Prefix.Equals("xmlns") && !attribute.Name.Equals("xmlns"))
                    {
                        attribute.Prefix = null;
                        continue;
                    }
                    list.Add(attribute);
                }
                foreach (XmlAttribute attribute2 in list)
                {
                    node.Attributes.Remove(attribute2);
                }
            }
            foreach (XmlNode node2 in node.ChildNodes)
            {
                this.ScrubTransformAttributesAndNamespaces(node2);
            }
        }

        private Exception WrapException(Exception ex) => 
            XmlNodeException.Wrap(ex, this.Element);

        private Exception WrapException(Exception ex, XmlNode node) => 
            XmlNodeException.Wrap(ex, node);

        public XmlElement Element =>
            (base.Node as XmlElement);

        public string XPath
        {
            get
            {
                if (this.xpath == null)
                {
                    this.xpath = this.ConstructXPath();
                }
                return this.xpath;
            }
        }

        public string ParentXPath
        {
            get
            {
                if (this.parentXPath == null)
                {
                    this.parentXPath = this.ConstructParentXPath();
                }
                return this.parentXPath;
            }
        }

        public int TransformLineNumber
        {
            get
            {
                IXmlLineInfo transformAttribute = this.transformAttribute as IXmlLineInfo;
                return ((transformAttribute == null) ? base.LineNumber : transformAttribute.LineNumber);
            }
        }

        public int TransformLinePosition
        {
            get
            {
                IXmlLineInfo transformAttribute = this.transformAttribute as IXmlLineInfo;
                return ((transformAttribute == null) ? base.LinePosition : transformAttribute.LinePosition);
            }
        }

        public XmlAttribute TransformAttribute =>
            this.transformAttribute;

        public XmlAttribute LocatorAttribute =>
            this.locatorAttribute;

        internal XmlNode TransformNode
        {
            get
            {
                if (this.transformNodes == null)
                {
                    this.transformNodes = this.CreateCloneInTargetDocument(this.Element);
                }
                return this.transformNodes;
            }
        }

        internal XmlNodeList TargetNodes
        {
            get
            {
                if (this.targetNodes == null)
                {
                    this.targetNodes = this.GetTargetNodes(this.XPath);
                }
                return this.targetNodes;
            }
        }

        internal XmlNodeList TargetParents
        {
            get
            {
                if ((this.targetParents == null) && (this.parentContext != null))
                {
                    this.targetParents = this.GetTargetNodes(this.ParentXPath);
                }
                return this.targetParents;
            }
        }

        private XmlDocument TargetDocument =>
            this.xmlTargetDoc;

        private Regex NameAndArgumentsRegex
        {
            get
            {
                if (nameAndArgumentsRegex == null)
                {
                    nameAndArgumentsRegex = new Regex(@"\A\s*(?<name>\w+)(\s*\((?<arguments>.*)\))?\s*\Z", RegexOptions.Singleline | RegexOptions.Compiled);
                }
                return nameAndArgumentsRegex;
            }
        }
    }
}

