namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Globalization;
    using System.Xml;

    internal class XmlTransformationLogger
    {
        private bool hasLoggedErrors;
        private IXmlTransformationLogger externalLogger;
        private XmlNode currentReferenceNode;
        private bool fSupressWarnings;

        internal XmlTransformationLogger(IXmlTransformationLogger logger)
        {
            this.externalLogger = logger;
        }

        private string ConvertUriToFileName(string fileName)
        {
            try
            {
                Uri uri = new Uri(fileName);
                if (uri.IsFile && string.IsNullOrEmpty(uri.Host))
                {
                    fileName = uri.LocalPath;
                }
            }
            catch (UriFormatException)
            {
            }
            return fileName;
        }

        private string ConvertUriToFileName(XmlDocument xmlDocument)
        {
            XmlFileInfoDocument document = xmlDocument as XmlFileInfoDocument;
            string fileName = (document == null) ? document.BaseURI : document.FileName;
            return this.ConvertUriToFileName(fileName);
        }

        public void EndSection(string message, params object[] messageArgs)
        {
            if (this.externalLogger != null)
            {
                this.externalLogger.EndSection(message, messageArgs);
            }
        }

        public void EndSection(MessageType type, string message, params object[] messageArgs)
        {
            if (this.externalLogger != null)
            {
                this.externalLogger.EndSection(type, message, messageArgs);
            }
        }

        public void LogError(string message, params object[] messageArgs)
        {
            this.hasLoggedErrors = true;
            if (this.CurrentReferenceNode != null)
            {
                this.LogError(this.CurrentReferenceNode, message, messageArgs);
            }
            else
            {
                if (this.externalLogger == null)
                {
                    throw new XmlTransformationException(string.Format(CultureInfo.CurrentCulture, message, messageArgs));
                }
                this.externalLogger.LogError(message, messageArgs);
            }
        }

        public void LogError(XmlNode referenceNode, string message, params object[] messageArgs)
        {
            this.hasLoggedErrors = true;
            if (this.externalLogger == null)
            {
                throw new XmlNodeException(string.Format(CultureInfo.CurrentCulture, message, messageArgs), referenceNode);
            }
            string file = this.ConvertUriToFileName(referenceNode.OwnerDocument);
            IXmlLineInfo info = referenceNode as IXmlLineInfo;
            if (info != null)
            {
                this.externalLogger.LogError(file, info.LineNumber, info.LinePosition, message, messageArgs);
            }
            else
            {
                this.externalLogger.LogError(file, message, messageArgs);
            }
        }

        internal void LogErrorFromException(Exception ex)
        {
            this.hasLoggedErrors = true;
            if (this.externalLogger == null)
            {
                throw ex;
            }
            XmlNodeException exception = ex as XmlNodeException;
            if ((exception != null) && exception.HasErrorInfo)
            {
                this.externalLogger.LogErrorFromException(exception, this.ConvertUriToFileName(exception.FileName), exception.LineNumber, exception.LinePosition);
            }
            else
            {
                this.externalLogger.LogErrorFromException(ex);
            }
        }

        public void LogMessage(string message, params object[] messageArgs)
        {
            if (this.externalLogger != null)
            {
                this.externalLogger.LogMessage(message, messageArgs);
            }
        }

        public void LogMessage(MessageType type, string message, params object[] messageArgs)
        {
            if (this.externalLogger != null)
            {
                this.externalLogger.LogMessage(type, message, messageArgs);
            }
        }

        public void LogWarning(string message, params object[] messageArgs)
        {
            if (this.SupressWarnings)
            {
                this.LogMessage(message, messageArgs);
            }
            else if (this.CurrentReferenceNode != null)
            {
                this.LogWarning(this.CurrentReferenceNode, message, messageArgs);
            }
            else if (this.externalLogger != null)
            {
                this.externalLogger.LogWarning(message, messageArgs);
            }
        }

        public void LogWarning(XmlNode referenceNode, string message, params object[] messageArgs)
        {
            if (this.SupressWarnings)
            {
                this.LogMessage(message, messageArgs);
            }
            else if (this.externalLogger != null)
            {
                string file = this.ConvertUriToFileName(referenceNode.OwnerDocument);
                IXmlLineInfo info = referenceNode as IXmlLineInfo;
                if (info != null)
                {
                    this.externalLogger.LogWarning(file, info.LineNumber, info.LinePosition, message, messageArgs);
                }
                else
                {
                    this.externalLogger.LogWarning(file, message, messageArgs);
                }
            }
        }

        public void StartSection(string message, params object[] messageArgs)
        {
            if (this.externalLogger != null)
            {
                this.externalLogger.StartSection(message, messageArgs);
            }
        }

        public void StartSection(MessageType type, string message, params object[] messageArgs)
        {
            if (this.externalLogger != null)
            {
                this.externalLogger.StartSection(type, message, messageArgs);
            }
        }

        internal bool HasLoggedErrors
        {
            get => 
                this.hasLoggedErrors;
            set => 
                (this.hasLoggedErrors = false);
        }

        internal XmlNode CurrentReferenceNode
        {
            get => 
                this.currentReferenceNode;
            set => 
                (this.currentReferenceNode = value);
        }

        public bool SupressWarnings
        {
            get => 
                this.fSupressWarnings;
            set => 
                (this.fSupressWarnings = value);
        }
    }
}

