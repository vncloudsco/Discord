namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;

    internal class UnzippedPackage : LocalPackage
    {
        private readonly IFileSystem _repositoryFileSystem;
        private readonly string _packageFileName;
        private readonly string _packageName;

        public UnzippedPackage(IFileSystem repositoryFileSystem, string packageName)
        {
            if (repositoryFileSystem == null)
            {
                throw new ArgumentNullException("repositoryFileSystem");
            }
            if (string.IsNullOrEmpty(packageName))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "packageName");
            }
            this._packageName = packageName;
            this._packageFileName = packageName + Constants.PackageExtension;
            this._repositoryFileSystem = repositoryFileSystem;
            this.EnsureManifest();
        }

        public UnzippedPackage(string repositoryDirectory, string packageName) : this(new PhysicalFileSystem(repositoryDirectory), packageName)
        {
        }

        private void EnsureManifest()
        {
            string path = Path.Combine(this._packageName, this._packageName + Constants.ManifestExtension);
            if (!this._repositoryFileSystem.FileExists(path))
            {
                object[] args = new object[] { this._repositoryFileSystem.GetFullPath(path) };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.Manifest_NotFound, args));
            }
            using (Stream stream = this._repositoryFileSystem.OpenFile(path))
            {
                base.ReadManifest(stream);
            }
            base.Published = new DateTimeOffset?(this._repositoryFileSystem.GetLastModified(path));
        }

        protected override IEnumerable<IPackageAssemblyReference> GetAssemblyReferencesCore()
        {
            string path = Path.Combine(this._packageName, Constants.LibDirectory);
            return (IEnumerable<IPackageAssemblyReference>) Enumerable.Select(from p in this._repositoryFileSystem.GetFiles(path, "*.*", true)
                let targetPath = this.GetPackageRelativePath(p)
                where IsAssemblyReference(targetPath)
                select <>h__TransparentIdentifier0, delegate (<>f__AnonymousType4<string, string> <>h__TransparentIdentifier0) {
                PhysicalPackageAssemblyReference reference1 = new PhysicalPackageAssemblyReference();
                reference1.SourcePath = this._repositoryFileSystem.GetFullPath(<>h__TransparentIdentifier0.p);
                reference1.TargetPath = <>h__TransparentIdentifier0.targetPath;
                return reference1;
            });
        }

        protected override IEnumerable<IPackageFile> GetFilesBase() => 
            ((IEnumerable<IPackageFile>) Enumerable.Select<string, PhysicalPackageFile>(this.GetPackageFilePaths(), delegate (string p) {
                PhysicalPackageFile file1 = new PhysicalPackageFile();
                file1.SourcePath = this._repositoryFileSystem.GetFullPath(p);
                file1.TargetPath = this.GetPackageRelativePath(p);
                return file1;
            }));

        private IEnumerable<string> GetPackageFilePaths() => 
            (from p in this._repositoryFileSystem.GetFiles(this._packageName, "*.*", true)
                where !PackageHelper.IsManifest(p) && !PackageHelper.IsPackageFile(p)
                select p);

        private string GetPackageRelativePath(string path) => 
            path.Substring(this._packageName.Length + 1);

        public override Stream GetStream()
        {
            if (this._repositoryFileSystem.FileExists(this._packageFileName))
            {
                return this._repositoryFileSystem.OpenFile(this._packageFileName);
            }
            string path = Path.Combine(this._packageName, this._packageFileName);
            return this._repositoryFileSystem.OpenFile(path);
        }

        public override IEnumerable<FrameworkName> GetSupportedFrameworks()
        {
            IEnumerable<FrameworkName> second = from <>h__TransparentIdentifier0 in Enumerable.Select(Enumerable.Select<string, string>(this.GetPackageFilePaths(), new Func<string, string>(this.GetPackageRelativePath)), delegate (string file) {
                string effectivePath;
                return new { 
                    file = file,
                    targetFramework = VersionUtility.ParseFrameworkNameFromFilePath(file, out effectivePath)
                };
            })
                where <>h__TransparentIdentifier0.targetFramework != null
                select <>h__TransparentIdentifier0.targetFramework;
            return base.GetSupportedFrameworks().Concat<FrameworkName>(second).Distinct<FrameworkName>();
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly UnzippedPackage.<>c <>9 = new UnzippedPackage.<>c();
            public static Func<<>f__AnonymousType3<string, FrameworkName>, bool> <>9__6_1;
            public static Func<<>f__AnonymousType3<string, FrameworkName>, FrameworkName> <>9__6_2;
            public static Func<<>f__AnonymousType4<string, string>, bool> <>9__8_1;
            public static Func<string, bool> <>9__9_0;

            internal bool <GetAssemblyReferencesCore>b__8_1(<>f__AnonymousType4<string, string> <>h__TransparentIdentifier0) => 
                LocalPackage.IsAssemblyReference(<>h__TransparentIdentifier0.targetPath);

            internal bool <GetPackageFilePaths>b__9_0(string p) => 
                (!PackageHelper.IsManifest(p) && !PackageHelper.IsPackageFile(p));

            internal bool <GetSupportedFrameworks>b__6_1(<>f__AnonymousType3<string, FrameworkName> <>h__TransparentIdentifier0) => 
                (<>h__TransparentIdentifier0.targetFramework != null);

            internal FrameworkName <GetSupportedFrameworks>b__6_2(<>f__AnonymousType3<string, FrameworkName> <>h__TransparentIdentifier0) => 
                <>h__TransparentIdentifier0.targetFramework;
        }
    }
}

