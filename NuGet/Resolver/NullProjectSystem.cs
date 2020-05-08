namespace NuGet.Resolver
{
    using NuGet;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;

    internal class NullProjectSystem : IProjectSystem, IFileSystem, IPropertyProvider
    {
        public void AddFile(string path, Action<Stream> writeToStream)
        {
            throw new NotImplementedException();
        }

        public void AddFile(string path, Stream stream)
        {
            throw new NotImplementedException();
        }

        public void AddFiles(IEnumerable<IPackageFile> files, string rootDir)
        {
            throw new NotImplementedException();
        }

        public void AddFrameworkReference(string name)
        {
            throw new NotImplementedException();
        }

        public void AddImport(string targetFullPath, ProjectImportLocation location)
        {
            throw new NotImplementedException();
        }

        public void AddReference(string referencePath)
        {
            throw new NotImplementedException();
        }

        public Stream CreateFile(string path)
        {
            throw new NotImplementedException();
        }

        public void DeleteDirectory(string path, bool recursive)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(string path)
        {
            throw new NotImplementedException();
        }

        public void DeleteFiles(IEnumerable<IPackageFile> files, string rootDir)
        {
            throw new NotImplementedException();
        }

        public bool DirectoryExists(string path)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string path)
        {
            throw new NotImplementedException();
        }

        public bool FileExistsInProject(string path)
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset GetCreated(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetDirectories(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetFiles(string path, string filter, bool recursive)
        {
            throw new NotImplementedException();
        }

        public string GetFullPath(string path)
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset GetLastAccessed(string path)
        {
            throw new NotImplementedException();
        }

        public DateTimeOffset GetLastModified(string path)
        {
            throw new NotImplementedException();
        }

        [return: Dynamic]
        public object GetPropertyValue(string propertyName)
        {
            throw new NotImplementedException();
        }

        public bool IsSupportedFile(string path)
        {
            throw new NotImplementedException();
        }

        public void MakeFileWritable(string path)
        {
            throw new NotImplementedException();
        }

        public void MoveFile(string source, string destination)
        {
            throw new NotImplementedException();
        }

        public Stream OpenFile(string path)
        {
            throw new NotImplementedException();
        }

        public bool ReferenceExists(string name)
        {
            throw new NotImplementedException();
        }

        public void RemoveImport(string targetFullPath)
        {
            throw new NotImplementedException();
        }

        public void RemoveReference(string name)
        {
            throw new NotImplementedException();
        }

        public string ResolvePath(string path)
        {
            throw new NotImplementedException();
        }

        public FrameworkName TargetFramework =>
            null;

        public string ProjectName =>
            "NullProject";

        public bool IsBindingRedirectSupported =>
            false;

        public ILogger Logger
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Root
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}

