namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal interface IFileSystem
    {
        void AddFile(string path, Action<Stream> writeToStream);
        void AddFile(string path, Stream stream);
        void AddFiles(IEnumerable<IPackageFile> files, string rootDir);
        Stream CreateFile(string path);
        void DeleteDirectory(string path, bool recursive);
        void DeleteFile(string path);
        void DeleteFiles(IEnumerable<IPackageFile> files, string rootDir);
        bool DirectoryExists(string path);
        bool FileExists(string path);
        DateTimeOffset GetCreated(string path);
        IEnumerable<string> GetDirectories(string path);
        IEnumerable<string> GetFiles(string path, string filter, bool recursive);
        string GetFullPath(string path);
        DateTimeOffset GetLastAccessed(string path);
        DateTimeOffset GetLastModified(string path);
        void MakeFileWritable(string path);
        void MoveFile(string source, string destination);
        Stream OpenFile(string path);

        ILogger Logger { get; set; }

        string Root { get; }
    }
}

