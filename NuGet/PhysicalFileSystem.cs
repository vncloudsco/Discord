namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;

    internal class PhysicalFileSystem : IFileSystem
    {
        private readonly string _root;
        private ILogger _logger;

        public PhysicalFileSystem(string root)
        {
            if (string.IsNullOrEmpty(root))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "root");
            }
            this._root = root;
        }

        public virtual void AddFile(string path, Action<Stream> writeToStream)
        {
            if (writeToStream == null)
            {
                throw new ArgumentNullException("writeToStream");
            }
            this.AddFileCore(path, writeToStream);
        }

        public virtual void AddFile(string path, Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            this.AddFileCore(path, targetStream => stream.CopyTo(targetStream));
        }

        private void AddFileCore(string path, Action<Stream> writeToStream)
        {
            this.EnsureDirectory(Path.GetDirectoryName(path));
            using (Stream stream = File.Create(this.GetFullPath(path)))
            {
                writeToStream(stream);
            }
            this.WriteAddedFileAndDirectory(path);
        }

        public virtual void AddFiles(IEnumerable<IPackageFile> files, string rootDir)
        {
            FileSystemExtensions.AddFiles(this, files, rootDir);
        }

        public virtual Stream CreateFile(string path)
        {
            string fullPath = this.GetFullPath(path);
            string directoryName = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            this.WriteAddedFileAndDirectory(path);
            return File.Create(fullPath);
        }

        public virtual void DeleteDirectory(string path)
        {
            this.DeleteDirectory(path, false);
        }

        public virtual void DeleteDirectory(string path, bool recursive)
        {
            if (this.DirectoryExists(path))
            {
                try
                {
                    path = this.GetFullPath(path);
                    Directory.Delete(path, recursive);
                    int num = 0;
                    while (true)
                    {
                        if (!Directory.Exists(path) || (num >= 5))
                        {
                            object[] args = new object[] { path };
                            this.Logger.Log(MessageLevel.Debug, NuGetResources.Debug_RemovedFolder, args);
                            break;
                        }
                        Thread.Sleep(100);
                        num++;
                    }
                }
                catch (DirectoryNotFoundException)
                {
                }
            }
        }

        public virtual void DeleteFile(string path)
        {
            if (this.FileExists(path))
            {
                try
                {
                    this.MakeFileWritable(path);
                    path = this.GetFullPath(path);
                    File.Delete(path);
                    string directoryName = Path.GetDirectoryName(path);
                    if (string.IsNullOrEmpty(directoryName))
                    {
                        object[] args = new object[] { Path.GetFileName(path) };
                        this.Logger.Log(MessageLevel.Debug, NuGetResources.Debug_RemovedFile, args);
                    }
                    else
                    {
                        object[] args = new object[] { Path.GetFileName(path), directoryName };
                        this.Logger.Log(MessageLevel.Debug, NuGetResources.Debug_RemovedFileFromFolder, args);
                    }
                }
                catch (FileNotFoundException)
                {
                }
            }
        }

        public virtual void DeleteFiles(IEnumerable<IPackageFile> files, string rootDir)
        {
            FileSystemExtensions.DeleteFiles(this, files, rootDir);
        }

        public virtual bool DirectoryExists(string path)
        {
            path = this.GetFullPath(path);
            return Directory.Exists(path);
        }

        protected virtual void EnsureDirectory(string path)
        {
            path = this.GetFullPath(path);
            Directory.CreateDirectory(path);
        }

        public virtual bool FileExists(string path)
        {
            path = this.GetFullPath(path);
            return File.Exists(path);
        }

        public DateTimeOffset GetCreated(string path)
        {
            path = this.GetFullPath(path);
            return (!File.Exists(path) ? Directory.GetCreationTimeUtc(path) : File.GetCreationTimeUtc(path));
        }

        public virtual IEnumerable<string> GetDirectories(string path)
        {
            try
            {
                path = PathUtility.EnsureTrailingSlash(this.GetFullPath(path));
                return (Directory.Exists(path) ? Enumerable.Select<string, string>(Directory.EnumerateDirectories(path), new Func<string, string>(this.MakeRelativePath)) : Enumerable.Empty<string>());
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (DirectoryNotFoundException)
            {
            }
            return Enumerable.Empty<string>();
        }

        public virtual IEnumerable<string> GetFiles(string path, bool recursive) => 
            this.GetFiles(path, null, recursive);

        public virtual IEnumerable<string> GetFiles(string path, string filter, bool recursive)
        {
            path = PathUtility.EnsureTrailingSlash(this.GetFullPath(path));
            if (string.IsNullOrEmpty(filter))
            {
                filter = "*.*";
            }
            try
            {
                return (Directory.Exists(path) ? Enumerable.Select<string, string>(Directory.EnumerateFiles(path, filter, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly), new Func<string, string>(this.MakeRelativePath)) : Enumerable.Empty<string>());
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (DirectoryNotFoundException)
            {
            }
            return Enumerable.Empty<string>();
        }

        public virtual string GetFullPath(string path) => 
            (!string.IsNullOrEmpty(path) ? Path.Combine(this.Root, path) : this.Root);

        public DateTimeOffset GetLastAccessed(string path)
        {
            path = this.GetFullPath(path);
            return (!File.Exists(path) ? Directory.GetLastAccessTimeUtc(path) : File.GetLastAccessTimeUtc(path));
        }

        public virtual DateTimeOffset GetLastModified(string path)
        {
            path = this.GetFullPath(path);
            return (!File.Exists(path) ? Directory.GetLastWriteTimeUtc(path) : File.GetLastWriteTimeUtc(path));
        }

        public void MakeFileWritable(string path)
        {
            path = this.GetFullPath(path);
            FileAttributes attributes = File.GetAttributes(path);
            if (attributes.HasFlag(FileAttributes.ReadOnly))
            {
                File.SetAttributes(path, attributes & ~FileAttributes.ReadOnly);
            }
        }

        protected string MakeRelativePath(string fullPath)
        {
            char[] trimChars = new char[] { Path.DirectorySeparatorChar };
            return fullPath.Substring(this.Root.Length).TrimStart(trimChars);
        }

        public virtual void MoveFile(string source, string destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            string fullPath = this.GetFullPath(source);
            string b = this.GetFullPath(destination);
            if (!string.Equals(fullPath, b, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    File.Move(fullPath, b);
                }
                catch (IOException)
                {
                    File.Delete(fullPath);
                }
            }
        }

        public virtual Stream OpenFile(string path)
        {
            path = this.GetFullPath(path);
            return File.OpenRead(path);
        }

        private void WriteAddedFileAndDirectory(string path)
        {
            string directoryName = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directoryName))
            {
                object[] args = new object[] { Path.GetFileName(path) };
                this.Logger.Log(MessageLevel.Debug, NuGetResources.Debug_AddedFile, args);
            }
            else
            {
                object[] args = new object[] { Path.GetFileName(path), directoryName };
                this.Logger.Log(MessageLevel.Debug, NuGetResources.Debug_AddedFileToFolder, args);
            }
        }

        public string Root =>
            this._root;

        public ILogger Logger
        {
            get => 
                (this._logger ?? NullLogger.Instance);
            set => 
                (this._logger = value);
        }
    }
}

