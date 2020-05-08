namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal class NullFileSystem : IFileSystem
    {
        private static readonly NullFileSystem _instance = new NullFileSystem();

        private NullFileSystem()
        {
        }

        public void AddFile(string path, Action<Stream> writeToStream)
        {
        }

        public void AddFile(string path, Stream stream)
        {
        }

        public void AddFiles(IEnumerable<IPackageFile> files, string rootDir)
        {
        }

        public Stream CreateFile(string path) => 
            Stream.Null;

        public void DeleteDirectory(string path, bool recursive)
        {
        }

        public void DeleteFile(string path)
        {
        }

        public void DeleteFiles(IEnumerable<IPackageFile> files, string rootDir)
        {
        }

        public bool DirectoryExists(string path) => 
            false;

        public bool FileExists(string path) => 
            false;

        public DateTimeOffset GetCreated(string path) => 
            DateTimeOffset.MinValue;

        public IEnumerable<string> GetDirectories(string path) => 
            Enumerable.Empty<string>();

        public IEnumerable<string> GetFiles(string path, string filter, bool recursive) => 
            Enumerable.Empty<string>();

        public string GetFullPath(string path) => 
            path;

        public DateTimeOffset GetLastAccessed(string path) => 
            DateTimeOffset.MinValue;

        public DateTimeOffset GetLastModified(string path) => 
            DateTimeOffset.MinValue;

        public void MakeFileWritable(string path)
        {
        }

        public void MoveFile(string source, string destination)
        {
        }

        public Stream OpenFile(string path) => 
            Stream.Null;

        public static NullFileSystem Instance =>
            _instance;

        public ILogger Logger { get; set; }

        public string Root =>
            string.Empty;
    }
}

