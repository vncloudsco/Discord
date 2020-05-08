namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Xml;
    using System.Xml.Linq;

    internal class PackageReferenceFile
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _path;
        private readonly Dictionary<string, string> _constraints;
        private readonly Dictionary<string, string> _developmentFlags;

        public PackageReferenceFile(string path) : this(new PhysicalFileSystem(Path.GetDirectoryName(path)), Path.GetFileName(path))
        {
        }

        public PackageReferenceFile(IFileSystem fileSystem, string path) : this(fileSystem, path, null)
        {
        }

        public PackageReferenceFile(IFileSystem fileSystem, string path, string projectName)
        {
            this._constraints = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this._developmentFlags = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (fileSystem == null)
            {
                throw new ArgumentNullException("fileSystem");
            }
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "path");
            }
            this._fileSystem = fileSystem;
            if (!string.IsNullOrEmpty(projectName))
            {
                string str = ConstructPackagesConfigFromProjectName(projectName);
                if (this._fileSystem.FileExists(str))
                {
                    this._path = str;
                }
            }
            if (this._path == null)
            {
                this._path = path;
            }
        }

        public void AddEntry(string id, SemanticVersion version)
        {
            this.AddEntry(id, version, false);
        }

        public void AddEntry(string id, SemanticVersion version, bool developmentDependency)
        {
            this.AddEntry(id, version, developmentDependency, null);
        }

        public void AddEntry(string id, SemanticVersion version, bool developmentDependency, FrameworkName targetFramework)
        {
            XDocument document = this.GetDocument(true);
            this.AddEntry(document, id, version, developmentDependency, targetFramework);
        }

        private void AddEntry(XDocument document, string id, SemanticVersion version, bool developmentDependency, FrameworkName targetFramework)
        {
            this.AddEntry(document, id, version, developmentDependency, targetFramework, false);
        }

        private void AddEntry(XDocument document, string id, SemanticVersion version, bool developmentDependency, FrameworkName targetFramework, bool requireReinstallation)
        {
            string str;
            string str2;
            XElement element = FindEntry(document, id, version);
            if (element != null)
            {
                element.Remove();
            }
            object[] content = new object[] { new XAttribute("id", id), new XAttribute("version", version) };
            XElement element2 = new XElement("package", content);
            if (targetFramework != null)
            {
                element2.Add(new XAttribute("targetFramework", VersionUtility.GetShortFrameworkName(targetFramework)));
            }
            if (this._constraints.TryGetValue(id, out str))
            {
                element2.Add(new XAttribute("allowedVersions", str));
            }
            if (this._developmentFlags.TryGetValue(id, out str2))
            {
                element2.Add(new XAttribute("developmentDependency", str2));
            }
            else if (developmentDependency)
            {
                element2.Add(new XAttribute("developmentDependency", "true"));
            }
            if (requireReinstallation)
            {
                element2.Add(new XAttribute("requireReinstallation", bool.TrueString));
            }
            document.Root.Add(element2);
            this.SaveDocument(document);
        }

        private static string ConstructPackagesConfigFromProjectName(string projectName) => 
            ("packages." + projectName.Replace(' ', '_') + ".config");

        public static PackageReferenceFile CreateFromProject(string projectFileFullPath) => 
            new PackageReferenceFile(new PhysicalFileSystem(Path.GetDirectoryName(projectFileFullPath)), Constants.PackageReferenceFile, Path.GetFileNameWithoutExtension(projectFileFullPath));

        public bool DeleteEntry(string id, SemanticVersion version)
        {
            XDocument document = this.GetDocument(false);
            return ((document != null) ? this.DeleteEntry(document, id, version) : false);
        }

        private bool DeleteEntry(XDocument document, string id, SemanticVersion version)
        {
            XElement element = FindEntry(document, id, version);
            if (element != null)
            {
                string str = element.GetOptionalAttributeValue("allowedVersions", null);
                if (!string.IsNullOrEmpty(str))
                {
                    this._constraints[id] = str;
                }
                string str2 = element.GetOptionalAttributeValue("developmentDependency", null);
                if (!string.IsNullOrEmpty(str2))
                {
                    this._developmentFlags[id] = str2;
                }
                element.Remove();
                this.SaveDocument(document);
                if (!document.Root.HasElements)
                {
                    this._fileSystem.DeleteFile(this._path);
                    return true;
                }
            }
            return false;
        }

        public bool EntryExists(string packageId, SemanticVersion version)
        {
            XDocument document = this.GetDocument(false);
            return ((document != null) ? (FindEntry(document, packageId, version) != null) : false);
        }

        private static XElement FindEntry(XDocument document, string id, SemanticVersion version) => 
            (!string.IsNullOrEmpty(id) ? (from e in document.Root.Elements("package")
                let entryId = e.GetOptionalAttributeValue("id", null)
                let entryVersion = SemanticVersion.ParseOptionalVersion(e.GetOptionalAttributeValue("version", null))
                where (entryId != null) && (entryVersion != null)
                where id.Equals(entryId, StringComparison.OrdinalIgnoreCase) && ((version == null) || entryVersion.Equals(version))
                select e).FirstOrDefault<XElement>() : null);

        private XDocument GetDocument(bool createIfNotExists = false)
        {
            XDocument document;
            try
            {
                if (!this._fileSystem.FileExists(this._path))
                {
                    if (!createIfNotExists)
                    {
                        document = null;
                    }
                    else
                    {
                        object[] content = new object[] { new XElement("packages") };
                        document = new XDocument(content);
                    }
                }
                else
                {
                    using (Stream stream = this._fileSystem.OpenFile(this._path))
                    {
                        document = XmlUtility.LoadSafe(stream);
                    }
                }
            }
            catch (XmlException exception)
            {
                object[] args = new object[] { this.FullPath };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.ErrorReadingFile, args), exception);
            }
            return document;
        }

        public IEnumerable<PackageReference> GetPackageReferences() => 
            this.GetPackageReferences(true);

        [IteratorStateMachine(typeof(<GetPackageReferences>d__10))]
        public IEnumerable<PackageReference> GetPackageReferences(bool requireVersion)
        {
            <GetPackageReferences>d__10 d__1 = new <GetPackageReferences>d__10(-2);
            d__1.<>4__this = this;
            d__1.<>3__requireVersion = requireVersion;
            return d__1;
        }

        public static bool IsValidConfigFileName(string fileName) => 
            ((fileName != null) && (fileName.StartsWith("packages.", StringComparison.OrdinalIgnoreCase) && fileName.EndsWith(".config", StringComparison.OrdinalIgnoreCase)));

        public void MarkEntryForReinstallation(string id, SemanticVersion version, FrameworkName targetFramework, bool requireReinstallation)
        {
            XDocument document = this.GetDocument(false);
            if (document != null)
            {
                this.DeleteEntry(id, version);
                this.AddEntry(document, id, version, false, targetFramework, requireReinstallation);
            }
        }

        private void SaveDocument(XDocument document)
        {
            List<XElement> content = (from e in document.Root.Elements("package")
                let id = e.GetOptionalAttributeValue("id", null)
                let version = e.GetOptionalAttributeValue("version", null)
                where !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(version)
                orderby id
                select e).ToList<XElement>();
            document.Root.RemoveAll();
            document.Root.Add(content);
            this._fileSystem.AddFile(this._path, new Action<Stream>(document.Save));
        }

        public string FullPath =>
            this._fileSystem.GetFullPath(this._path);

        public IFileSystem FileSystem =>
            this._fileSystem;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PackageReferenceFile.<>c <>9 = new PackageReferenceFile.<>c();
            public static Func<XElement, <>f__AnonymousType7<XElement, string>> <>9__23_0;
            public static Func<<>f__AnonymousType7<XElement, string>, <>f__AnonymousType8<<>f__AnonymousType7<XElement, string>, SemanticVersion>> <>9__23_1;
            public static Func<<>f__AnonymousType8<<>f__AnonymousType7<XElement, string>, SemanticVersion>, bool> <>9__23_2;
            public static Func<<>f__AnonymousType8<<>f__AnonymousType7<XElement, string>, SemanticVersion>, XElement> <>9__23_4;
            public static Func<XElement, <>f__AnonymousType9<XElement, string>> <>9__24_0;
            public static Func<<>f__AnonymousType9<XElement, string>, <>f__AnonymousType10<<>f__AnonymousType9<XElement, string>, string>> <>9__24_1;
            public static Func<<>f__AnonymousType10<<>f__AnonymousType9<XElement, string>, string>, bool> <>9__24_2;
            public static Func<<>f__AnonymousType10<<>f__AnonymousType9<XElement, string>, string>, string> <>9__24_3;
            public static Func<<>f__AnonymousType10<<>f__AnonymousType9<XElement, string>, string>, XElement> <>9__24_4;

            internal <>f__AnonymousType7<XElement, string> <FindEntry>b__23_0(XElement e) => 
                new { 
                    e = e,
                    entryId = e.GetOptionalAttributeValue("id", null)
                };

            internal <>f__AnonymousType8<<>f__AnonymousType7<XElement, string>, SemanticVersion> <FindEntry>b__23_1(<>f__AnonymousType7<XElement, string> <>h__TransparentIdentifier0) => 
                new { 
                    <>h__TransparentIdentifier0 = <>h__TransparentIdentifier0,
                    entryVersion = SemanticVersion.ParseOptionalVersion(<>h__TransparentIdentifier0.e.GetOptionalAttributeValue("version", null))
                };

            internal bool <FindEntry>b__23_2(<>f__AnonymousType8<<>f__AnonymousType7<XElement, string>, SemanticVersion> <>h__TransparentIdentifier1) => 
                ((<>h__TransparentIdentifier1.<>h__TransparentIdentifier0.entryId != null) && (<>h__TransparentIdentifier1.entryVersion != null));

            internal XElement <FindEntry>b__23_4(<>f__AnonymousType8<<>f__AnonymousType7<XElement, string>, SemanticVersion> <>h__TransparentIdentifier1) => 
                <>h__TransparentIdentifier1.<>h__TransparentIdentifier0.e;

            internal <>f__AnonymousType9<XElement, string> <SaveDocument>b__24_0(XElement e) => 
                new { 
                    e = e,
                    id = e.GetOptionalAttributeValue("id", null)
                };

            internal <>f__AnonymousType10<<>f__AnonymousType9<XElement, string>, string> <SaveDocument>b__24_1(<>f__AnonymousType9<XElement, string> <>h__TransparentIdentifier0) => 
                new { 
                    <>h__TransparentIdentifier0 = <>h__TransparentIdentifier0,
                    version = <>h__TransparentIdentifier0.e.GetOptionalAttributeValue("version", null)
                };

            internal bool <SaveDocument>b__24_2(<>f__AnonymousType10<<>f__AnonymousType9<XElement, string>, string> <>h__TransparentIdentifier1) => 
                (!string.IsNullOrEmpty(<>h__TransparentIdentifier1.<>h__TransparentIdentifier0.id) && !string.IsNullOrEmpty(<>h__TransparentIdentifier1.version));

            internal string <SaveDocument>b__24_3(<>f__AnonymousType10<<>f__AnonymousType9<XElement, string>, string> <>h__TransparentIdentifier1) => 
                <>h__TransparentIdentifier1.<>h__TransparentIdentifier0.id;

            internal XElement <SaveDocument>b__24_4(<>f__AnonymousType10<<>f__AnonymousType9<XElement, string>, string> <>h__TransparentIdentifier1) => 
                <>h__TransparentIdentifier1.<>h__TransparentIdentifier0.e;
        }

        [CompilerGenerated]
        private sealed class <GetPackageReferences>d__10 : IEnumerable<PackageReference>, IEnumerable, IEnumerator<PackageReference>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private PackageReference <>2__current;
            private int <>l__initialThreadId;
            public PackageReferenceFile <>4__this;
            private bool requireVersion;
            public bool <>3__requireVersion;
            private IEnumerator<XElement> <>7__wrap1;

            [DebuggerHidden]
            public <GetPackageReferences>d__10(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private void <>m__Finally1()
            {
                this.<>1__state = -1;
                if (this.<>7__wrap1 != null)
                {
                    this.<>7__wrap1.Dispose();
                }
            }

            private bool MoveNext()
            {
                bool flag;
                try
                {
                    int num = this.<>1__state;
                    if (num == 0)
                    {
                        this.<>1__state = -1;
                        XDocument document = this.<>4__this.GetDocument(false);
                        if (document != null)
                        {
                            this.<>7__wrap1 = document.Root.Elements("package").GetEnumerator();
                            this.<>1__state = -3;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (num == 1)
                    {
                        this.<>1__state = -3;
                    }
                    else
                    {
                        return false;
                    }
                    while (true)
                    {
                        if (!this.<>7__wrap1.MoveNext())
                        {
                            this.<>m__Finally1();
                            this.<>7__wrap1 = null;
                            flag = false;
                        }
                        else
                        {
                            XElement current = this.<>7__wrap1.Current;
                            string str = current.GetOptionalAttributeValue("id", null);
                            string str2 = current.GetOptionalAttributeValue("version", null);
                            string str3 = current.GetOptionalAttributeValue("allowedVersions", null);
                            string str4 = current.GetOptionalAttributeValue("targetFramework", null);
                            string str5 = current.GetOptionalAttributeValue("developmentDependency", null);
                            string str6 = current.GetOptionalAttributeValue("requireReinstallation", null);
                            SemanticVersion version = null;
                            if (string.IsNullOrEmpty(str))
                            {
                                continue;
                            }
                            if ((this.requireVersion || !string.IsNullOrEmpty(str2)) && !SemanticVersion.TryParse(str2, out version))
                            {
                                object[] args = new object[] { str2, this.<>4__this._path };
                                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, NuGetResources.ReferenceFile_InvalidVersion, args));
                            }
                            IVersionSpec result = null;
                            if (!string.IsNullOrEmpty(str3))
                            {
                                if (!VersionUtility.TryParseVersionSpec(str3, out result))
                                {
                                    object[] args = new object[] { str3, this.<>4__this._path };
                                    throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, NuGetResources.ReferenceFile_InvalidVersion, args));
                                }
                                this.<>4__this._constraints[str] = str3;
                            }
                            FrameworkName targetFramework = null;
                            if (!string.IsNullOrEmpty(str4) && (VersionUtility.ParseFrameworkName(str4) == VersionUtility.UnsupportedFrameworkName))
                            {
                                targetFramework = null;
                            }
                            bool flag2 = false;
                            if (!string.IsNullOrEmpty(str5))
                            {
                                if (!bool.TryParse(str5, out flag2))
                                {
                                    object[] args = new object[] { str5, this.<>4__this._path };
                                    throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, NuGetResources.ReferenceFile_InvalidDevelopmentFlag, args));
                                }
                                this.<>4__this._developmentFlags[str] = str5;
                            }
                            bool flag3 = false;
                            if (!string.IsNullOrEmpty(str6) && !bool.TryParse(str6, out flag3))
                            {
                                object[] args = new object[] { str6, this.<>4__this._path };
                                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, NuGetResources.ReferenceFile_InvalidRequireReinstallationFlag, args));
                            }
                            this.<>2__current = new PackageReference(str, version, result, targetFramework, flag2, flag3);
                            this.<>1__state = 1;
                            flag = true;
                        }
                        break;
                    }
                }
                fault
                {
                    this.System.IDisposable.Dispose();
                }
                return flag;
            }

            [DebuggerHidden]
            IEnumerator<PackageReference> IEnumerable<PackageReference>.GetEnumerator()
            {
                PackageReferenceFile.<GetPackageReferences>d__10 d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Environment.CurrentManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                else
                {
                    d__ = new PackageReferenceFile.<GetPackageReferences>d__10(0) {
                        <>4__this = this.<>4__this
                    };
                }
                d__.requireVersion = this.<>3__requireVersion;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<NuGet.PackageReference>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
                int num = this.<>1__state;
                if ((num == -3) || (num == 1))
                {
                    try
                    {
                    }
                    finally
                    {
                        this.<>m__Finally1();
                    }
                }
            }

            PackageReference IEnumerator<PackageReference>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

