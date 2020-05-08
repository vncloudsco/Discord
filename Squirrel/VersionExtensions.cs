namespace Squirrel
{
    using NuGet;
    using System;
    using System.Runtime.CompilerServices;
    using System.Text.RegularExpressions;

    internal static class VersionExtensions
    {
        private static readonly Regex _suffixRegex = new Regex(@"(-full|-delta)?\.nupkg$", RegexOptions.Compiled);
        private static readonly Regex _versionRegex = new Regex(@"\d+(\.\d+){0,3}(-[a-z][0-9a-z-]*)?$", RegexOptions.Compiled);

        public static SemanticVersion ToSemanticVersion(this IReleasePackage package) => 
            package.InputPackageFile.ToSemanticVersion();

        public static SemanticVersion ToSemanticVersion(this string fileName)
        {
            string input = _suffixRegex.Replace(fileName, "");
            return new SemanticVersion(_versionRegex.Match(input).Value);
        }
    }
}

