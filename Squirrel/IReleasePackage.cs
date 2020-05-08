namespace Squirrel
{
    using System;
    using System.Runtime.InteropServices;

    internal interface IReleasePackage
    {
        string CreateReleasePackage(string outputFile, string packagesRootDir = null, Func<string, string> releaseNotesProcessor = null, Action<string> contentsPostProcessHook = null);

        string InputPackageFile { get; }

        string ReleasePackageFile { get; }

        string SuggestedReleaseFileName { get; }
    }
}

