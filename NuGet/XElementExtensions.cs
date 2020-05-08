namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Linq;

    internal static class XElementExtensions
    {
        private static void AddContents<T>(Queue<T> pendingComments, Action<T> action)
        {
            while (pendingComments.Count > 0)
            {
                action(pendingComments.Dequeue());
            }
        }

        public static void AddIndented(this XContainer container, XContainer content)
        {
            string oneIndentLevel = container.ComputeOneLevelOfIndentation();
            XText previousNode = container.PreviousNode as XText;
            string containerIndent = (previousNode != null) ? previousNode.Value : Environment.NewLine;
            content.IndentChildrenElements(containerIndent + oneIndentLevel, oneIndentLevel);
            AddLeadingIndentation(container, containerIndent, oneIndentLevel);
            container.Add(content);
            AddTrailingIndentation(container, containerIndent);
        }

        private static void AddLeadingIndentation(XContainer container, string containerIndent, string oneIndentLevel)
        {
            XText lastNode = container.LastNode as XText;
            if (container.Nodes().Any<XNode>() && (lastNode != null))
            {
                lastNode.Value = lastNode.Value + oneIndentLevel;
            }
            else
            {
                container.Add(new XText(containerIndent + oneIndentLevel));
            }
        }

        private static void AddTrailingIndentation(XContainer container, string containerIndent)
        {
            container.Add(new XText(containerIndent));
        }

        private static bool AttributeEquals(XAttribute source, XAttribute target) => 
            (((source != null) || (target != null)) ? ((source != null) && ((target != null) && ((source.Name == target.Name) && (source.Value == target.Value)))) : true);

        private static int Compare(XElement target, XElement left, XElement right)
        {
            int num = CountMatches(left, target, new Func<XAttribute, XAttribute, bool>(XElementExtensions.AttributeEquals));
            int num2 = CountMatches(right, target, new Func<XAttribute, XAttribute, bool>(XElementExtensions.AttributeEquals));
            if (num != num2)
            {
                return num2.CompareTo(num);
            }
            int num3 = CountMatches(left, target, (a, b) => a.Name == b.Name);
            return CountMatches(right, target, (a, b) => a.Name == b.Name).CompareTo(num3);
        }

        private static string ComputeOneLevelOfIndentation(this XNode node)
        {
            int num = node.Ancestors().Count<XElement>();
            XText previousNode = node.PreviousNode as XText;
            if ((num == 0) || ((previousNode == null) || !previousNode.IsWhiteSpace()))
            {
                return "  ";
            }
            string source = previousNode.Value.Trim(Environment.NewLine.ToCharArray());
            return new string((source.LastOrDefault<char>() == '\t') ? '\t' : ' ', Math.Max(1, source.Length / num));
        }

        private static int CountMatches(XElement left, XElement right, Func<XAttribute, XAttribute, bool> matcher) => 
            (from la in left.Attributes()
                from ta in right.Attributes()
                where matcher(la, ta)
                select la).Count<XAttribute>();

        public static IEnumerable<XElement> ElementsNoNamespace(this IEnumerable<XContainer> source, string localName) => 
            (from e in source.Elements<XContainer>()
                where e.Name.LocalName == localName
                select e);

        public static IEnumerable<XElement> ElementsNoNamespace(this XContainer container, string localName) => 
            (from e in container.Elements()
                where e.Name.LocalName == localName
                select e);

        public static XElement Except(this XElement source, XElement target)
        {
            if (target != null)
            {
                using (List<XAttribute>.Enumerator enumerator = (from e in source.Attributes()
                    where AttributeEquals(e, target.Attribute(e.Name))
                    select e).ToList<XAttribute>().GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current.Remove();
                    }
                }
                foreach (XNode node in source.Nodes().ToList<XNode>())
                {
                    XComment comment = node as XComment;
                    if (comment != null)
                    {
                        if (!HasComment(target, comment))
                        {
                            continue;
                        }
                        comment.Remove();
                        continue;
                    }
                    XElement targetChild = node as XElement;
                    if (targetChild != null)
                    {
                        XElement element2 = FindElement(target, targetChild);
                        if ((element2 != null) && !HasConflict(targetChild, element2))
                        {
                            targetChild.Except(element2);
                            if (!(targetChild.HasAttributes || targetChild.HasElements))
                            {
                                targetChild.Remove();
                                element2.Remove();
                            }
                        }
                    }
                }
            }
            return source;
        }

        private static XElement FindElement(XElement source, XElement targetChild)
        {
            List<XElement> list1 = source.Elements(targetChild.Name).ToList<XElement>();
            list1.Sort((a, b) => Compare(targetChild, a, b));
            return list1.FirstOrDefault<XElement>();
        }

        public static string GetOptionalAttributeValue(this XElement element, string localName, string namespaceName = null)
        {
            XAttribute attribute = !string.IsNullOrEmpty(namespaceName) ? element.Attribute(XName.Get(localName, namespaceName)) : element.Attribute(localName);
            return attribute?.Value;
        }

        public static string GetOptionalElementValue(this XContainer element, string localName, string namespaceName = null)
        {
            XElement element2 = !string.IsNullOrEmpty(namespaceName) ? element.Element(XName.Get(localName, namespaceName)) : element.ElementsNoNamespace(localName).FirstOrDefault<XElement>();
            return element2?.Value;
        }

        private static bool HasComment(XElement element, XComment comment) => 
            Enumerable.Any<XNode>(element.Nodes(), node => (node.NodeType == XmlNodeType.Comment) && ((XComment) node).Value.Equals(comment.Value, StringComparison.Ordinal));

        private static bool HasConflict(XElement source, XElement target)
        {
            bool flag;
            Dictionary<XName, string> dictionary = Enumerable.ToDictionary<XAttribute, XName, string>(source.Attributes(), a => a.Name, a => a.Value);
            using (IEnumerator<XAttribute> enumerator = target.Attributes().GetEnumerator())
            {
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        string str;
                        XAttribute current = enumerator.Current;
                        if (!dictionary.TryGetValue(current.Name, out str) || (str == current.Value))
                        {
                            continue;
                        }
                        flag = true;
                    }
                    else
                    {
                        return false;
                    }
                    break;
                }
            }
            return flag;
        }

        private static void IndentChildrenElements(this XContainer container, string containerIndent, string oneIndentLevel)
        {
            string str = containerIndent + oneIndentLevel;
            foreach (XElement local1 in container.Elements())
            {
                local1.AddBeforeSelf(new XText(str));
                local1.IndentChildrenElements(str + oneIndentLevel, oneIndentLevel);
            }
            if (container.Elements().Any<XElement>())
            {
                container.Add(new XText(containerIndent));
            }
        }

        private static bool IsWhiteSpace(this XText textNode) => 
            string.IsNullOrWhiteSpace(textNode.Value);

        public static XElement MergeWith(this XElement source, XElement target) => 
            source.MergeWith(target, null);

        public static XElement MergeWith(this XElement source, XElement target, IDictionary<XName, Action<XElement, XElement>> nodeActions)
        {
            if (target != null)
            {
                foreach (XAttribute attribute in target.Attributes())
                {
                    if (source.Attribute(attribute.Name) == null)
                    {
                        source.Add(attribute);
                    }
                }
                Queue<XComment> pendingComments = new Queue<XComment>();
                foreach (XNode node in target.Nodes())
                {
                    XComment item = node as XComment;
                    if (item != null)
                    {
                        pendingComments.Enqueue(item);
                        continue;
                    }
                    XElement targetChild = node as XElement;
                    if (targetChild != null)
                    {
                        XElement element2 = FindElement(source, targetChild);
                        if (element2 != null)
                        {
                            AddContents<XComment>(pendingComments, new Action<XComment>(element2.AddBeforeSelf));
                        }
                        if ((element2 != null) && !HasConflict(element2, targetChild))
                        {
                            element2.MergeWith(targetChild, nodeActions);
                        }
                        else
                        {
                            Action<XElement, XElement> action;
                            if ((nodeActions != null) && nodeActions.TryGetValue(targetChild.Name, out action))
                            {
                                action(source, targetChild);
                            }
                            else
                            {
                                source.Add(targetChild);
                                AddContents<XComment>(pendingComments, new Action<XComment>(source.Elements().Last<XElement>().AddBeforeSelf));
                            }
                        }
                    }
                }
                AddContents<XComment>(pendingComments, new Action<XComment>(source.Add));
            }
            return source;
        }

        public static void RemoveAttributes(this XElement element, Func<XAttribute, bool> condition)
        {
            Enumerable.Where<XAttribute>(element.Attributes(), condition).ToList<XAttribute>().Remove();
            element.Descendants().ToList<XElement>().ForEach(e => e.RemoveAttributes(condition));
        }

        public static void RemoveIndented(this XNode element)
        {
            XText previousNode = element.PreviousNode as XText;
            XText nextNode = element.NextNode as XText;
            string str = element.ComputeOneLevelOfIndentation();
            element.Remove();
            if ((nextNode != null) && nextNode.IsWhiteSpace())
            {
                nextNode.Remove();
            }
            if (!element.ElementsAfterSelf().Any<XElement>() && ((previousNode != null) && previousNode.IsWhiteSpace()))
            {
                previousNode.Value = previousNode.Value.Substring(0, previousNode.Value.Length - str.Length);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly XElementExtensions.<>c <>9 = new XElementExtensions.<>c();
            public static Func<XAttribute, XAttribute, bool> <>9__9_0;
            public static Func<XAttribute, XAttribute, bool> <>9__9_1;
            public static Func<XAttribute, XAttribute, <>f__AnonymousType33<XAttribute, XAttribute>> <>9__10_1;
            public static Func<<>f__AnonymousType33<XAttribute, XAttribute>, XAttribute> <>9__10_3;
            public static Func<XAttribute, XName> <>9__11_0;
            public static Func<XAttribute, string> <>9__11_1;

            internal bool <Compare>b__9_0(XAttribute a, XAttribute b) => 
                (a.Name == b.Name);

            internal bool <Compare>b__9_1(XAttribute a, XAttribute b) => 
                (a.Name == b.Name);

            internal <>f__AnonymousType33<XAttribute, XAttribute> <CountMatches>b__10_1(XAttribute la, XAttribute ta) => 
                new { 
                    la = la,
                    ta = ta
                };

            internal XAttribute <CountMatches>b__10_3(<>f__AnonymousType33<XAttribute, XAttribute> <>h__TransparentIdentifier0) => 
                <>h__TransparentIdentifier0.la;

            internal XName <HasConflict>b__11_0(XAttribute a) => 
                a.Name;

            internal string <HasConflict>b__11_1(XAttribute a) => 
                a.Value;
        }
    }
}

