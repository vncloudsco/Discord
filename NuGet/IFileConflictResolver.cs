namespace NuGet
{
    using System;

    internal interface IFileConflictResolver
    {
        FileConflictResolution ResolveFileConflict(string message);
    }
}

