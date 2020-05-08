namespace NuGet
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    internal static class PathValidator
    {
        private static readonly char[] _invalidPathChars = Path.GetInvalidPathChars();

        public static bool IsValidLocalPath(string path)
        {
            try
            {
                return (((Environment.OSVersion.Platform == PlatformID.MacOSX) || ((Environment.OSVersion.Platform == PlatformID.Unix) || Regex.IsMatch(path.Trim(), @"^[A-Za-z]:\\"))) ? (Path.IsPathRooted(path) && (path.IndexOfAny(_invalidPathChars) == -1)) : false);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidSource(string source) => 
            (IsValidLocalPath(source) || (IsValidUncPath(source) || IsValidUrl(source)));

        public static bool IsValidUncPath(string path)
        {
            try
            {
                Path.GetFullPath(path);
                return Regex.IsMatch(path.Trim(), @"^\\\\");
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidUrl(string url)
        {
            Uri uri;
            return (Regex.IsMatch(url, @"^\w+://", RegexOptions.IgnoreCase) && Uri.TryCreate(url, UriKind.Absolute, out uri));
        }
    }
}

