namespace Microsoft.Web.XmlTransform
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;

    internal class SetTokenizedAttributes : AttributeTransform
    {
        private SetTokenizedAttributeStorage storageDictionary;
        private bool fInitStorageDictionary;
        public static readonly string Token = "Token";
        public static readonly string TokenNumber = "TokenNumber";
        public static readonly string XPathWithIndex = "XPathWithIndex";
        public static readonly string ParameterAttribute = "Parameter";
        public static readonly string XpathLocator = "XpathLocator";
        public static readonly string XPathWithLocator = "XPathWithLocator";
        private XmlAttribute tokenizeValueCurrentXmlAttribute;
        private static Regex s_dirRegex = null;
        private static Regex s_parentAttribRegex = null;
        private static Regex s_tokenFormatRegex = null;

        protected override void Apply()
        {
            bool fTokenizeParameter = false;
            SetTokenizedAttributeStorage transformStorage = this.TransformStorage;
            List<Dictionary<string, string>> parameters = null;
            if (transformStorage != null)
            {
                fTokenizeParameter = transformStorage.EnableTokenizeParameters;
                if (fTokenizeParameter)
                {
                    parameters = transformStorage.DictionaryList;
                }
            }
            foreach (XmlAttribute attribute in base.TransformAttributes)
            {
                XmlAttribute namedItem = base.TargetNode.Attributes.GetNamedItem(attribute.Name) as XmlAttribute;
                string str = this.TokenizeValue(namedItem, attribute, fTokenizeParameter, parameters);
                if (namedItem != null)
                {
                    namedItem.Value = str;
                }
                else
                {
                    XmlAttribute node = (XmlAttribute) attribute.Clone();
                    node.Value = str;
                    base.TargetNode.Attributes.Append(node);
                }
                object[] messageArgs = new object[] { attribute.Name };
                base.Log.LogMessage(MessageType.Verbose, SR.XMLTRANSFORMATION_TransformMessageSetAttribute, messageArgs);
            }
            if (base.TransformAttributes.Count <= 0)
            {
                base.Log.LogWarning(SR.XMLTRANSFORMATION_TransformMessageNoSetAttributes, new object[0]);
            }
            else
            {
                object[] messageArgs = new object[] { base.TransformAttributes.Count };
                base.Log.LogMessage(MessageType.Verbose, SR.XMLTRANSFORMATION_TransformMessageSetAttributes, messageArgs);
            }
        }

        protected string EscapeDirRegexSpecialCharacter(string value, bool escape) => 
            (!escape ? value.Replace("&apos;", "'") : value.Replace("'", "&apos;"));

        protected string GetAttributeValue(string attributeName)
        {
            string str = null;
            XmlAttribute namedItem = base.TargetNode.Attributes.GetNamedItem(attributeName) as XmlAttribute;
            if ((namedItem == null) && (string.Compare(attributeName, this.tokenizeValueCurrentXmlAttribute.Name, StringComparison.OrdinalIgnoreCase) != 0))
            {
                namedItem = base.TransformNode.Attributes.GetNamedItem(attributeName) as XmlAttribute;
            }
            if (namedItem != null)
            {
                str = namedItem.Value;
            }
            return str;
        }

        private string GetXPathToAttribute(XmlAttribute xmlAttribute) => 
            this.GetXPathToAttribute(xmlAttribute, null);

        private string GetXPathToAttribute(XmlAttribute xmlAttribute, IList<string> locators)
        {
            string str = string.Empty;
            if (xmlAttribute != null)
            {
                string xPathToNode = this.GetXPathToNode(xmlAttribute.OwnerElement);
                if (!string.IsNullOrEmpty(xPathToNode))
                {
                    StringBuilder builder = new StringBuilder(0x100);
                    if ((locators != null) && (locators.Count != 0))
                    {
                        foreach (string str3 in locators)
                        {
                            string attributeValue = this.GetAttributeValue(str3);
                            if (string.IsNullOrEmpty(attributeValue))
                            {
                                throw new XmlTransformationException(string.Format(CultureInfo.CurrentCulture, SR.XMLTRANSFORMATION_MatchAttributeDoesNotExist, new object[] { str3 }));
                            }
                            if (builder.Length != 0)
                            {
                                builder.Append(" and ");
                            }
                            builder.Append(string.Format(CultureInfo.InvariantCulture, "@{0}='{1}'", new object[] { str3, attributeValue }));
                        }
                    }
                    if (builder.Length == 0)
                    {
                        for (int i = 0; i < base.TargetNodes.Count; i++)
                        {
                            if (ReferenceEquals(base.TargetNodes[i], xmlAttribute.OwnerElement))
                            {
                                builder.Append((i + 1).ToString(CultureInfo.InvariantCulture));
                                break;
                            }
                        }
                    }
                    xPathToNode = xPathToNode + "[" + builder.ToString() + "]";
                }
                str = xPathToNode + "/@" + xmlAttribute.Name;
            }
            return str;
        }

        private string GetXPathToNode(XmlNode xmlNode) => 
            (((xmlNode == null) || (xmlNode.NodeType == XmlNodeType.Document)) ? null : (this.GetXPathToNode(xmlNode.ParentNode) + "/" + xmlNode.Name));

        protected static string SubstituteKownValue(string transformValue, Regex patternRegex, string patternPrefix, GetValueCallback getValueDelegate)
        {
            int startIndex = 0;
            List<Match> list = new List<Match>();
            while (true)
            {
                startIndex = transformValue.IndexOf(patternPrefix, startIndex, StringComparison.OrdinalIgnoreCase);
                if (startIndex > -1)
                {
                    Match item = patternRegex.Match(transformValue, startIndex);
                    if (!item.Success)
                    {
                        startIndex++;
                    }
                    else
                    {
                        list.Add(item);
                        startIndex = item.Index + item.Length;
                    }
                }
                if (startIndex <= -1)
                {
                    StringBuilder builder = new StringBuilder(transformValue.Length);
                    if (list.Count > 0)
                    {
                        builder.Remove(0, builder.Length);
                        startIndex = 0;
                        int num2 = 0;
                        foreach (Match match2 in list)
                        {
                            builder.Append(transformValue.Substring(startIndex, match2.Index - startIndex));
                            Capture capture = match2.Groups["tagname"];
                            string key = capture.Value;
                            string str2 = getValueDelegate(key);
                            if (str2 != null)
                            {
                                builder.Append(str2);
                            }
                            else
                            {
                                builder.Append(match2.Value);
                            }
                            startIndex = match2.Index + match2.Length;
                            num2++;
                        }
                        builder.Append(transformValue.Substring(startIndex));
                        transformValue = builder.ToString();
                    }
                    return transformValue;
                }
            }
        }

        private string TokenizeValue(XmlAttribute targetAttribute, XmlAttribute transformAttribute, bool fTokenizeParameter, List<Dictionary<string, string>> parameters)
        {
            this.tokenizeValueCurrentXmlAttribute = transformAttribute;
            string xPathToAttribute = this.GetXPathToAttribute(targetAttribute);
            string input = SubstituteKownValue(transformAttribute.Value, ParentAttributeRegex, "$(", key => this.EscapeDirRegexSpecialCharacter(this.GetAttributeValue(key), true));
            if (fTokenizeParameter && (parameters != null))
            {
                int startIndex = 0;
                StringBuilder builder = new StringBuilder(input.Length);
                startIndex = 0;
                List<Match> list = new List<Match>();
                while (true)
                {
                    startIndex = input.IndexOf("{%", startIndex, StringComparison.OrdinalIgnoreCase);
                    if (startIndex > -1)
                    {
                        Match item = DirRegex.Match(input, startIndex);
                        if (!item.Success)
                        {
                            startIndex++;
                        }
                        else
                        {
                            list.Add(item);
                            startIndex = item.Index + item.Length;
                        }
                    }
                    if (startIndex <= -1)
                    {
                        if (list.Count > 0)
                        {
                            builder.Remove(0, builder.Length);
                            startIndex = 0;
                            int num2 = 0;
                            foreach (Match match2 in list)
                            {
                                builder.Append(input.Substring(startIndex, match2.Index - startIndex));
                                CaptureCollection captures = match2.Groups["attrname"].Captures;
                                if ((captures != null) && (captures.Count > 0))
                                {
                                    GetValueCallback getValueDelegate = null;
                                    CaptureCollection captures2 = match2.Groups["attrval"].Captures;
                                    Dictionary<string, string> paramDictionary = new Dictionary<string, string>(4, StringComparer.OrdinalIgnoreCase) {
                                        [XPathWithIndex] = xPathToAttribute,
                                        [TokenNumber] = num2.ToString(CultureInfo.InvariantCulture)
                                    };
                                    int num3 = 0;
                                    while (true)
                                    {
                                        if (num3 >= captures.Count)
                                        {
                                            string tokenFormat = null;
                                            if (!paramDictionary.TryGetValue(Token, out tokenFormat))
                                            {
                                                tokenFormat = this.storageDictionary.TokenFormat;
                                            }
                                            if (!string.IsNullOrEmpty(tokenFormat))
                                            {
                                                paramDictionary[Token] = tokenFormat;
                                            }
                                            int count = paramDictionary.Count;
                                            string[] array = new string[count];
                                            paramDictionary.Keys.CopyTo(array, 0);
                                            int index = 0;
                                            while (true)
                                            {
                                                if (index >= count)
                                                {
                                                    string str9;
                                                    if (paramDictionary.TryGetValue(Token, out tokenFormat))
                                                    {
                                                        builder.Append(tokenFormat);
                                                    }
                                                    if (paramDictionary.TryGetValue(XpathLocator, out str9) && !string.IsNullOrEmpty(str9))
                                                    {
                                                        IList<string> locators = XmlArgumentUtility.SplitArguments(str9);
                                                        string str10 = this.GetXPathToAttribute(targetAttribute, locators);
                                                        if (!string.IsNullOrEmpty(str10))
                                                        {
                                                            paramDictionary[XPathWithLocator] = str10;
                                                        }
                                                    }
                                                    parameters.Add(paramDictionary);
                                                    break;
                                                }
                                                string str6 = array[index];
                                                string transformValue = paramDictionary[str6];
                                                if (getValueDelegate == null)
                                                {
                                                    getValueDelegate = key => paramDictionary.ContainsKey(key) ? paramDictionary[key] : null;
                                                }
                                                paramDictionary[str6] = SubstituteKownValue(transformValue, TokenFormatRegex, "#(", getValueDelegate);
                                                index++;
                                            }
                                            break;
                                        }
                                        string str3 = captures[num3].Value;
                                        string str4 = null;
                                        if ((captures2 != null) && (num3 < captures2.Count))
                                        {
                                            str4 = this.EscapeDirRegexSpecialCharacter(captures2[num3].Value, false);
                                        }
                                        paramDictionary[str3] = str4;
                                        num3++;
                                    }
                                }
                                startIndex = match2.Index + match2.Length;
                                num2++;
                            }
                            builder.Append(input.Substring(startIndex));
                            input = builder.ToString();
                        }
                        break;
                    }
                }
            }
            return input;
        }

        protected SetTokenizedAttributeStorage TransformStorage
        {
            get
            {
                if ((this.storageDictionary == null) && !this.fInitStorageDictionary)
                {
                    this.storageDictionary = base.GetService<SetTokenizedAttributeStorage>();
                    this.fInitStorageDictionary = true;
                }
                return this.storageDictionary;
            }
        }

        internal static Regex DirRegex
        {
            get
            {
                if (s_dirRegex == null)
                {
                    s_dirRegex = new Regex(@"\G\{%(\s*(?<attrname>\w+(?=\W))(\s*(?<equal>=)\s*'(?<attrval>[^']*)'|\s*(?<equal>=)\s*(?<attrval>[^\s%>]*)|(?<equal>)(?<attrval>\s*?)))*\s*?%\}");
                }
                return s_dirRegex;
            }
        }

        internal static Regex ParentAttributeRegex
        {
            get
            {
                if (s_parentAttribRegex == null)
                {
                    s_parentAttribRegex = new Regex(@"\G\$\((?<tagname>[\w:\.]+)\)");
                }
                return s_parentAttribRegex;
            }
        }

        internal static Regex TokenFormatRegex
        {
            get
            {
                if (s_tokenFormatRegex == null)
                {
                    s_tokenFormatRegex = new Regex(@"\G\#\((?<tagname>[\w:\.]+)\)");
                }
                return s_tokenFormatRegex;
            }
        }

        protected delegate string GetValueCallback(string key);
    }
}

