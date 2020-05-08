namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;

    internal static class PathResolver
    {
        private static readonly string OneDotSlash = ("." + Path.DirectorySeparatorChar.ToString());
        private static readonly string TwoDotSlash = (".." + Path.DirectorySeparatorChar.ToString());

        public static void FilterPackageFiles<T>(ICollection<T> source, Func<T, string> getPath, IEnumerable<string> wildcards)
        {
            HashSet<T> set = new HashSet<T>(GetMatches<T>(source, getPath, wildcards));
            source.RemoveAll<T>(new Func<T, bool>(set.Contains));
        }

        public static IEnumerable<T> GetMatches<T>(IEnumerable<T> source, Func<T, string> getPath, IEnumerable<string> wildcards)
        {
            IEnumerable<Regex> filters = Enumerable.Select<string, Regex>(wildcards, new Func<string, Regex>(PathResolver.WildcardToRegex));
            return (from item in source
                where Enumerable.Any<Regex>(filters, f => f.IsMatch(getPath(item)))
                select item);
        }

        internal static string GetPathToEnumerateFrom(string basePath, string searchPath)
        {
            string str;
            int index = searchPath.IndexOf('*');
            if (index == -1)
            {
                string directoryName = Path.GetDirectoryName(searchPath);
                str = Path.Combine(basePath, directoryName);
            }
            else
            {
                int length = searchPath.LastIndexOf(Path.DirectorySeparatorChar, index);
                if (length == -1)
                {
                    str = basePath;
                }
                else
                {
                    string str3 = searchPath.Substring(0, length);
                    str = Path.Combine(basePath, str3);
                }
            }
            return str;
        }

        internal static bool IsDirectoryPath(string path) => 
            ((path != null) && ((path.Length > 1) && ((path[path.Length - 1] == Path.DirectorySeparatorChar) || (path[path.Length - 1] == Path.AltDirectorySeparatorChar))));

        private static bool IsEmptyDirectory(string directory) => 
            !Directory.EnumerateFileSystemEntries(directory).Any<string>();

        internal static bool IsWildcardSearch(string filter) => 
            (filter.IndexOf('*') != -1);

        internal static string NormalizeBasePath(string basePath, ref string searchPath)
        {
            basePath = string.IsNullOrEmpty(basePath) ? OneDotSlash : basePath;
            while (searchPath.StartsWith(TwoDotSlash, StringComparison.OrdinalIgnoreCase))
            {
                basePath = Path.Combine(basePath, TwoDotSlash);
                searchPath = searchPath.Substring(TwoDotSlash.Length);
            }
            return Path.GetFullPath(basePath);
        }

        public static string NormalizeWildcardForExcludedFiles(string basePath, string wildcard)
        {
            if (wildcard.StartsWith("**", StringComparison.OrdinalIgnoreCase))
            {
                return wildcard;
            }
            basePath = NormalizeBasePath(basePath, ref wildcard);
            return Path.Combine(basePath, wildcard);
        }

        public static IEnumerable<string> PerformWildcardSearch(string basePath, string searchPath)
        {
            string str;
            return (from s in PerformWildcardSearchInternal(basePath, searchPath, false, out str) select s.Path);
        }

        private static IEnumerable<SearchPathResult> PerformWildcardSearchInternal(string basePath, string searchPath, bool includeEmptyDirectories, out string normalizedBasePath)
        {
            if (!searchPath.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase) && (Path.DirectorySeparatorChar != '/'))
            {
                char[] trimChars = new char[] { Path.DirectorySeparatorChar };
                searchPath = searchPath.TrimStart(trimChars);
            }
            bool flag = false;
            if (IsDirectoryPath(searchPath))
            {
                searchPath = searchPath + "**" + Path.DirectorySeparatorChar.ToString() + "*";
                flag = true;
            }
            basePath = NormalizeBasePath(basePath, ref searchPath);
            normalizedBasePath = GetPathToEnumerateFrom(basePath, searchPath);
            Regex searchRegex = WildcardToRegex(Path.Combine(basePath, searchPath));
            SearchOption allDirectories = SearchOption.AllDirectories;
            bool flag2 = Path.GetDirectoryName(searchPath).Contains<char>('*');
            if ((searchPath.IndexOf("**", StringComparison.OrdinalIgnoreCase) == -1) && !flag2)
            {
                allDirectories = SearchOption.TopDirectoryOnly;
            }
            IEnumerable<SearchPathResult> first = from file in Directory.GetFiles(normalizedBasePath, "*.*", allDirectories)
                where searchRegex.IsMatch(file)
                select new SearchPathResult(file, true);
            if (!includeEmptyDirectories)
            {
                return first;
            }
            IEnumerable<SearchPathResult> enumerable2 = from directory in Directory.GetDirectories(normalizedBasePath, "*.*", allDirectories)
                where searchRegex.IsMatch(directory) && IsEmptyDirectory(directory)
                select new SearchPathResult(directory, false);
            if (flag && IsEmptyDirectory(normalizedBasePath))
            {
                SearchPathResult[] second = new SearchPathResult[] { new SearchPathResult(normalizedBasePath, false) };
                enumerable2 = enumerable2.Concat<SearchPathResult>(second);
            }
            return first.Concat<SearchPathResult>(enumerable2);
        }

        internal static string ResolvePackagePath(string searchDirectory, string searchPattern, string fullPath, string targetPath)
        {
            string fileName;
            bool flag = IsDirectoryPath(searchPattern);
            bool flag2 = IsWildcardSearch(searchPattern);
            if (((flag2 && (searchPattern.IndexOf("**", StringComparison.OrdinalIgnoreCase) != -1)) | flag) && fullPath.StartsWith(searchDirectory, StringComparison.OrdinalIgnoreCase))
            {
                char[] trimChars = new char[] { Path.DirectorySeparatorChar };
                fileName = fullPath.Substring(searchDirectory.Length).TrimStart(trimChars);
            }
            else
            {
                if (!flag2 && Path.GetExtension(searchPattern).Equals(Path.GetExtension(targetPath), StringComparison.OrdinalIgnoreCase))
                {
                    return targetPath;
                }
                fileName = Path.GetFileName(fullPath);
            }
            return Path.Combine(targetPath ?? string.Empty, fileName);
        }

        internal static IEnumerable<PhysicalPackageFile> ResolveSearchPattern(string basePath, string searchPath, string targetPath, bool includeEmptyDirectories)
        {
            string normalizedBasePath;
            return Enumerable.Select<SearchPathResult, PhysicalPackageFile>(PerformWildcardSearchInternal(basePath, searchPath, includeEmptyDirectories, out normalizedBasePath), delegate (SearchPathResult result) {
                if (!result.IsFile)
                {
                    EmptyFrameworkFolderFile file1 = new EmptyFrameworkFolderFile(ResolvePackagePath(normalizedBasePath, searchPath, result.Path, targetPath));
                    file1.SourcePath = result.Path;
                    return file1;
                }
                PhysicalPackageFile file2 = new PhysicalPackageFile();
                file2.SourcePath = result.Path;
                file2.TargetPath = ResolvePackagePath(normalizedBasePath, searchPath, result.Path, targetPath);
                return file2;
            });
        }

        private static Regex WildcardToRegex(string wildcard)
        {
            string str = Regex.Escape(wildcard);
            str = (Path.DirectorySeparatorChar != '/') ? str.Replace("/", @"\\").Replace(@"\*\*\\", ".*").Replace(@"\*\*", ".*").Replace(@"\*", @"[^\\]*(\\)?").Replace(@"\?", ".") : str.Replace(@"\*\*/", ".*").Replace(@"\*\*", ".*").Replace(@"\*", "[^/]*(/)?").Replace(@"\?", ".");
            return new Regex("^" + str + "$", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PathResolver.<>c <>9 = new PathResolver.<>c();
            public static Func<PathResolver.SearchPathResult, string> <>9__5_0;
            public static Func<string, PathResolver.SearchPathResult> <>9__6_1;
            public static Func<string, PathResolver.SearchPathResult> <>9__6_3;

            internal string <PerformWildcardSearch>b__5_0(PathResolver.SearchPathResult s) => 
                s.Path;

            internal PathResolver.SearchPathResult <PerformWildcardSearchInternal>b__6_1(string file) => 
                new PathResolver.SearchPathResult(file, true);

            internal PathResolver.SearchPathResult <PerformWildcardSearchInternal>b__6_3(string directory) => 
                new PathResolver.SearchPathResult(directory, false);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SearchPathResult
        {
            private readonly string _path;
            private readonly bool _isFile;
            public string Path =>
                this._path;
            public bool IsFile =>
                this._isFile;
            public SearchPathResult(string path, bool isFile)
            {
                this._path = path;
                this._isFile = isFile;
            }
        }
    }
}

