namespace NuGet
{
    using Microsoft.Web.XmlTransform;
    using NuGet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    internal class XdtTransformer : IPackageFileTransformer
    {
        private static void PerformXdtTransform(IPackageFile file, string targetPath, IProjectSystem projectSystem)
        {
            if (projectSystem.FileExists(targetPath))
            {
                string transform = Preprocessor.Process(file, projectSystem);
                try
                {
                    using (XmlTransformation transformation = new XmlTransformation(transform, false, null))
                    {
                        using (XmlTransformableDocument document = new XmlTransformableDocument())
                        {
                            document.PreserveWhitespace = true;
                            using (Stream stream = projectSystem.OpenFile(targetPath))
                            {
                                document.Load(stream);
                            }
                            if (transformation.Apply(document))
                            {
                                using (MemoryStream stream2 = new MemoryStream())
                                {
                                    document.Save(stream2);
                                    stream2.Seek(0L, SeekOrigin.Begin);
                                    using (Stream stream3 = projectSystem.CreateFile(targetPath))
                                    {
                                        stream2.CopyTo(stream3);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    object[] args = new object[] { targetPath, projectSystem.ProjectName };
                    throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, NuGetResources.XdtError + " " + exception.Message, args), exception);
                }
            }
        }

        public void RevertFile(IPackageFile file, string targetPath, IEnumerable<IPackageFile> matchingFiles, IProjectSystem projectSystem)
        {
            PerformXdtTransform(file, targetPath, projectSystem);
        }

        public void TransformFile(IPackageFile file, string targetPath, IProjectSystem projectSystem)
        {
            PerformXdtTransform(file, targetPath, projectSystem);
        }
    }
}

