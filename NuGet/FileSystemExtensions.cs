namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal static class FileSystemExtensions
    {
        public static void AddFiles(IFileSystem fileSystem, IEnumerable<IPackageFile> files, string rootDir)
        {
            fileSystem.AddFiles(files, rootDir, true);
        }

        public static void AddFiles(this IFileSystem fileSystem, IEnumerable<IPackageFile> files, string rootDir, bool preserveFilePath)
        {
            foreach (IPackageFile file in files)
            {
                string path = Path.Combine(rootDir, preserveFilePath ? file.Path : Path.GetFileName(file.Path));
                fileSystem.AddFileWithCheck(path, new Func<Stream>(file.GetStream));
            }
        }

        internal static void AddFileWithCheck(this IFileSystem fileSystem, string path, Action<Stream> write)
        {
            if (!fileSystem.FileExists(path))
            {
                fileSystem.AddFile(path, write);
            }
            else
            {
                object[] args = new object[] { path };
                fileSystem.Logger.Log(MessageLevel.Warning, NuGetResources.Warning_FileAlreadyExists, args);
            }
        }

        internal static void AddFileWithCheck(this IFileSystem fileSystem, string path, Func<Stream> streamFactory)
        {
            if (fileSystem.FileExists(path))
            {
                object[] args = new object[] { path };
                fileSystem.Logger.Log(MessageLevel.Warning, NuGetResources.Warning_FileAlreadyExists, args);
            }
            else
            {
                using (Stream stream = streamFactory())
                {
                    fileSystem.AddFile(path, stream);
                }
            }
        }

        private static void Attempt(Action action, int retries = 3, int delayBeforeRetry = 150)
        {
            while (true)
            {
                while (true)
                {
                    if (retries > 0)
                    {
                        try
                        {
                            action();
                        }
                        catch
                        {
                            retries--;
                            if (retries != 0)
                            {
                                break;
                            }
                            throw;
                        }
                        break;
                    }
                    else
                    {
                        break;
                    }
                    break;
                }
                Thread.Sleep(delayBeforeRetry);
            }
        }

        public static bool ContentEqual(IFileSystem fileSystem, string path, Func<Stream> streamFactory)
        {
            bool flag;
            using (Stream stream = streamFactory())
            {
                using (Stream stream2 = fileSystem.OpenFile(path))
                {
                    flag = stream.ContentEquals(stream2);
                }
            }
            return flag;
        }

        internal static void DeleteDirectorySafe(this IFileSystem fileSystem, string path, bool recursive)
        {
            DoSafeAction(() => fileSystem.DeleteDirectory(path, recursive), fileSystem.Logger);
        }

        public static void DeleteFileAndParentDirectoriesIfEmpty(this IFileSystem fileSystem, string filePath)
        {
            fileSystem.DeleteFileSafe(filePath);
            for (string str = Path.GetDirectoryName(filePath); !string.IsNullOrEmpty(str) && (!fileSystem.GetFiles(str, "*.*").Any<string>() && !fileSystem.GetDirectories(str).Any<string>()); str = Path.GetDirectoryName(str))
            {
                fileSystem.DeleteDirectorySafe(str, false);
            }
        }

        internal static void DeleteFiles(IFileSystem fileSystem, IEnumerable<IPackageFile> files, string rootDir)
        {
            ILookup<string, IPackageFile> lookup = Enumerable.ToLookup<IPackageFile, string>(files, p => Path.GetDirectoryName(p.Path));
            foreach (string str in from grouping in lookup
                from directory in GetDirectories(grouping.Key)
                orderby directory.Length descending
                select directory)
            {
                IEnumerable<IPackageFile> enumerable = lookup.Contains(str) ? lookup[str] : Enumerable.Empty<IPackageFile>();
                string path = Path.Combine(rootDir, str);
                if (fileSystem.DirectoryExists(path))
                {
                    foreach (IPackageFile file in enumerable)
                    {
                        string str3 = Path.Combine(rootDir, file.Path);
                        fileSystem.DeleteFileSafe(str3, new Func<Stream>(file.GetStream));
                    }
                    if (!fileSystem.GetFilesSafe(path).Any<string>() && !fileSystem.GetDirectoriesSafe(path).Any<string>())
                    {
                        fileSystem.DeleteDirectorySafe(path, false);
                    }
                }
            }
        }

        internal static void DeleteFileSafe(this IFileSystem fileSystem, string path)
        {
            DoSafeAction(() => fileSystem.DeleteFile(path), fileSystem.Logger);
        }

        public static void DeleteFileSafe(this IFileSystem fileSystem, string path, Func<Stream> streamFactory)
        {
            if (fileSystem.FileExists(path))
            {
                if (ContentEqual(fileSystem, path, streamFactory))
                {
                    fileSystem.DeleteFileSafe(path);
                }
                else
                {
                    object[] args = new object[] { path };
                    fileSystem.Logger.Log(MessageLevel.Warning, NuGetResources.Warning_FileModified, args);
                }
            }
        }

        private static void DoSafeAction(Action action, ILogger logger)
        {
            try
            {
                Attempt(action, 3, 150);
            }
            catch (Exception exception)
            {
                logger.Log(MessageLevel.Warning, exception.Message, new object[0]);
            }
        }

        [IteratorStateMachine(typeof(<GetDirectories>d__14))]
        internal static IEnumerable<string> GetDirectories(string path)
        {
            IEnumerator<int> enumerator = IndexOfAll(path, Path.DirectorySeparatorChar).GetEnumerator();
            if (enumerator.MoveNext())
            {
                int length = enumerator.Current;
                yield return path.Substring(0, length);
                yield break;
            }
            else
            {
                enumerator = null;
                yield return path;
                yield break;
            }
        }

        internal static IEnumerable<string> GetDirectoriesSafe(this IFileSystem fileSystem, string path)
        {
            IEnumerable<string> directories;
            try
            {
                directories = fileSystem.GetDirectories(path);
            }
            catch (Exception exception)
            {
                fileSystem.Logger.Log(MessageLevel.Warning, exception.Message, new object[0]);
                return Enumerable.Empty<string>();
            }
            return directories;
        }

        public static IEnumerable<string> GetFiles(this IFileSystem fileSystem, string path, string filter) => 
            fileSystem.GetFiles(path, filter, false);

        internal static IEnumerable<string> GetFilesSafe(this IFileSystem fileSystem, string path) => 
            fileSystem.GetFilesSafe(path, "*.*");

        internal static IEnumerable<string> GetFilesSafe(this IFileSystem fileSystem, string path, string filter)
        {
            IEnumerable<string> enumerable;
            try
            {
                enumerable = fileSystem.GetFiles(path, filter);
            }
            catch (Exception exception)
            {
                fileSystem.Logger.Log(MessageLevel.Warning, exception.Message, new object[0]);
                return Enumerable.Empty<string>();
            }
            return enumerable;
        }

        [IteratorStateMachine(typeof(<IndexOfAll>d__15))]
        private static IEnumerable<int> IndexOfAll(string value, char ch)
        {
            <IndexOfAll>d__15 d__1 = new <IndexOfAll>d__15(-2);
            d__1.<>3__value = value;
            d__1.<>3__ch = ch;
            return d__1;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly FileSystemExtensions.<>c <>9 = new FileSystemExtensions.<>c();
            public static Func<IPackageFile, string> <>9__3_0;
            public static Func<IGrouping<string, IPackageFile>, IEnumerable<string>> <>9__3_1;
            public static Func<IGrouping<string, IPackageFile>, string, <>f__AnonymousType28<IGrouping<string, IPackageFile>, string>> <>9__3_2;
            public static Func<<>f__AnonymousType28<IGrouping<string, IPackageFile>, string>, int> <>9__3_3;
            public static Func<<>f__AnonymousType28<IGrouping<string, IPackageFile>, string>, string> <>9__3_4;

            internal string <DeleteFiles>b__3_0(IPackageFile p) => 
                Path.GetDirectoryName(p.Path);

            internal IEnumerable<string> <DeleteFiles>b__3_1(IGrouping<string, IPackageFile> grouping) => 
                FileSystemExtensions.GetDirectories(grouping.Key);

            internal <>f__AnonymousType28<IGrouping<string, IPackageFile>, string> <DeleteFiles>b__3_2(IGrouping<string, IPackageFile> grouping, string directory) => 
                new { 
                    grouping = grouping,
                    directory = directory
                };

            internal int <DeleteFiles>b__3_3(<>f__AnonymousType28<IGrouping<string, IPackageFile>, string> <>h__TransparentIdentifier0) => 
                <>h__TransparentIdentifier0.directory.Length;

            internal string <DeleteFiles>b__3_4(<>f__AnonymousType28<IGrouping<string, IPackageFile>, string> <>h__TransparentIdentifier0) => 
                <>h__TransparentIdentifier0.directory;
        }


        [CompilerGenerated]
        private sealed class <IndexOfAll>d__15 : IEnumerable<int>, IEnumerable, IEnumerator<int>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private int <>2__current;
            private int <>l__initialThreadId;
            private string value;
            public string <>3__value;
            private char ch;
            public char <>3__ch;
            private int <index>5__1;

            [DebuggerHidden]
            public <IndexOfAll>d__15(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Environment.CurrentManagedThreadId;
            }

            private bool MoveNext()
            {
                int num = this.<>1__state;
                if (num == 0)
                {
                    this.<>1__state = -1;
                    this.<index>5__1 = -1;
                }
                else
                {
                    if (num != 1)
                    {
                        return false;
                    }
                    this.<>1__state = -1;
                    goto TR_0006;
                }
            TR_0003:
                this.<index>5__1 = this.value.IndexOf(this.ch, this.<index>5__1 + 1);
                if (this.<index>5__1 >= 0)
                {
                    this.<>2__current = this.<index>5__1;
                    this.<>1__state = 1;
                    return true;
                }
            TR_0006:
                while (true)
                {
                    if (this.<index>5__1 >= 0)
                    {
                        break;
                    }
                    return false;
                }
                goto TR_0003;
            }

            [DebuggerHidden]
            IEnumerator<int> IEnumerable<int>.GetEnumerator()
            {
                FileSystemExtensions.<IndexOfAll>d__15 d__;
                if ((this.<>1__state != -2) || (this.<>l__initialThreadId != Environment.CurrentManagedThreadId))
                {
                    d__ = new FileSystemExtensions.<IndexOfAll>d__15(0);
                }
                else
                {
                    this.<>1__state = 0;
                    d__ = this;
                }
                d__.value = this.<>3__value;
                d__.ch = this.<>3__ch;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator() => 
                this.System.Collections.Generic.IEnumerable<System.Int32>.GetEnumerator();

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
            }

            int IEnumerator<int>.Current =>
                this.<>2__current;

            object IEnumerator.Current =>
                this.<>2__current;
        }
    }
}

