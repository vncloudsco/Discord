namespace Squirrel
{
    using ICSharpCode.SharpZipLib.Core;
    using ICSharpCode.SharpZipLib.Zip;
    using MarkdownSharp;
    using NuGet;
    using Splat;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Text;
    using System.Xml;

    internal class ReleasePackage : IEnableLogger, IReleasePackage
    {
        public ReleasePackage(string inputPackageFile, bool isReleasePackage = false)
        {
            this.InputPackageFile = inputPackageFile;
            if (isReleasePackage)
            {
                this.ReleasePackageFile = inputPackageFile;
            }
        }

        internal static void addDeltaFilesToContentTypes(string rootDirectory)
        {
            XmlDocument doc = new XmlDocument();
            string filename = Path.Combine(rootDirectory, "[Content_Types].xml");
            doc.Load(filename);
            ContentType.Merge(doc);
            using (StreamWriter writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                doc.Save(writer);
            }
        }

        private void compressFolderEncoded(string path, ZipOutputStream zipStream, int folderOffset)
        {
            string[] files = Directory.GetFiles(path);
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo info = new FileInfo(files[i]);
                string text1 = files[i];
                ZipEntry entry = new ZipEntry(Uri.EscapeUriString(ZipEntry.CleanName(text1.Substring(folderOffset)))) {
                    DateTime = info.LastWriteTime,
                    Size = info.Length
                };
                zipStream.PutNextEntry(entry);
                byte[] buffer = new byte[0x1000];
                using (FileStream stream = File.OpenRead(text1))
                {
                    StreamUtils.Copy(stream, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }
            foreach (string str in Directory.GetDirectories(path))
            {
                this.compressFolderEncoded(str, zipStream, folderOffset);
            }
        }

        public string CreateReleasePackage(string outputFile, string packagesRootDir = null, Func<string, string> releaseNotesProcessor = null, Action<string> contentsPostProcessHook = null)
        {
            releaseNotesProcessor = releaseNotesProcessor ?? x => new Markdown().Transform(x);
            if (this.ReleasePackageFile != null)
            {
                return this.ReleasePackageFile;
            }
            ZipPackage package = new ZipPackage(this.InputPackageFile);
            IEnumerable<FrameworkName> supportedFrameworks = package.GetSupportedFrameworks();
            if (supportedFrameworks.Count<FrameworkName>() > 1)
            {
                StringBuilder builder = Enumerable.Aggregate<FrameworkName, StringBuilder>(supportedFrameworks, new StringBuilder(), (sb, f) => sb.Append(f.ToString() + "; "));
                throw new InvalidOperationException($"The input package file {this.InputPackageFile} targets multiple platforms - {builder} - and cannot be transformed into a release package.");
            }
            if (!supportedFrameworks.Any<FrameworkName>())
            {
                throw new InvalidOperationException($"The input package file {this.InputPackageFile} targets no platform and cannot be transformed into a release package.");
            }
            FrameworkName frameworkName = supportedFrameworks.Single<FrameworkName>();
            this.Log<ReleasePackage>().Info<string, string>("Creating release package: {0} => {1}", this.InputPackageFile, outputFile);
            IEnumerable<IPackage> dependencies = this.findAllDependentPackages(package, new LocalPackageRepository(packagesRootDir), null, frameworkName);
            string path = null;
            using (Utility.WithTempDirectory(out path, null))
            {
                DirectoryInfo tempPath = new DirectoryInfo(path);
                this.extractZipDecoded(this.InputPackageFile, path);
                this.Log<ReleasePackage>().Info<string>("Extracting dependent packages: [{0}]", string.Join(",", (IEnumerable<string>) (from x in dependencies select x.Id)));
                this.extractDependentPackages(dependencies, tempPath, frameworkName);
                string fullName = tempPath.GetFiles("*.nuspec").First<FileInfo>().FullName;
                this.Log<ReleasePackage>().Info("Removing unnecessary data");
                this.removeDependenciesFromPackageSpec(fullName);
                this.removeDeveloperDocumentation(tempPath);
                if (releaseNotesProcessor != null)
                {
                    this.renderReleaseNotesMarkdown(fullName, releaseNotesProcessor);
                }
                addDeltaFilesToContentTypes(tempPath.FullName);
                if (contentsPostProcessHook != null)
                {
                    contentsPostProcessHook(path);
                }
                this.createZipEncoded(outputFile, path);
                this.ReleasePackageFile = outputFile;
                return this.ReleasePackageFile;
            }
        }

        private void createZipEncoded(string zipFilePath, string folder)
        {
            folder = Path.GetFullPath(folder);
            ZipOutputStream zipStream = new ZipOutputStream(File.Create(zipFilePath));
            zipStream.SetLevel(5);
            this.compressFolderEncoded(folder, zipStream, folder.Length + (folder.EndsWith(@"\", StringComparison.OrdinalIgnoreCase) ? 1 : 0));
            zipStream.IsStreamOwner = true;
            zipStream.Close();
        }

        private void extractDependentPackages(IEnumerable<IPackage> dependencies, DirectoryInfo tempPath, FrameworkName framework)
        {
            dependencies.ForEach<IPackage>(delegate (IPackage pkg) {
                Action<IPackageFile> <>9__1;
                this.Log<ReleasePackage>().Info<string>("Scanning {0}", pkg.Id);
                Action<IPackageFile> onNext = <>9__1;
                if (<>9__1 == null)
                {
                    Action<IPackageFile> local1 = <>9__1;
                    onNext = <>9__1 = delegate (IPackageFile file) {
                        FileInfo argument = new FileInfo(Path.Combine(tempPath.FullName, file.Path));
                        FrameworkName[] packageSupportedFrameworks = new FrameworkName[] { file.TargetFramework };
                        if (!VersionUtility.IsCompatible(framework, packageSupportedFrameworks))
                        {
                            this.Log<ReleasePackage>().Info<FileInfo>("Ignoring {0} as the target framework is not compatible", argument);
                        }
                        else
                        {
                            Directory.CreateDirectory(argument.Directory.FullName);
                            using (FileStream stream = File.Create(argument.FullName))
                            {
                                this.Log<ReleasePackage>().Info<string, FileInfo>("Writing {0} to {1}", file.Path, argument);
                                file.GetStream().CopyTo(stream);
                            }
                        }
                    };
                }
                pkg.GetLibFiles().ForEach<IPackageFile>(onNext);
            });
        }

        private void extractZipDecoded(string zipFilePath, string outFolder)
        {
            ZipFile file = new ZipFile(zipFilePath);
            foreach (ZipEntry entry in file)
            {
                if (entry.IsFile)
                {
                    string str = Uri.UnescapeDataString(entry.Name);
                    byte[] buffer = new byte[0x1000];
                    Stream inputStream = file.GetInputStream(entry);
                    string path = Path.Combine(outFolder, str);
                    string directoryName = Path.GetDirectoryName(path);
                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                    using (FileStream stream2 = File.Create(path))
                    {
                        StreamUtils.Copy(inputStream, stream2, buffer);
                    }
                }
            }
            file.Close();
        }

        internal IEnumerable<IPackage> findAllDependentPackages(IPackage package = null, IPackageRepository packageRepository = null, HashSet<string> packageCache = null, FrameworkName frameworkName = null)
        {
            package = package ?? new ZipPackage(this.InputPackageFile);
            packageCache = packageCache ?? new HashSet<string>();
            return Enumerable.SelectMany<PackageDependency, IPackage>(from x in package.DependencySets
                where (x.TargetFramework == null) || (x.TargetFramework == frameworkName)
                select x.Dependencies, delegate (PackageDependency dependency) {
                IPackage package = this.matchPackage(packageRepository, dependency.Id, dependency.VersionSpec);
                if (package == null)
                {
                    string message = string.Format("Couldn't find file for package in {1}: {0}", dependency.Id, packageRepository.Source);
                    this.Log<ReleasePackage>().Error(message);
                    throw new Exception(message);
                }
                if (packageCache.Contains(package.GetFullName()))
                {
                    return Enumerable.Empty<IPackage>();
                }
                packageCache.Add(package.GetFullName());
                IPackage[] values = new IPackage[] { package };
                return this.findAllDependentPackages(package, packageRepository, packageCache, frameworkName).StartWith<IPackage>(values).Distinct<IPackage, string>(y => y.GetFullName());
            }).ToArray<IPackage>();
        }

        private IPackage matchPackage(IPackageRepository packageRepository, string id, IVersionSpec version) => 
            Enumerable.FirstOrDefault<IPackage>(packageRepository.FindPackagesById(id), x => VersionComparer.Matches(version, x.Version));

        private void removeDependenciesFromPackageSpec(string specPath)
        {
            XmlDocument document1 = new XmlDocument();
            document1.Load(specPath);
            XmlNode firstChild = document1.DocumentElement.FirstChild;
            XmlElement oldChild = Enumerable.FirstOrDefault<XmlElement>(firstChild.ChildNodes.OfType<XmlElement>(), x => x.Name.ToLowerInvariant() == "dependencies");
            if (oldChild != null)
            {
                firstChild.RemoveChild(oldChild);
            }
            document1.Save(specPath);
        }

        private void removeDeveloperDocumentation(DirectoryInfo expandedRepoPath)
        {
            (from x in expandedRepoPath.GetAllFilesRecursively()
                where x.Name.EndsWith(".dll", true, CultureInfo.InvariantCulture)
                select new FileInfo(x.FullName.ToLowerInvariant().Replace(".dll", ".xml")) into x
                where x.Exists
                select x).ForEach<FileInfo>(x => x.Delete());
        }

        private void renderReleaseNotesMarkdown(string specPath, Func<string, string> releaseNotesProcessor)
        {
            XmlDocument document = new XmlDocument();
            document.Load(specPath);
            XmlElement element = Enumerable.FirstOrDefault<XmlElement>(Enumerable.First<XmlElement>(document.DocumentElement.ChildNodes.OfType<XmlElement>(), x => x.Name.ToLowerInvariant() == "metadata").ChildNodes.OfType<XmlElement>(), x => x.Name.ToLowerInvariant() == "releasenotes");
            if (element == null)
            {
                this.Log<ReleasePackage>().Info<string>("No release notes found in {0}", specPath);
            }
            else
            {
                element.InnerText = $"<![CDATA[
{releaseNotesProcessor(element.InnerText)}
]]>";
                document.Save(specPath);
            }
        }

        public string InputPackageFile { get; protected set; }

        public string ReleasePackageFile { get; protected set; }

        public string SuggestedReleaseFileName
        {
            get
            {
                ZipPackage package = new ZipPackage(this.InputPackageFile);
                return $"{package.Id}-{package.Version}-full.nupkg";
            }
        }

        public SemanticVersion Version =>
            this.InputPackageFile.ToSemanticVersion();

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ReleasePackage.<>c <>9 = new ReleasePackage.<>c();
            public static Func<string, string> <>9__13_0;
            public static Func<StringBuilder, FrameworkName, StringBuilder> <>9__13_1;
            public static Func<IPackage, string> <>9__13_2;
            public static Func<FileInfo, bool> <>9__18_0;
            public static Func<FileInfo, FileInfo> <>9__18_1;
            public static Func<FileInfo, bool> <>9__18_2;
            public static Action<FileInfo> <>9__18_3;
            public static Func<XmlElement, bool> <>9__19_0;
            public static Func<XmlElement, bool> <>9__19_1;
            public static Func<XmlElement, bool> <>9__20_0;
            public static Func<PackageDependencySet, IEnumerable<PackageDependency>> <>9__21_1;
            public static Func<IPackage, string> <>9__21_3;

            internal string <CreateReleasePackage>b__13_0(string x) => 
                new Markdown().Transform(x);

            internal StringBuilder <CreateReleasePackage>b__13_1(StringBuilder sb, FrameworkName f) => 
                sb.Append(f.ToString() + "; ");

            internal string <CreateReleasePackage>b__13_2(IPackage x) => 
                x.Id;

            internal IEnumerable<PackageDependency> <findAllDependentPackages>b__21_1(PackageDependencySet x) => 
                x.Dependencies;

            internal string <findAllDependentPackages>b__21_3(IPackage y) => 
                y.GetFullName();

            internal bool <removeDependenciesFromPackageSpec>b__20_0(XmlElement x) => 
                (x.Name.ToLowerInvariant() == "dependencies");

            internal bool <removeDeveloperDocumentation>b__18_0(FileInfo x) => 
                x.Name.EndsWith(".dll", true, CultureInfo.InvariantCulture);

            internal FileInfo <removeDeveloperDocumentation>b__18_1(FileInfo x) => 
                new FileInfo(x.FullName.ToLowerInvariant().Replace(".dll", ".xml"));

            internal bool <removeDeveloperDocumentation>b__18_2(FileInfo x) => 
                x.Exists;

            internal void <removeDeveloperDocumentation>b__18_3(FileInfo x)
            {
                x.Delete();
            }

            internal bool <renderReleaseNotesMarkdown>b__19_0(XmlElement x) => 
                (x.Name.ToLowerInvariant() == "metadata");

            internal bool <renderReleaseNotesMarkdown>b__19_1(XmlElement x) => 
                (x.Name.ToLowerInvariant() == "releasenotes");
        }
    }
}

