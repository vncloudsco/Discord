namespace NuGet
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Linq;

    internal static class XmlUtility
    {
        public static XDocument CreateDocument(XName rootName, IFileSystem fileSystem, string path)
        {
            object[] content = new object[] { new XElement(rootName) };
            XDocument document = new XDocument(content);
            fileSystem.AddFile(path, new Action<Stream>(document.Save));
            return document;
        }

        private static XmlReaderSettings CreateSafeSettings(bool ignoreWhiteSpace = false)
        {
            XmlReaderSettings settings1 = new XmlReaderSettings();
            settings1.XmlResolver = null;
            settings1.DtdProcessing = DtdProcessing.Prohibit;
            settings1.IgnoreWhitespace = ignoreWhiteSpace;
            return settings1;
        }

        internal static XDocument GetDocument(IFileSystem fileSystem, string path)
        {
            using (Stream stream = fileSystem.OpenFile(path))
            {
                return LoadSafe(stream, LoadOptions.PreserveWhitespace);
            }
        }

        internal static XDocument GetOrCreateDocument(XName rootName, IFileSystem fileSystem, string path)
        {
            if (!fileSystem.FileExists(path))
            {
                return CreateDocument(rootName, fileSystem, path);
            }
            try
            {
                return GetDocument(fileSystem, path);
            }
            catch (FileNotFoundException)
            {
                return CreateDocument(rootName, fileSystem, path);
            }
        }

        public static XDocument LoadSafe(Stream input)
        {
            XmlReaderSettings settings = CreateSafeSettings(false);
            return XDocument.Load(XmlReader.Create(input, settings));
        }

        public static XDocument LoadSafe(string filePath)
        {
            XmlReaderSettings settings = CreateSafeSettings(false);
            using (XmlReader reader = XmlReader.Create(filePath, settings))
            {
                return XDocument.Load(reader);
            }
        }

        public static XDocument LoadSafe(Stream input, bool ignoreWhiteSpace)
        {
            XmlReaderSettings settings = CreateSafeSettings(ignoreWhiteSpace);
            return XDocument.Load(XmlReader.Create(input, settings));
        }

        public static XDocument LoadSafe(Stream input, LoadOptions options)
        {
            XmlReaderSettings settings = CreateSafeSettings(false);
            return XDocument.Load(XmlReader.Create(input, settings), options);
        }

        internal static bool TryParseDocument(string content, out XDocument document)
        {
            document = null;
            try
            {
                document = XDocument.Parse(content);
                return true;
            }
            catch (XmlException)
            {
                return false;
            }
        }
    }
}

