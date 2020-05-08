namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static class ProjectSystemExtensions
    {
        public static void AddFiles(this IProjectSystem project, IEnumerable<IPackageFile> files, IDictionary<FileTransformExtensions, IPackageFileTransformer> fileTransformers)
        {
            List<IPackageFile> list = files.ToList<IPackageFile>();
            IComparer<IPackageFile> comparer = project as IComparer<IPackageFile>;
            if (comparer != null)
            {
                list.Sort(comparer);
            }
            IBatchProcessor<string> processor = project as IBatchProcessor<string>;
            try
            {
                if (processor != null)
                {
                    processor.BeginProcessing(from file in list select ResolvePath(fileTransformers, fte => fte.InstallExtension, file.EffectivePath), PackageAction.Install);
                }
                foreach (IPackageFile file in list)
                {
                    IPackageFileTransformer transformer;
                    if (file.IsEmptyFolder())
                    {
                        continue;
                    }
                    string path = ResolveTargetPath(project, fileTransformers, fte => fte.InstallExtension, file.EffectivePath, out transformer);
                    if (project.IsSupportedFile(path))
                    {
                        string str2;
                        if (transformer != null)
                        {
                            transformer.TransformFile(file, path, project);
                            continue;
                        }
                        if (FindFileTransformer(fileTransformers, fte => fte.UninstallExtension, file.EffectivePath, out str2) == null)
                        {
                            TryAddFile(project, path, new Func<Stream>(file.GetStream));
                        }
                    }
                }
            }
            finally
            {
                if (processor != null)
                {
                    processor.EndProcessing();
                }
            }
        }

        public static void DeleteFiles(this IProjectSystem project, IEnumerable<IPackageFile> files, IEnumerable<IPackage> otherPackages, IDictionary<FileTransformExtensions, IPackageFileTransformer> fileTransformers)
        {
            IPackageFileTransformer transformer;
            ILookup<string, IPackageFile> lookup = Enumerable.ToLookup<IPackageFile, string>(files, p => Path.GetDirectoryName(ResolveTargetPath(project, fileTransformers, fte => fte.UninstallExtension, p.EffectivePath, out transformer)));
            foreach (string str in from grouping in lookup
                from directory in FileSystemExtensions.GetDirectories(grouping.Key)
                orderby directory.Length descending
                select directory)
            {
                IEnumerable<IPackageFile> enumerable = lookup.Contains(str) ? lookup[str] : Enumerable.Empty<IPackageFile>();
                if (project.DirectoryExists(str))
                {
                    IBatchProcessor<string> processor = project as IBatchProcessor<string>;
                    try
                    {
                        if (processor != null)
                        {
                            Func<IPackageFile, string> <>9__6;
                            Func<IPackageFile, string> func5 = <>9__6;
                            if (<>9__6 == null)
                            {
                                Func<IPackageFile, string> local5 = <>9__6;
                                func5 = <>9__6 = file => ResolvePath(fileTransformers, fte => fte.UninstallExtension, file.EffectivePath);
                            }
                            processor.BeginProcessing(Enumerable.Select<IPackageFile, string>(enumerable, func5), PackageAction.Uninstall);
                        }
                        foreach (IPackageFile file in enumerable)
                        {
                            if (!file.IsEmptyFolder())
                            {
                                string path = ResolveTargetPath(project, fileTransformers, fte => fte.UninstallExtension, file.EffectivePath, out transformer);
                                if (project.IsSupportedFile(path))
                                {
                                    Func<IPackage, IEnumerable<IPackageFile>> <>9__9;
                                    if (transformer == null)
                                    {
                                        project.DeleteFileSafe(path, new Func<Stream>(file.GetStream));
                                        continue;
                                    }
                                    Func<IPackage, IEnumerable<IPackageFile>> func4 = <>9__9;
                                    if (<>9__9 == null)
                                    {
                                        Func<IPackage, IEnumerable<IPackageFile>> local7 = <>9__9;
                                        func4 = <>9__9 = p => project.GetCompatibleItemsCore<IPackageFile>(p.GetContentFiles());
                                    }
                                    IEnumerable<IPackageFile> matchingFiles = from <>h__TransparentIdentifier0 in Enumerable.SelectMany(otherPackages, func4, (p, otherFile) => new { 
                                        p = p,
                                        otherFile = otherFile
                                    })
                                        where <>h__TransparentIdentifier0.otherFile.EffectivePath.Equals(file.EffectivePath, StringComparison.OrdinalIgnoreCase)
                                        select <>h__TransparentIdentifier0.otherFile;
                                    try
                                    {
                                        transformer.RevertFile(file, path, matchingFiles, project);
                                    }
                                    catch (Exception exception)
                                    {
                                        project.Logger.Log(MessageLevel.Warning, exception.Message, new object[0]);
                                    }
                                }
                            }
                        }
                        if (!project.GetFilesSafe(str).Any<string>() && !project.GetDirectoriesSafe(str).Any<string>())
                        {
                            project.DeleteDirectorySafe(str, false);
                        }
                    }
                    finally
                    {
                        if (processor != null)
                        {
                            processor.EndProcessing();
                        }
                    }
                }
            }
        }

        private static IPackageFileTransformer FindFileTransformer(IDictionary<FileTransformExtensions, IPackageFileTransformer> fileTransformers, Func<FileTransformExtensions, string> extensionSelector, string effectivePath, out string truncatedPath)
        {
            IPackageFileTransformer transformer;
            using (IEnumerator<FileTransformExtensions> enumerator = fileTransformers.Keys.GetEnumerator())
            {
                while (true)
                {
                    if (enumerator.MoveNext())
                    {
                        FileTransformExtensions current = enumerator.Current;
                        string str = extensionSelector(current);
                        if (!effectivePath.EndsWith(str, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        truncatedPath = effectivePath.Substring(0, effectivePath.Length - str.Length);
                        string fileName = Path.GetFileName(truncatedPath);
                        if (Constants.PackageReferenceFile.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        transformer = fileTransformers[current];
                    }
                    else
                    {
                        truncatedPath = effectivePath;
                        return null;
                    }
                    break;
                }
            }
            return transformer;
        }

        internal static IEnumerable<T> GetCompatibleItemsCore<T>(this IProjectSystem projectSystem, IEnumerable<T> items) where T: IFrameworkTargetable
        {
            IEnumerable<T> enumerable;
            return (!VersionUtility.TryGetCompatibleItems<T>(projectSystem.TargetFramework, items, out enumerable) ? Enumerable.Empty<T>() : enumerable);
        }

        private static string ResolvePath(IDictionary<FileTransformExtensions, IPackageFileTransformer> fileTransformers, Func<FileTransformExtensions, string> extensionSelector, string effectivePath)
        {
            string str;
            if (FindFileTransformer(fileTransformers, extensionSelector, effectivePath, out str) != null)
            {
                effectivePath = str;
            }
            return effectivePath;
        }

        private static string ResolveTargetPath(IProjectSystem projectSystem, IDictionary<FileTransformExtensions, IPackageFileTransformer> fileTransformers, Func<FileTransformExtensions, string> extensionSelector, string effectivePath, out IPackageFileTransformer transformer)
        {
            string str;
            transformer = FindFileTransformer(fileTransformers, extensionSelector, effectivePath, out str);
            if (transformer != null)
            {
                effectivePath = str;
            }
            return projectSystem.ResolvePath(effectivePath);
        }

        internal static void TryAddFile(IProjectSystem project, string path, Func<Stream> content)
        {
            if (!project.FileExists(path) || !project.FileExistsInProject(path))
            {
                using (Stream stream2 = content())
                {
                    project.AddFile(path, stream2);
                }
            }
            else
            {
                object[] args = new object[] { path, project.ProjectName };
                string message = string.Format(CultureInfo.CurrentCulture, NuGetResources.FileConflictMessage, args);
                FileConflictResolution resolution = project.Logger.ResolveFileConflict(message);
                if ((resolution != FileConflictResolution.Overwrite) && (resolution != FileConflictResolution.OverwriteAll))
                {
                    object[] objArray3 = new object[] { path };
                    project.Logger.Log(MessageLevel.Info, NuGetResources.Warning_FileAlreadyExists, objArray3);
                }
                else
                {
                    object[] objArray2 = new object[] { path };
                    project.Logger.Log(MessageLevel.Info, NuGetResources.Info_OverwriteExistingFile, objArray2);
                    using (Stream stream = content())
                    {
                        project.AddFile(path, stream);
                    }
                }
            }
        }

        public static bool TryGetCompatibleItems<T>(this IProjectSystem projectSystem, IEnumerable<T> items, out IEnumerable<T> compatibleItems) where T: IFrameworkTargetable
        {
            if (projectSystem == null)
            {
                throw new ArgumentNullException("projectSystem");
            }
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            return VersionUtility.TryGetCompatibleItems<T>(projectSystem.TargetFramework, items, out compatibleItems);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ProjectSystemExtensions.<>c <>9 = new ProjectSystemExtensions.<>c();
            public static Func<FileTransformExtensions, string> <>9__0_1;
            public static Func<FileTransformExtensions, string> <>9__0_2;
            public static Func<FileTransformExtensions, string> <>9__0_3;
            public static Func<FileTransformExtensions, string> <>9__2_1;
            public static Func<IGrouping<string, IPackageFile>, IEnumerable<string>> <>9__2_2;
            public static Func<IGrouping<string, IPackageFile>, string, <>f__AnonymousType28<IGrouping<string, IPackageFile>, string>> <>9__2_3;
            public static Func<<>f__AnonymousType28<IGrouping<string, IPackageFile>, string>, int> <>9__2_4;
            public static Func<<>f__AnonymousType28<IGrouping<string, IPackageFile>, string>, string> <>9__2_5;
            public static Func<FileTransformExtensions, string> <>9__2_7;
            public static Func<FileTransformExtensions, string> <>9__2_8;
            public static Func<IPackage, IPackageFile, <>f__AnonymousType29<IPackage, IPackageFile>> <>9__2_10;
            public static Func<<>f__AnonymousType29<IPackage, IPackageFile>, IPackageFile> <>9__2_12;

            internal string <AddFiles>b__0_1(FileTransformExtensions fte) => 
                fte.InstallExtension;

            internal string <AddFiles>b__0_2(FileTransformExtensions fte) => 
                fte.InstallExtension;

            internal string <AddFiles>b__0_3(FileTransformExtensions fte) => 
                fte.UninstallExtension;

            internal string <DeleteFiles>b__2_1(FileTransformExtensions fte) => 
                fte.UninstallExtension;

            internal <>f__AnonymousType29<IPackage, IPackageFile> <DeleteFiles>b__2_10(IPackage p, IPackageFile otherFile) => 
                new { 
                    p = p,
                    otherFile = otherFile
                };

            internal IPackageFile <DeleteFiles>b__2_12(<>f__AnonymousType29<IPackage, IPackageFile> <>h__TransparentIdentifier0) => 
                <>h__TransparentIdentifier0.otherFile;

            internal IEnumerable<string> <DeleteFiles>b__2_2(IGrouping<string, IPackageFile> grouping) => 
                FileSystemExtensions.GetDirectories(grouping.Key);

            internal <>f__AnonymousType28<IGrouping<string, IPackageFile>, string> <DeleteFiles>b__2_3(IGrouping<string, IPackageFile> grouping, string directory) => 
                new { 
                    grouping = grouping,
                    directory = directory
                };

            internal int <DeleteFiles>b__2_4(<>f__AnonymousType28<IGrouping<string, IPackageFile>, string> <>h__TransparentIdentifier0) => 
                <>h__TransparentIdentifier0.directory.Length;

            internal string <DeleteFiles>b__2_5(<>f__AnonymousType28<IGrouping<string, IPackageFile>, string> <>h__TransparentIdentifier0) => 
                <>h__TransparentIdentifier0.directory;

            internal string <DeleteFiles>b__2_7(FileTransformExtensions fte) => 
                fte.UninstallExtension;

            internal string <DeleteFiles>b__2_8(FileTransformExtensions fte) => 
                fte.UninstallExtension;
        }
    }
}

