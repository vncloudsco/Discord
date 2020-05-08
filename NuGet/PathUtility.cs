namespace NuGet
{
    using System;
    using System.IO;

    internal static class PathUtility
    {
        public static void EnsureParentDirectory(string filePath)
        {
            string directoryName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
        }

        private static string EnsureTrailingCharacter(string path, char trailingCharacter)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            return (((path.Length == 0) || (path[path.Length - 1] == trailingCharacter)) ? path : (path + trailingCharacter.ToString()));
        }

        public static string EnsureTrailingForwardSlash(string path) => 
            EnsureTrailingCharacter(path, '/');

        public static string EnsureTrailingSlash(string path) => 
            EnsureTrailingCharacter(path, Path.DirectorySeparatorChar);

        public static string GetAbsolutePath(string basePath, string relativePath)
        {
            if (basePath == null)
            {
                throw new ArgumentNullException("basePath");
            }
            if (relativePath == null)
            {
                throw new ArgumentNullException("relativePath");
            }
            return new Uri(new Uri(basePath), new Uri(relativePath, UriKind.Relative)).LocalPath;
        }

        public static string GetCanonicalPath(string path) => 
            ((PathValidator.IsValidLocalPath(path) || PathValidator.IsValidUncPath(path)) ? Path.GetFullPath(EnsureTrailingSlash(path)) : (!PathValidator.IsValidUrl(path) ? path : new Uri(path).AbsoluteUri));

        public static string GetRelativePath(string path1, string path2)
        {
            if (path1 == null)
            {
                throw new ArgumentNullException("path1");
            }
            if (path2 == null)
            {
                throw new ArgumentNullException("path2");
            }
            return UriUtility.GetPath(new Uri(path1).MakeRelativeUri(new Uri(path2)));
        }

        public static bool IsSubdirectory(string basePath, string path)
        {
            if (basePath == null)
            {
                throw new ArgumentNullException("basePath");
            }
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            char[] trimChars = new char[] { Path.DirectorySeparatorChar };
            basePath = basePath.TrimEnd(trimChars);
            return path.StartsWith(basePath, StringComparison.OrdinalIgnoreCase);
        }
    }
}

