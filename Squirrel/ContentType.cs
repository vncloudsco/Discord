namespace Squirrel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Xml;

    internal static class ContentType
    {
        public static void Merge(XmlDocument doc)
        {
            Tuple<string, string, string>[] tupleArray1 = new Tuple<string, string, string>[] { Tuple.Create<string, string, string>("Default", "diff", "application/octet"), Tuple.Create<string, string, string>("Default", "bsdiff", "application/octet"), Tuple.Create<string, string, string>("Default", "exe", "application/octet"), Tuple.Create<string, string, string>("Default", "dll", "application/octet"), Tuple.Create<string, string, string>("Default", "shasum", "text/plain") };
            XmlNode typesElement = doc.FirstChild.NextSibling;
            if (typesElement.Name.ToLowerInvariant() != "types")
            {
                throw new Exception("Invalid ContentTypes file, expected root node should be 'Types'");
            }
            IEnumerable<Tuple<string, string, string>> existingTypes = from k in typesElement.ChildNodes.OfType<XmlElement>() select Tuple.Create<string, string, string>(k.Name, k.GetAttribute("Extension").ToLowerInvariant(), k.GetAttribute("ContentType").ToLowerInvariant());
            foreach (XmlElement element in Enumerable.Select<Tuple<string, string, string>, XmlElement>(from x in tupleArray1
                where Enumerable.All<Tuple<string, string, string>>(existingTypes, t => t.Item2 != x.Item2.ToLowerInvariant())
                select x, delegate (Tuple<string, string, string> element) {
                XmlAttribute node = doc.CreateAttribute("Extension")node.Value = element.Item2XmlAttribute attribute2 = doc.CreateAttribute("ContentType")attribute2.Value = element.Item3XmlElement element1 = doc.CreateElement(element.Item1, typesElement.NamespaceURI)element1.Attributes.Append(node)element1.Attributes.Append(attribute2)return element1;
            }))
            {
                typesElement.AppendChild(element);
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ContentType.<>c <>9 = new ContentType.<>c();
            public static Func<XmlElement, Tuple<string, string, string>> <>9__0_0;

            internal Tuple<string, string, string> <Merge>b__0_0(XmlElement k) => 
                Tuple.Create<string, string, string>(k.Name, k.GetAttribute("Extension").ToLowerInvariant(), k.GetAttribute("ContentType").ToLowerInvariant());
        }
    }
}

