namespace Squirrel
{
    using NuGet;
    using System;

    internal interface IReleaseEntry
    {
        Uri GetIconUrl(string packageDirectory);
        string GetReleaseNotes(string packageDirectory);

        string SHA1 { get; }

        string Filename { get; }

        long Filesize { get; }

        bool IsDelta { get; }

        string EntryAsString { get; }

        SemanticVersion Version { get; }

        string PackageName { get; }
    }
}

