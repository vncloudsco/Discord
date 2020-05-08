namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.IO;
    using System.Xml;

    internal class XmlTransformation : IServiceProvider, IDisposable
    {
        internal static readonly string TransformNamespace = "http://schemas.microsoft.com/XML-Document-Transform";
        internal static readonly string SupressWarnings = "SupressWarnings";
        private string transformFile;
        private XmlDocument xmlTransformation;
        private XmlDocument xmlTarget;
        private XmlTransformableDocument xmlTransformable;
        private XmlTransformationLogger logger;
        private NamedTypeFactory namedTypeFactory;
        private ServiceContainer transformationServiceContainer;
        private ServiceContainer documentServiceContainer;
        private bool hasTransformNamespace;

        public XmlTransformation(string transformFile) : this(transformFile, true, null)
        {
        }

        public XmlTransformation(Stream transformStream, IXmlTransformationLogger logger)
        {
            this.transformationServiceContainer = new ServiceContainer();
            this.logger = new XmlTransformationLogger(logger);
            this.transformFile = string.Empty;
            this.xmlTransformation = new XmlFileInfoDocument();
            this.xmlTransformation.Load(transformStream);
            this.InitializeTransformationServices();
            this.PreprocessTransformDocument();
        }

        public XmlTransformation(string transform, IXmlTransformationLogger logger) : this(transform, true, logger)
        {
        }

        public XmlTransformation(string transform, bool isTransformAFile, IXmlTransformationLogger logger)
        {
            this.transformationServiceContainer = new ServiceContainer();
            this.transformFile = transform;
            this.logger = new XmlTransformationLogger(logger);
            this.xmlTransformation = new XmlFileInfoDocument();
            if (isTransformAFile)
            {
                this.xmlTransformation.Load(transform);
            }
            else
            {
                this.xmlTransformation.LoadXml(transform);
            }
            this.InitializeTransformationServices();
            this.PreprocessTransformDocument();
        }

        public void AddTransformationService(Type serviceType, object serviceInstance)
        {
            this.transformationServiceContainer.AddService(serviceType, serviceInstance);
        }

        public bool Apply(XmlDocument xmlTarget)
        {
            if (this.xmlTarget != null)
            {
                return false;
            }
            this.logger.HasLoggedErrors = false;
            this.xmlTarget = xmlTarget;
            this.xmlTransformable = xmlTarget as XmlTransformableDocument;
            try
            {
                if (this.hasTransformNamespace)
                {
                    this.InitializeDocumentServices(xmlTarget);
                    this.TransformLoop(this.xmlTransformation);
                }
                else
                {
                    object[] messageArgs = new object[] { TransformNamespace };
                    this.logger.LogMessage(MessageType.Normal, "The expected namespace {0} was not found in the transform file", messageArgs);
                }
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
            }
            finally
            {
                this.ReleaseDocumentServices();
                this.xmlTarget = null;
                this.xmlTransformable = null;
            }
            return !this.logger.HasLoggedErrors;
        }

        private XmlElementContext CreateElementContext(XmlElementContext parentContext, XmlElement element) => 
            new XmlElementContext(parentContext, element, this.xmlTarget, this);

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.transformationServiceContainer != null)
            {
                this.transformationServiceContainer.Dispose();
                this.transformationServiceContainer = null;
            }
            if (this.documentServiceContainer != null)
            {
                this.documentServiceContainer.Dispose();
                this.documentServiceContainer = null;
            }
            if (this.xmlTransformable != null)
            {
                this.xmlTransformable.Dispose();
                this.xmlTransformable = null;
            }
            if (this.xmlTransformation is XmlFileInfoDocument)
            {
                (this.xmlTransformation as XmlFileInfoDocument).Dispose();
                this.xmlTransformation = null;
            }
        }

        ~XmlTransformation()
        {
            this.Dispose(false);
        }

        public object GetService(Type serviceType)
        {
            object service = null;
            if (this.documentServiceContainer != null)
            {
                service = this.documentServiceContainer.GetService(serviceType);
            }
            if (service == null)
            {
                service = this.transformationServiceContainer.GetService(serviceType);
            }
            return service;
        }

        private void HandleElement(XmlElementContext context)
        {
            string str;
            Transform transform = context.ConstructTransform(out str);
            if (transform != null)
            {
                bool supressWarnings = this.logger.SupressWarnings;
                XmlAttribute namedItem = context.Element.Attributes.GetNamedItem(SupressWarnings, TransformNamespace) as XmlAttribute;
                if (namedItem != null)
                {
                    bool flag2 = Convert.ToBoolean(namedItem.Value, CultureInfo.InvariantCulture);
                    this.logger.SupressWarnings = flag2;
                }
                try
                {
                    this.OnApplyingTransform();
                    transform.Execute(context, str);
                    this.OnAppliedTransform();
                }
                catch (Exception exception)
                {
                    this.HandleException(exception, context);
                }
                finally
                {
                    this.logger.SupressWarnings = supressWarnings;
                }
            }
            this.TransformLoop(context);
        }

        private void HandleException(Exception ex)
        {
            this.logger.LogErrorFromException(ex);
        }

        private void HandleException(Exception ex, XmlNodeContext context)
        {
            this.HandleException(this.WrapException(ex, context));
        }

        private void InitializeDocumentServices(XmlDocument document)
        {
            this.documentServiceContainer = new ServiceContainer();
            if (document is IXmlOriginalDocumentService)
            {
                this.documentServiceContainer.AddService(typeof(IXmlOriginalDocumentService), document);
            }
        }

        private void InitializeTransformationServices()
        {
            this.namedTypeFactory = new NamedTypeFactory(this.transformFile);
            this.transformationServiceContainer.AddService(this.namedTypeFactory.GetType(), this.namedTypeFactory);
            this.transformationServiceContainer.AddService(this.logger.GetType(), this.logger);
        }

        private void OnAppliedTransform()
        {
            if (this.xmlTransformable != null)
            {
                this.xmlTransformable.OnAfterChange();
            }
        }

        private void OnApplyingTransform()
        {
            if (this.xmlTransformable != null)
            {
                this.xmlTransformable.OnBeforeChange();
            }
        }

        private void PreprocessImportElement(XmlElementContext context)
        {
            string assemblyName = null;
            string nameSpace = null;
            string path = null;
            foreach (XmlAttribute attribute in context.Element.Attributes)
            {
                string str4;
                if ((attribute.NamespaceURI.Length == 0) && ((str4 = attribute.Name) != null))
                {
                    if (str4 == "assembly")
                    {
                        assemblyName = attribute.Value;
                        continue;
                    }
                    if (str4 == "namespace")
                    {
                        nameSpace = attribute.Value;
                        continue;
                    }
                    if (str4 == "path")
                    {
                        path = attribute.Value;
                        continue;
                    }
                }
                object[] args = new object[] { attribute.Name };
                throw new XmlNodeException(string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_ImportUnknownAttribute, args), attribute);
            }
            if ((assemblyName != null) && (path != null))
            {
                throw new XmlNodeException(string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_ImportAttributeConflict, new object[0]), context.Element);
            }
            if ((assemblyName == null) && (path == null))
            {
                throw new XmlNodeException(string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_ImportMissingAssembly, new object[0]), context.Element);
            }
            if (nameSpace == null)
            {
                throw new XmlNodeException(string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_ImportMissingNamespace, new object[0]), context.Element);
            }
            if (assemblyName != null)
            {
                this.namedTypeFactory.AddAssemblyRegistration(assemblyName, nameSpace);
            }
            else
            {
                this.namedTypeFactory.AddPathRegistration(path, nameSpace);
            }
        }

        private void PreprocessTransformDocument()
        {
            this.hasTransformNamespace = false;
            foreach (XmlAttribute attribute in this.xmlTransformation.SelectNodes("//namespace::*"))
            {
                if (attribute.Value.Equals(TransformNamespace, StringComparison.Ordinal))
                {
                    this.hasTransformNamespace = true;
                    break;
                }
            }
            if (this.hasTransformNamespace)
            {
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("xdt", TransformNamespace);
                foreach (XmlNode node in this.xmlTransformation.SelectNodes("//xdt:*", nsmgr))
                {
                    XmlElement element = node as XmlElement;
                    if (element != null)
                    {
                        XmlElementContext context = null;
                        try
                        {
                            string str;
                            if (((str = element.LocalName) != null) && (str == "Import"))
                            {
                                context = this.CreateElementContext(null, element);
                                this.PreprocessImportElement(context);
                            }
                            else
                            {
                                object[] messageArgs = new object[] { element.Name };
                                this.logger.LogWarning(element, SR.XMLTRANSFORMATION_UnknownXdtTag, messageArgs);
                            }
                        }
                        catch (Exception exception)
                        {
                            if (context != null)
                            {
                                exception = this.WrapException(exception, context);
                            }
                            this.logger.LogErrorFromException(exception);
                            throw new XmlTransformationException(SR.XMLTRANSFORMATION_FatalTransformSyntaxError, exception);
                        }
                        finally
                        {
                            context = null;
                        }
                    }
                }
            }
        }

        private void ReleaseDocumentServices()
        {
            if (this.documentServiceContainer != null)
            {
                this.documentServiceContainer.RemoveService(typeof(IXmlOriginalDocumentService));
                this.documentServiceContainer = null;
            }
        }

        public void RemoveTransformationService(Type serviceType)
        {
            this.transformationServiceContainer.RemoveService(serviceType);
        }

        private void TransformLoop(XmlNodeContext parentContext)
        {
            foreach (XmlNode node in parentContext.Node.ChildNodes)
            {
                XmlElement element = node as XmlElement;
                if (element != null)
                {
                    XmlElementContext context = this.CreateElementContext(parentContext as XmlElementContext, element);
                    try
                    {
                        this.HandleElement(context);
                    }
                    catch (Exception exception)
                    {
                        this.HandleException(exception, context);
                    }
                }
            }
        }

        private void TransformLoop(XmlDocument xmlSource)
        {
            this.TransformLoop(new XmlNodeContext(xmlSource));
        }

        private Exception WrapException(Exception ex, XmlNodeContext context) => 
            XmlNodeException.Wrap(ex, context.Node);

        public bool HasTransformNamespace =>
            this.hasTransformNamespace;
    }
}

