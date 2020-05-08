namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal static class Constants
    {
        public static readonly string PackageExtension = ".nupkg";
        public static readonly string ManifestExtension = ".nuspec";
        public static readonly string ContentDirectory = "content";
        public static readonly string LibDirectory = "lib";
        public static readonly string ToolsDirectory = "tools";
        public static readonly string BuildDirectory = "build";
        public static readonly string BinDirectory = "bin";
        public static readonly string SettingsFileName = "NuGet.Config";
        public static readonly string PackageReferenceFile = "packages.config";
        public static readonly string MirroringReferenceFile = "mirroring.config";
        public static readonly string BeginIgnoreMarker = "NUGET: BEGIN LICENSE TEXT";
        public static readonly string EndIgnoreMarker = "NUGET: END LICENSE TEXT";
        internal const string PackageRelationshipNamespace = "http://schemas.microsoft.com/packaging/2010/07/";
        internal const string PackageEmptyFileName = "_._";
        public static readonly DateTimeOffset Unpublished = new DateTimeOffset(0x76c, 1, 1, 0, 0, 0, TimeSpan.FromHours(-8.0));
        public static readonly ICollection<string> AssemblyReferencesExtensions;
        public static readonly Version NuGetVersion;

        static Constants()
        {
            string[] list = new string[] { ".dll", ".exe", ".winmd" };
            AssemblyReferencesExtensions = new ReadOnlyCollection<string>(list);
            NuGetVersion = typeof(IPackage).Assembly.GetName().Version;
        }
    }
}

