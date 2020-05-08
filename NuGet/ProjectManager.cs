namespace NuGet
{
    using Microsoft.VisualStudio.ProjectSystem.Interop;
    using NuGet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;
    using System.Threading;
    using System.Xml.Linq;

    internal class ProjectManager : IProjectManager
    {
        [CompilerGenerated]
        private EventHandler<PackageOperationEventArgs> PackageReferenceAdding;
        [CompilerGenerated]
        private EventHandler<PackageOperationEventArgs> PackageReferenceAdded;
        [CompilerGenerated]
        private EventHandler<PackageOperationEventArgs> PackageReferenceRemoving;
        [CompilerGenerated]
        private EventHandler<PackageOperationEventArgs> PackageReferenceRemoved;
        private ILogger _logger;
        private IPackageConstraintProvider _constraintProvider;
        private readonly IPackageReferenceRepository _packageReferenceRepository;
        private readonly IDictionary<FileTransformExtensions, IPackageFileTransformer> _fileTransformers;

        public event EventHandler<PackageOperationEventArgs> PackageReferenceAdded
        {
            [CompilerGenerated] add
            {
                EventHandler<PackageOperationEventArgs> packageReferenceAdded = this.PackageReferenceAdded;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> a = packageReferenceAdded;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Combine(a, value);
                    packageReferenceAdded = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageReferenceAdded, handler3, a);
                    if (ReferenceEquals(packageReferenceAdded, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<PackageOperationEventArgs> packageReferenceAdded = this.PackageReferenceAdded;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> source = packageReferenceAdded;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Remove(source, value);
                    packageReferenceAdded = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageReferenceAdded, handler3, source);
                    if (ReferenceEquals(packageReferenceAdded, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EventHandler<PackageOperationEventArgs> PackageReferenceAdding
        {
            [CompilerGenerated] add
            {
                EventHandler<PackageOperationEventArgs> packageReferenceAdding = this.PackageReferenceAdding;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> a = packageReferenceAdding;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Combine(a, value);
                    packageReferenceAdding = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageReferenceAdding, handler3, a);
                    if (ReferenceEquals(packageReferenceAdding, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<PackageOperationEventArgs> packageReferenceAdding = this.PackageReferenceAdding;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> source = packageReferenceAdding;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Remove(source, value);
                    packageReferenceAdding = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageReferenceAdding, handler3, source);
                    if (ReferenceEquals(packageReferenceAdding, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EventHandler<PackageOperationEventArgs> PackageReferenceRemoved
        {
            [CompilerGenerated] add
            {
                EventHandler<PackageOperationEventArgs> packageReferenceRemoved = this.PackageReferenceRemoved;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> a = packageReferenceRemoved;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Combine(a, value);
                    packageReferenceRemoved = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageReferenceRemoved, handler3, a);
                    if (ReferenceEquals(packageReferenceRemoved, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<PackageOperationEventArgs> packageReferenceRemoved = this.PackageReferenceRemoved;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> source = packageReferenceRemoved;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Remove(source, value);
                    packageReferenceRemoved = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageReferenceRemoved, handler3, source);
                    if (ReferenceEquals(packageReferenceRemoved, source))
                    {
                        return;
                    }
                }
            }
        }

        public event EventHandler<PackageOperationEventArgs> PackageReferenceRemoving
        {
            [CompilerGenerated] add
            {
                EventHandler<PackageOperationEventArgs> packageReferenceRemoving = this.PackageReferenceRemoving;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> a = packageReferenceRemoving;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Combine(a, value);
                    packageReferenceRemoving = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageReferenceRemoving, handler3, a);
                    if (ReferenceEquals(packageReferenceRemoving, a))
                    {
                        return;
                    }
                }
            }
            [CompilerGenerated] remove
            {
                EventHandler<PackageOperationEventArgs> packageReferenceRemoving = this.PackageReferenceRemoving;
                while (true)
                {
                    EventHandler<PackageOperationEventArgs> source = packageReferenceRemoving;
                    EventHandler<PackageOperationEventArgs> handler3 = (EventHandler<PackageOperationEventArgs>) Delegate.Remove(source, value);
                    packageReferenceRemoving = Interlocked.CompareExchange<EventHandler<PackageOperationEventArgs>>(ref this.PackageReferenceRemoving, handler3, source);
                    if (ReferenceEquals(packageReferenceRemoving, source))
                    {
                        return;
                    }
                }
            }
        }

        public ProjectManager(IPackageManager packageManager, IPackagePathResolver pathResolver, IProjectSystem project, IPackageRepository localRepository)
        {
            Dictionary<FileTransformExtensions, IPackageFileTransformer> dictionary1 = new Dictionary<FileTransformExtensions, IPackageFileTransformer>();
            dictionary1.Add(new FileTransformExtensions(".transform", ".transform"), new XmlTransformer(GetConfigMappings()));
            dictionary1.Add(new FileTransformExtensions(".pp", ".pp"), new Preprocessor());
            dictionary1.Add(new FileTransformExtensions(".install.xdt", ".uninstall.xdt"), new XdtTransformer());
            this._fileTransformers = dictionary1;
            if (pathResolver == null)
            {
                throw new ArgumentNullException("pathResolver");
            }
            if (project == null)
            {
                throw new ArgumentNullException("project");
            }
            if (localRepository == null)
            {
                throw new ArgumentNullException("localRepository");
            }
            this.PackageManager = packageManager;
            this.Project = project;
            this.PathResolver = pathResolver;
            this.LocalRepository = localRepository;
            this._packageReferenceRepository = this.LocalRepository as IPackageReferenceRepository;
        }

        private void AddPackageReferenceToNuGetAwareProject(IPackage package)
        {
            INuGetPackageManager project = this.Project as INuGetPackageManager;
            Dictionary<string, object> options = new Dictionary<string, object>();
            using (CancellationTokenSource source = new CancellationTokenSource())
            {
                IEnumerable<FrameworkName> packageSupportedFrameworks = package.GetSupportedFrameworks();
                IReadOnlyCollection<FrameworkName> result = project.GetSupportedFrameworksAsync(source.Token).Result;
                options["Frameworks"] = (from projectFramework in result
                    where VersionUtility.IsCompatible(projectFramework, packageSupportedFrameworks)
                    select projectFramework).ToArray<FrameworkName>();
                NuGetPackageMoniker moniker1 = new NuGetPackageMoniker();
                moniker1.Id = package.Id;
                moniker1.Version = package.Version.ToString();
                project.InstallPackageAsync(moniker1, options, null, null, source.Token).Wait();
            }
        }

        protected void AddPackageReferenceToProject(IPackage package)
        {
            string fullName = package.GetFullName();
            object[] objArray1 = new object[] { fullName, this.Project.ProjectName };
            this.Logger.Log(MessageLevel.Info, NuGetResources.Log_BeginAddPackageReference, objArray1);
            if (this.IsNuGetAwareProject())
            {
                this.AddPackageReferenceToNuGetAwareProject(package);
                object[] args = new object[] { fullName, this.Project.ProjectName };
                this.Logger.Log(MessageLevel.Info, NuGetResources.Log_SuccessfullyAddedPackageReference, args);
            }
            else
            {
                PackageOperationEventArgs e = this.CreateOperation(package);
                this.OnPackageReferenceAdding(e);
                if (!e.Cancel)
                {
                    this.ExtractPackageFilesToProject(package);
                    object[] args = new object[] { fullName, this.Project.ProjectName };
                    this.Logger.Log(MessageLevel.Info, NuGetResources.Log_SuccessfullyAddedPackageReference, args);
                    this.OnPackageReferenceAdded(e);
                }
            }
        }

        public PackageOperationEventArgs CreateOperation(IPackage package) => 
            new PackageOperationEventArgs(package, this.Project, this.PathResolver.GetInstallPath(package));

        public virtual void Execute(PackageOperation operation)
        {
            bool flag = this.LocalRepository.Exists(operation.Package);
            if (operation.Action != PackageAction.Install)
            {
                if (flag)
                {
                    this.RemovePackageReferenceFromProject(operation.Package);
                }
            }
            else if (!flag)
            {
                this.AddPackageReferenceToProject(operation.Package);
            }
            else
            {
                object[] args = new object[] { this.Project.ProjectName, operation.Package.GetFullName() };
                this.Logger.Log(MessageLevel.Info, NuGetResources.Log_ProjectAlreadyReferencesPackage, args);
            }
        }

        protected virtual void ExtractPackageFilesToProject(IPackage package)
        {
            List<IPackageAssemblyReference> assemblyReferences = this.Project.GetCompatibleItemsCore<IPackageAssemblyReference>(package.AssemblyReferences).ToList<IPackageAssemblyReference>();
            List<FrameworkAssemblyReference> list2 = this.Project.GetCompatibleItemsCore<FrameworkAssemblyReference>(package.FrameworkAssemblies).ToList<FrameworkAssemblyReference>();
            List<IPackageFile> contentFiles = this.Project.GetCompatibleItemsCore<IPackageFile>(package.GetContentFiles()).ToList<IPackageFile>();
            List<IPackageFile> buildFiles = this.Project.GetCompatibleItemsCore<IPackageFile>(package.GetBuildFiles()).ToList<IPackageFile>();
            if ((assemblyReferences.Count == 0) && ((list2.Count == 0) && ((contentFiles.Count == 0) && ((buildFiles.Count == 0) && (package.FrameworkAssemblies.Any<FrameworkAssemblyReference>() || (package.AssemblyReferences.Any<IPackageAssemblyReference>() || (package.GetContentFiles().Any<IPackageFile>() || package.GetBuildFiles().Any<IPackageFile>())))))))
            {
                FrameworkName targetFramework = this.Project.TargetFramework;
                string str = targetFramework.IsPortableFramework() ? VersionUtility.GetShortFrameworkName(targetFramework) : targetFramework?.ToString();
                object[] args = new object[] { package.GetFullName(), str };
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, NuGetResources.UnableToFindCompatibleItems, args));
            }
            this.FilterAssemblyReferences(assemblyReferences, package.PackageAssemblyReferences);
            try
            {
                this.LogTargetFrameworkInfo(package, assemblyReferences, contentFiles, buildFiles);
                this.Project.AddFiles(contentFiles, this._fileTransformers);
                foreach (IPackageAssemblyReference reference in assemblyReferences)
                {
                    if (!reference.IsEmptyFolder())
                    {
                        string str2 = Path.Combine(this.PathResolver.GetInstallPath(package), reference.Path);
                        string relativePath = PathUtility.GetRelativePath(this.Project.Root, str2);
                        if (this.Project.ReferenceExists(reference.Name))
                        {
                            this.Project.RemoveReference(reference.Name);
                        }
                        this.Project.AddReference(relativePath);
                    }
                }
                foreach (FrameworkAssemblyReference reference2 in list2)
                {
                    if (!this.Project.ReferenceExists(reference2.AssemblyName))
                    {
                        this.Project.AddFrameworkReference(reference2.AssemblyName);
                    }
                }
                foreach (IPackageFile file in buildFiles)
                {
                    string targetFullPath = Path.Combine(this.PathResolver.GetInstallPath(package), file.Path);
                    this.Project.AddImport(targetFullPath, file.Path.EndsWith(".props", StringComparison.OrdinalIgnoreCase) ? ProjectImportLocation.Top : ProjectImportLocation.Bottom);
                }
            }
            finally
            {
                if (this._packageReferenceRepository != null)
                {
                    this._packageReferenceRepository.AddPackage(package.Id, package.Version, package.DevelopmentDependency, this.Project.TargetFramework);
                }
                else
                {
                    this.LocalRepository.AddPackage(package);
                }
            }
        }

        private void FilterAssemblyReferences(List<IPackageAssemblyReference> assemblyReferences, ICollection<PackageReferenceSet> packageAssemblyReferences)
        {
            if ((packageAssemblyReferences != null) && (packageAssemblyReferences.Count > 0))
            {
                PackageReferenceSet packageReferences = this.Project.GetCompatibleItemsCore<PackageReferenceSet>(packageAssemblyReferences).FirstOrDefault<PackageReferenceSet>();
                if (packageReferences != null)
                {
                    assemblyReferences.RemoveAll(assembly => !packageReferences.References.Contains<string>(assembly.Name, StringComparer.OrdinalIgnoreCase));
                }
            }
        }

        private IEnumerable<T> GetCompatibleInstalledItemsForPackage<T>(string packageId, IEnumerable<T> items, NetPortableProfileTable portableProfileTable) where T: IFrameworkTargetable
        {
            IEnumerable<T> enumerable;
            FrameworkName targetFrameworkForPackage = this.GetTargetFrameworkForPackage(packageId);
            return ((targetFrameworkForPackage != null) ? (!VersionUtility.TryGetCompatibleItems<T>(targetFrameworkForPackage, items, portableProfileTable, out enumerable) ? Enumerable.Empty<T>() : enumerable) : items);
        }

        private static IDictionary<XName, Action<XElement, XElement>> GetConfigMappings()
        {
            Dictionary<XName, Action<XElement, XElement>> dictionary1 = new Dictionary<XName, Action<XElement, XElement>>();
            Dictionary<XName, Action<XElement, XElement>> dictionary2 = new Dictionary<XName, Action<XElement, XElement>>();
            dictionary2.Add("configSections", delegate (XElement parent, XElement element) {
                parent.AddFirst(element);
            });
            return dictionary2;
        }

        private IList<IPackageAssemblyReference> GetFilteredAssembliesToDelete(IPackage package)
        {
            List<IPackageAssemblyReference> list = this.GetCompatibleInstalledItemsForPackage<IPackageAssemblyReference>(package.Id, package.AssemblyReferences, NetPortableProfileTable.Default).ToList<IPackageAssemblyReference>();
            if (list.Count != 0)
            {
                PackageReferenceSet packageReferences = this.GetCompatibleInstalledItemsForPackage<PackageReferenceSet>(package.Id, package.PackageAssemblyReferences, NetPortableProfileTable.Default).FirstOrDefault<PackageReferenceSet>();
                if (packageReferences != null)
                {
                    list.RemoveAll(p => !packageReferences.References.Contains<string>(p.Name, StringComparer.OrdinalIgnoreCase));
                }
            }
            return list;
        }

        private bool IsNuGetAwareProject() => 
            (this.Project is INuGetPackageManager);

        private bool IsTransformFile(string path) => 
            Enumerable.Any<FileTransformExtensions>(this._fileTransformers.Keys, file => path.EndsWith(file.InstallExtension, StringComparison.OrdinalIgnoreCase) || path.EndsWith(file.UninstallExtension, StringComparison.OrdinalIgnoreCase));

        private void LogTargetFrameworkInfo(IPackage package, List<IPackageAssemblyReference> assemblyReferences, List<IPackageFile> contentFiles, List<IPackageFile> buildFiles)
        {
            if ((assemblyReferences.Count > 0) || ((contentFiles.Count > 0) || (buildFiles.Count > 0)))
            {
                string str = (this.Project.TargetFramework == null) ? string.Empty : VersionUtility.GetShortFrameworkName(this.Project.TargetFramework);
                object[] args = new object[] { package.GetFullName(), this.Project.ProjectName, str };
                this.Logger.Log(MessageLevel.Debug, NuGetResources.Debug_TargetFrameworkInfoPrefix, args);
                if (assemblyReferences.Count > 0)
                {
                    object[] objArray2 = new object[] { NuGetResources.Debug_TargetFrameworkInfo_AssemblyReferences, Path.GetDirectoryName(assemblyReferences[0].Path), VersionUtility.GetTargetFrameworkLogString(assemblyReferences[0].TargetFramework) };
                    this.Logger.Log(MessageLevel.Debug, NuGetResources.Debug_TargetFrameworkInfo, objArray2);
                }
                if (contentFiles.Count > 0)
                {
                    object[] objArray3 = new object[] { NuGetResources.Debug_TargetFrameworkInfo_ContentFiles, Path.GetDirectoryName(contentFiles[0].Path), VersionUtility.GetTargetFrameworkLogString(contentFiles[0].TargetFramework) };
                    this.Logger.Log(MessageLevel.Debug, NuGetResources.Debug_TargetFrameworkInfo, objArray3);
                }
                if (buildFiles.Count > 0)
                {
                    object[] objArray4 = new object[] { NuGetResources.Debug_TargetFrameworkInfo_BuildFiles, Path.GetDirectoryName(buildFiles[0].Path), VersionUtility.GetTargetFrameworkLogString(buildFiles[0].TargetFramework) };
                    this.Logger.Log(MessageLevel.Debug, NuGetResources.Debug_TargetFrameworkInfo, objArray4);
                }
            }
        }

        private void OnPackageReferenceAdded(PackageOperationEventArgs e)
        {
            if (this.PackageReferenceAdded != null)
            {
                this.PackageReferenceAdded(this, e);
            }
        }

        private void OnPackageReferenceAdding(PackageOperationEventArgs e)
        {
            if (this.PackageReferenceAdding != null)
            {
                this.PackageReferenceAdding(this, e);
            }
        }

        private void OnPackageReferenceRemoved(PackageOperationEventArgs e)
        {
            if (this.PackageReferenceRemoved != null)
            {
                this.PackageReferenceRemoved(this, e);
            }
        }

        private void OnPackageReferenceRemoving(PackageOperationEventArgs e)
        {
            if (this.PackageReferenceRemoving != null)
            {
                this.PackageReferenceRemoving(this, e);
            }
        }

        private void RemovePackageReferenceFromNuGetAwareProject(IPackage package)
        {
            INuGetPackageManager project = this.Project as INuGetPackageManager;
            string fullName = package.GetFullName();
            object[] args = new object[] { fullName, this.Project.ProjectName };
            this.Logger.Log(MessageLevel.Info, NuGetResources.Log_BeginRemovePackageReference, args);
            Dictionary<string, object> options = new Dictionary<string, object>();
            using (CancellationTokenSource source = new CancellationTokenSource())
            {
                NuGetPackageMoniker moniker1 = new NuGetPackageMoniker();
                moniker1.Id = package.Id;
                moniker1.Version = package.Version.ToString();
                project.UninstallPackageAsync(moniker1, options, null, null, source.Token).Wait();
            }
            object[] objArray2 = new object[] { fullName, this.Project.ProjectName };
            this.Logger.Log(MessageLevel.Info, NuGetResources.Log_SuccessfullyRemovedPackageReference, objArray2);
        }

        private void RemovePackageReferenceFromProject(IPackage package)
        {
            if (this.IsNuGetAwareProject())
            {
                this.RemovePackageReferenceFromNuGetAwareProject(package);
            }
            else
            {
                string fullName = package.GetFullName();
                object[] objArray1 = new object[] { fullName, this.Project.ProjectName };
                this.Logger.Log(MessageLevel.Info, NuGetResources.Log_BeginRemovePackageReference, objArray1);
                PackageOperationEventArgs e = this.CreateOperation(package);
                this.OnPackageReferenceRemoving(e);
                if (!e.Cancel)
                {
                    <>c__DisplayClass47_0 class_;
                    ParameterExpression expression = Expression.Parameter(typeof(IPackage), "p");
                    ParameterExpression[] parameters = new ParameterExpression[] { expression };
                    IEnumerable<IPackage> otherPackages = Queryable.Where<IPackage>(this.LocalRepository.GetPackages(), Expression.Lambda<Func<IPackage, bool>>(Expression.NotEqual(Expression.Property(expression, (MethodInfo) methodof(IPackageName.get_Id)), Expression.Property(Expression.Field(Expression.Constant(class_, typeof(<>c__DisplayClass47_0)), fieldof(<>c__DisplayClass47_0.package)), (MethodInfo) methodof(IPackageName.get_Id))), parameters));
                    IEnumerable<IPackageAssemblyReference> second = from p in otherPackages
                        let assemblyReferences = this.GetFilteredAssembliesToDelete(p)
                        from assemblyReference in assemblyReferences ?? Enumerable.Empty<IPackageAssemblyReference>()
                        select assemblyReference;
                    IEnumerable<IPackageFile> enumerable3 = from p in otherPackages
                        from file in this.GetCompatibleInstalledItemsForPackage<IPackageFile>(p.Id, p.GetContentFiles(), NetPortableProfileTable.Default)
                        where !this.IsTransformFile(file.Path)
                        select file;
                    IEnumerable<IPackageFile> files = this.GetCompatibleInstalledItemsForPackage<IPackageFile>(package.Id, package.GetContentFiles(), NetPortableProfileTable.Default).Except<IPackageFile>(enumerable3, PackageFileComparer.Default);
                    IEnumerable<IPackageFile> enumerable5 = this.GetCompatibleInstalledItemsForPackage<IPackageFile>(package.Id, package.GetBuildFiles(), NetPortableProfileTable.Default);
                    this.Project.DeleteFiles(files, otherPackages, this._fileTransformers);
                    foreach (IPackageAssemblyReference reference in this.GetFilteredAssembliesToDelete(package).Except<IPackageFile>(second, PackageFileComparer.Default))
                    {
                        this.Project.RemoveReference(reference.Name);
                    }
                    foreach (IPackageFile file in enumerable5)
                    {
                        string targetFullPath = Path.Combine(this.PathResolver.GetInstallPath(package), file.Path);
                        this.Project.RemoveImport(targetFullPath);
                    }
                    this.LocalRepository.RemovePackage(package);
                    object[] args = new object[] { fullName, this.Project.ProjectName };
                    this.Logger.Log(MessageLevel.Info, NuGetResources.Log_SuccessfullyRemovedPackageReference, args);
                    this.OnPackageReferenceRemoved(e);
                }
            }
        }

        public IPackageManager PackageManager { get; private set; }

        public IPackagePathResolver PathResolver { get; private set; }

        public IPackageRepository LocalRepository { get; private set; }

        public IPackageConstraintProvider ConstraintProvider
        {
            get => 
                (this._constraintProvider ?? NullConstraintProvider.Instance);
            set => 
                (this._constraintProvider = value);
        }

        public IProjectSystem Project { get; private set; }

        public ILogger Logger
        {
            get => 
                (this._logger ?? NullLogger.Instance);
            set => 
                (this._logger = value);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ProjectManager.<>c <>9 = new ProjectManager.<>c();
            public static Func<<>f__AnonymousType18<IPackage, IList<IPackageAssemblyReference>>, IEnumerable<IPackageAssemblyReference>> <>9__47_1;
            public static Func<<>f__AnonymousType18<IPackage, IList<IPackageAssemblyReference>>, IPackageAssemblyReference, IPackageAssemblyReference> <>9__47_2;
            public static Func<IPackage, IPackageFile, <>f__AnonymousType19<IPackage, IPackageFile>> <>9__47_4;
            public static Func<<>f__AnonymousType19<IPackage, IPackageFile>, IPackageFile> <>9__47_6;
            public static Action<XElement, XElement> <>9__56_0;

            internal void <GetConfigMappings>b__56_0(XElement parent, XElement element)
            {
                parent.AddFirst(element);
            }

            internal IEnumerable<IPackageAssemblyReference> <RemovePackageReferenceFromProject>b__47_1(<>f__AnonymousType18<IPackage, IList<IPackageAssemblyReference>> <>h__TransparentIdentifier0) => 
                (<>h__TransparentIdentifier0.assemblyReferences ?? Enumerable.Empty<IPackageAssemblyReference>());

            internal IPackageAssemblyReference <RemovePackageReferenceFromProject>b__47_2(<>f__AnonymousType18<IPackage, IList<IPackageAssemblyReference>> <>h__TransparentIdentifier0, IPackageAssemblyReference assemblyReference) => 
                assemblyReference;

            internal <>f__AnonymousType19<IPackage, IPackageFile> <RemovePackageReferenceFromProject>b__47_4(IPackage p, IPackageFile file) => 
                new { 
                    p = p,
                    file = file
                };

            internal IPackageFile <RemovePackageReferenceFromProject>b__47_6(<>f__AnonymousType19<IPackage, IPackageFile> <>h__TransparentIdentifier0) => 
                <>h__TransparentIdentifier0.file;
        }

        private class PackageFileComparer : IEqualityComparer<IPackageFile>
        {
            internal static readonly ProjectManager.PackageFileComparer Default = new ProjectManager.PackageFileComparer();

            private PackageFileComparer()
            {
            }

            public bool Equals(IPackageFile x, IPackageFile y) => 
                ((x.TargetFramework == y.TargetFramework) && x.EffectivePath.Equals(y.EffectivePath, StringComparison.OrdinalIgnoreCase));

            public int GetHashCode(IPackageFile obj) => 
                obj.Path.GetHashCode();
        }
    }
}

