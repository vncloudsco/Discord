namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    internal class XmlTransformer : IPackageFileTransformer
    {
        private readonly IDictionary<XName, Action<XElement, XElement>> _nodeActions;

        public XmlTransformer(IDictionary<XName, Action<XElement, XElement>> nodeActions)
        {
            this._nodeActions = nodeActions;
        }

        private static XElement GetXml(IPackageFile file, IProjectSystem projectSystem) => 
            XElement.Parse(Preprocessor.Process(file, projectSystem), LoadOptions.PreserveWhitespace);

        public virtual void RevertFile(IPackageFile file, string targetPath, IEnumerable<IPackageFile> matchingFiles, IProjectSystem projectSystem)
        {
            XElement xml = GetXml(file, projectSystem);
            XDocument document = XmlUtility.GetOrCreateDocument(xml.Name, projectSystem, targetPath);
            document.Root.Except(xml.Except(Enumerable.Aggregate<XElement, XElement>(from f in matchingFiles select GetXml(f, projectSystem), new XElement(xml.Name), (left, right) => left.MergeWith(right, this._nodeActions))));
            using (Stream stream = projectSystem.CreateFile(targetPath))
            {
                document.Save(stream);
            }
        }

        public virtual void TransformFile(IPackageFile file, string targetPath, IProjectSystem projectSystem)
        {
            XElement xml = GetXml(file, projectSystem);
            XDocument document = XmlUtility.GetOrCreateDocument(xml.Name, projectSystem, targetPath);
            document.Root.MergeWith(xml, this._nodeActions);
            projectSystem.AddFile(targetPath, new Action<Stream>(document.Save));
        }
    }
}

