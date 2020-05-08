namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Xml;

    internal abstract class Transform
    {
        private Microsoft.Web.XmlTransform.MissingTargetMessage missingTargetMessage;
        private bool applyTransformToAllTargetNodes;
        private bool useParentAsTargetNode;
        private XmlTransformationLogger logger;
        private XmlElementContext context;
        private XmlNode currentTransformNode;
        private XmlNode currentTargetNode;
        private string argumentString;
        private IList<string> arguments;

        protected Transform() : this(TransformFlags.None)
        {
        }

        protected Transform(TransformFlags flags) : this(flags, Microsoft.Web.XmlTransform.MissingTargetMessage.Warning)
        {
        }

        protected Transform(TransformFlags flags, Microsoft.Web.XmlTransform.MissingTargetMessage message)
        {
            this.missingTargetMessage = message;
            this.applyTransformToAllTargetNodes = (flags & TransformFlags.ApplyTransformToAllTargetNodes) == TransformFlags.ApplyTransformToAllTargetNodes;
            this.useParentAsTargetNode = (flags & TransformFlags.UseParentAsTargetNode) == TransformFlags.UseParentAsTargetNode;
        }

        protected abstract void Apply();
        private bool ApplyOnAllTargetNodes()
        {
            bool flag = false;
            XmlNode transformNode = this.TransformNode;
            foreach (XmlNode node2 in this.TargetNodes)
            {
                try
                {
                    this.currentTargetNode = node2;
                    this.currentTransformNode = transformNode.Clone();
                    this.ApplyOnce();
                }
                catch (Exception exception)
                {
                    this.Log.LogErrorFromException(exception);
                    flag = true;
                }
            }
            this.currentTargetNode = null;
            return flag;
        }

        private void ApplyOnce()
        {
            this.WriteApplyMessage(this.TargetNode);
            this.Apply();
        }

        internal void Execute(XmlElementContext context, string argumentString)
        {
            if ((this.context == null) && (this.argumentString == null))
            {
                bool flag = false;
                bool flag2 = false;
                try
                {
                    this.context = context;
                    this.argumentString = argumentString;
                    this.arguments = null;
                    if (this.ShouldExecuteTransform())
                    {
                        flag2 = true;
                        object[] messageArgs = new object[] { this.TransformNameLong };
                        this.Log.StartSection(MessageType.Verbose, SR.XMLTRANSFORMATION_TransformBeginExecutingMessage, messageArgs);
                        object[] objArray2 = new object[] { context.XPath };
                        this.Log.LogMessage(MessageType.Verbose, SR.XMLTRANSFORMATION_TransformStatusXPath, objArray2);
                        if (this.ApplyTransformToAllTargetNodes)
                        {
                            this.ApplyOnAllTargetNodes();
                        }
                        else
                        {
                            this.ApplyOnce();
                        }
                    }
                }
                catch (Exception exception)
                {
                    flag = true;
                    if (context.TransformAttribute != null)
                    {
                        this.Log.LogErrorFromException(XmlNodeException.Wrap(exception, context.TransformAttribute));
                    }
                    else
                    {
                        this.Log.LogErrorFromException(exception);
                    }
                }
                finally
                {
                    if (!flag2)
                    {
                        object[] messageArgs = new object[] { this.TransformNameLong };
                        this.Log.LogMessage(MessageType.Normal, SR.XMLTRANSFORMATION_TransformNotExecutingMessage, messageArgs);
                    }
                    else if (flag)
                    {
                        object[] messageArgs = new object[] { this.TransformNameShort };
                        this.Log.EndSection(MessageType.Verbose, SR.XMLTRANSFORMATION_TransformErrorExecutingMessage, messageArgs);
                    }
                    else
                    {
                        object[] messageArgs = new object[] { this.TransformNameShort };
                        this.Log.EndSection(MessageType.Verbose, SR.XMLTRANSFORMATION_TransformEndExecutingMessage, messageArgs);
                    }
                    this.context = null;
                    this.argumentString = null;
                    this.arguments = null;
                    this.ReleaseLogger();
                }
            }
        }

        protected T GetService<T>() where T: class => 
            this.context.GetService<T>();

        private void HandleMissingTarget(XmlElementContext matchFailureContext, bool existedInOriginal)
        {
            string format = existedInOriginal ? SR.XMLTRANSFORMATION_TransformSourceMatchWasRemoved : SR.XMLTRANSFORMATION_TransformNoMatchingTargetNodes;
            object[] args = new object[] { matchFailureContext.XPath };
            string message = string.Format(CultureInfo.CurrentCulture, format, args);
            switch (this.MissingTargetMessage)
            {
                case Microsoft.Web.XmlTransform.MissingTargetMessage.None:
                    this.Log.LogMessage(MessageType.Verbose, message, new object[0]);
                    return;

                case Microsoft.Web.XmlTransform.MissingTargetMessage.Information:
                    this.Log.LogMessage(MessageType.Normal, message, new object[0]);
                    return;

                case Microsoft.Web.XmlTransform.MissingTargetMessage.Warning:
                    this.Log.LogWarning(matchFailureContext.Node, message, new object[0]);
                    return;

                case Microsoft.Web.XmlTransform.MissingTargetMessage.Error:
                    throw new XmlNodeException(message, matchFailureContext.Node);
            }
        }

        private bool HasRequiredTarget()
        {
            XmlElementContext context;
            bool existedInOriginal = false;
            if (!this.UseParentAsTargetNode ? this.context.HasTargetNode(out context, out existedInOriginal) : this.context.HasTargetParent(out context, out existedInOriginal))
            {
                return true;
            }
            this.HandleMissingTarget(context, existedInOriginal);
            return false;
        }

        private void ReleaseLogger()
        {
            if (this.logger != null)
            {
                this.logger.CurrentReferenceNode = null;
                this.logger = null;
            }
        }

        private bool ShouldExecuteTransform() => 
            this.HasRequiredTarget();

        private void WriteApplyMessage(XmlNode targetNode)
        {
            IXmlLineInfo info = targetNode as IXmlLineInfo;
            if (info != null)
            {
                object[] messageArgs = new object[] { targetNode.Name, info.LineNumber, info.LinePosition };
                this.Log.LogMessage(MessageType.Verbose, SR.XMLTRANSFORMATION_TransformStatusApplyTarget, messageArgs);
            }
            else
            {
                object[] messageArgs = new object[] { targetNode.Name };
                this.Log.LogMessage(MessageType.Verbose, SR.XMLTRANSFORMATION_TransformStatusApplyTargetNoLineInfo, messageArgs);
            }
        }

        protected bool ApplyTransformToAllTargetNodes
        {
            get => 
                this.applyTransformToAllTargetNodes;
            set => 
                (this.applyTransformToAllTargetNodes = value);
        }

        protected bool UseParentAsTargetNode
        {
            get => 
                this.useParentAsTargetNode;
            set => 
                (this.useParentAsTargetNode = value);
        }

        protected Microsoft.Web.XmlTransform.MissingTargetMessage MissingTargetMessage
        {
            get => 
                this.missingTargetMessage;
            set => 
                (this.missingTargetMessage = value);
        }

        protected XmlNode TransformNode =>
            ((this.currentTransformNode != null) ? this.currentTransformNode : this.context.TransformNode);

        protected XmlNode TargetNode
        {
            get
            {
                if (this.currentTargetNode == null)
                {
                    using (IEnumerator enumerator = this.TargetNodes.GetEnumerator())
                    {
                        if (enumerator.MoveNext())
                        {
                            return (XmlNode) enumerator.Current;
                        }
                    }
                }
                return this.currentTargetNode;
            }
        }

        protected XmlNodeList TargetNodes =>
            (!this.UseParentAsTargetNode ? this.context.TargetNodes : this.context.TargetParents);

        protected XmlNodeList TargetChildNodes =>
            this.context.TargetNodes;

        protected XmlTransformationLogger Log
        {
            get
            {
                if (this.logger == null)
                {
                    this.logger = this.context.GetService<XmlTransformationLogger>();
                    if (this.logger != null)
                    {
                        this.logger.CurrentReferenceNode = this.context.TransformAttribute;
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

        private string TransformNameLong
        {
            get
            {
                if (!this.context.HasLineInfo)
                {
                    return this.TransformNameShort;
                }
                object[] args = new object[] { this.TransformName, this.context.TransformLineNumber, this.context.TransformLinePosition };
                return string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_TransformNameFormatLong, args);
            }
        }

        internal string TransformNameShort
        {
            get
            {
                object[] args = new object[] { this.TransformName };
                return string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_TransformNameFormatShort, args);
            }
        }

        private string TransformName =>
            base.GetType().Name;
    }
}

