namespace NuGet
{
    using NuGet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;

    internal class MachineCache : LocalPackageRepository, IPackageCacheRepository, IPackageRepository
    {
        private const int MaxPackages = 200;
        private const string NuGetCachePathEnvironmentVariable = "NuGetCachePath";
        private static readonly Lazy<MachineCache> _instance = new Lazy<MachineCache>(() => CreateDefault(new Func<string>(MachineCache.GetCachePath)));

        internal MachineCache(IFileSystem fileSystem) : base(new DefaultPackagePathResolver(fileSystem), fileSystem, false)
        {
        }

        public override void AddPackage(IPackage package)
        {
            List<string> list = base.GetPackageFiles(null).ToList<string>();
            if (list.Count >= 200)
            {
                IFileSystem fileSystem = base.FileSystem;
                List<string> files = Enumerable.OrderBy<string, DateTimeOffset>(list, new Func<string, DateTimeOffset>(fileSystem.GetLastAccessed)).Take<string>((list.Count - 160)).ToList<string>();
                this.TryClear(files);
            }
            string path = this.GetPackageFilePath(package);
            this.TryAct(delegate {
                if (!this.FileSystem.FileExists(path))
                {
                    string tempFile = GetTempFile(path);
                    using (Stream stream = package.GetStream())
                    {
                        this.FileSystem.AddFile(tempFile, stream);
                    }
                    this.FileSystem.MoveFile(tempFile, path);
                }
                return true;
            }, path);
        }

        public void Clear()
        {
            this.TryClear(base.GetPackageFiles(null).ToList<string>());
        }

        internal static MachineCache CreateDefault(Func<string> getCachePath)
        {
            IFileSystem instance;
            try
            {
                string str = getCachePath();
                instance = !string.IsNullOrEmpty(str) ? ((IFileSystem) new PhysicalFileSystem(str)) : ((IFileSystem) NullFileSystem.Instance);
            }
            catch (SecurityException)
            {
                instance = NullFileSystem.Instance;
            }
            return new MachineCache(instance);
        }

        public override bool Exists(string packageId, SemanticVersion version)
        {
            string packagePath = this.GetPackageFilePath(packageId, version);
            return this.TryAct(() => this.FileSystem.FileExists(packagePath), packagePath);
        }

        internal static string GetCachePath() => 
            GetCachePath(new Func<string, string>(Environment.GetEnvironmentVariable), new Func<Environment.SpecialFolder, string>(Environment.GetFolderPath));

        internal static string GetCachePath(Func<string, string> getEnvironmentVariable, Func<Environment.SpecialFolder, string> getFolderPath)
        {
            string str = getEnvironmentVariable("NuGetCachePath");
            if (!string.IsNullOrEmpty(str))
            {
                return str;
            }
            string str2 = getFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (string.IsNullOrEmpty(str2))
            {
                str2 = getEnvironmentVariable("LocalAppData");
            }
            return (!string.IsNullOrEmpty(str2) ? Path.Combine(str2, "NuGet", "Cache") : null);
        }

        protected override string GetPackageFilePath(IPackage package) => 
            Path.GetFileName(base.GetPackageFilePath(package));

        protected override string GetPackageFilePath(string id, SemanticVersion version) => 
            Path.GetFileName(base.GetPackageFilePath(id, version));

        private static string GetTempFile(string filename) => 
            (filename + ".tmp");

        public bool InvokeOnPackage(string packageId, SemanticVersion version, Action<Stream> action)
        {
            if (base.FileSystem is NullFileSystem)
            {
                return false;
            }
            string packagePath = this.GetPackageFilePath(packageId, version);
            return this.TryAct(delegate {
                IPackage package;
                string tempFile = GetTempFile(packagePath);
                using (Stream stream = this.FileSystem.CreateFile(tempFile))
                {
                    bool flag;
                    if (stream != null)
                    {
                        action(stream);
                        if ((stream == null) || (stream.Length == 0))
                        {
                            flag = false;
                        }
                        else
                        {
                            goto TR_0005;
                        }
                    }
                    else
                    {
                        flag = false;
                    }
                    return flag;
                }
            TR_0005:
                package = this.OpenPackage(this.FileSystem.GetFullPath(tempFile));
                packagePath = this.GetPackageFilePath(package.Id, package.Version);
                this.FileSystem.DeleteFile(packagePath);
                this.FileSystem.MoveFile(tempFile, packagePath);
                return true;
            }, packagePath);
        }

        protected override IPackage OpenPackage(string path)
        {
            OptimizedZipPackage package;
            try
            {
                package = new OptimizedZipPackage(base.FileSystem, path);
            }
            catch (FileFormatException exception)
            {
                object[] args = new object[] { path };
                throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, NuGetResources.ErrorReadingPackage, args), exception);
            }
            return package;
        }

        private bool TryAct(Func<bool> action, string path)
        {
            try
            {
                bool flag2;
                using (Mutex mutex = new Mutex(false, @"Global\" + EncryptionUtility.GenerateUniqueToken(base.FileSystem.GetFullPath(path) ?? path)))
                {
                    bool flag = false;
                    try
                    {
                        try
                        {
                            flag = mutex.WaitOne(TimeSpan.FromMinutes(3.0));
                        }
                        catch (AbandonedMutexException)
                        {
                            flag = true;
                        }
                        flag2 = action();
                    }
                    finally
                    {
                        if (flag)
                        {
                            mutex.ReleaseMutex();
                        }
                    }
                }
                return flag2;
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            return false;
        }

        private void TryClear(IEnumerable<string> files)
        {
            foreach (string packageFile in files)
            {
                this.TryAct(delegate {
                    this.FileSystem.DeleteFileSafe(packageFile);
                    return true;
                }, packageFile);
            }
        }

        public static MachineCache Default =>
            _instance.Value;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly MachineCache.<>c <>9 = new MachineCache.<>c();

            internal MachineCache <.cctor>b__19_0() => 
                MachineCache.CreateDefault(new Func<string>(MachineCache.GetCachePath));
        }
    }
}

