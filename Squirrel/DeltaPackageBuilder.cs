namespace Squirrel
{
    using DeltaCompressionDotNet.MsDelta;
    using ICSharpCode.SharpZipLib.Zip;
    using Splat;
    using Squirrel.Bsdiff;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;

    internal class DeltaPackageBuilder : IEnableLogger, IDeltaPackageBuilder
    {
        private readonly string localAppDirectory;

        public DeltaPackageBuilder(string localAppDataOverride = null)
        {
            this.localAppDirectory = localAppDataOverride;
        }

        public ReleasePackage ApplyDeltaPackage(ReleasePackage basePackage, ReleasePackage deltaPackage, string outputFile)
        {
            string deltaPath;
            using (Utility.WithTempDirectory(out deltaPath, this.localAppDirectory))
            {
                string workingPath;
                using (Utility.WithTempDirectory(out workingPath, this.localAppDirectory))
                {
                    FastZip zip1 = new FastZip();
                    zip1.ExtractZip(deltaPackage.InputPackageFile, deltaPath, null);
                    zip1.ExtractZip(basePackage.InputPackageFile, workingPath, null);
                    List<string> pathsVisited = new List<string>();
                    string[] deltaPathRelativePaths = (from x in new DirectoryInfo(deltaPath).GetAllFilesRecursively() select x.FullName.Replace(deltaPath + Path.DirectorySeparatorChar.ToString(), "")).ToArray<string>();
                    (from x in deltaPathRelativePaths
                        where x.StartsWith("lib", StringComparison.InvariantCultureIgnoreCase)
                        where !x.EndsWith(".shasum", StringComparison.InvariantCultureIgnoreCase)
                        where !x.EndsWith(".diff", StringComparison.InvariantCultureIgnoreCase) || !deltaPathRelativePaths.Contains<string>(x.Replace(".diff", ".bsdiff"))
                        select x).ForEach<string>(delegate (string file) {
                        pathsVisited.Add(Regex.Replace(file, @"\.(bs)?diff$", "").ToLowerInvariant());
                        this.applyDiffToFile(deltaPath, file, workingPath);
                    });
                    (from x in new DirectoryInfo(workingPath).GetAllFilesRecursively()
                        select x.FullName.Replace(workingPath + Path.DirectorySeparatorChar.ToString(), "").ToLowerInvariant() into x
                        where x.StartsWith("lib", StringComparison.InvariantCultureIgnoreCase) && !pathsVisited.Contains(x)
                        select x).ForEach<string>(delegate (string x) {
                        this.Log<DeltaPackageBuilder>().Info<string>("{0} was in old package but not in new one, deleting", x);
                        File.Delete(Path.Combine(workingPath, x));
                    });
                    (from x in deltaPathRelativePaths
                        where !x.StartsWith("lib", StringComparison.InvariantCultureIgnoreCase)
                        select x).ForEach<string>(delegate (string x) {
                        this.Log<DeltaPackageBuilder>().Info<string>("Updating metadata file: {0}", x);
                        File.Copy(Path.Combine(deltaPath, x), Path.Combine(workingPath, x), true);
                    });
                    this.Log<DeltaPackageBuilder>().Info<string>("Repacking into full package: {0}", outputFile);
                    zip1.CreateZip(outputFile, workingPath, true, null);
                }
            }
            return new ReleasePackage(outputFile, false);
        }

        private void applyDiffToFile(string deltaPath, string relativeFilePath, string workingDirectory)
        {
            string inputFile = Path.Combine(deltaPath, relativeFilePath);
            string path = Path.Combine(workingDirectory, Regex.Replace(relativeFilePath, @"\.(bs)?diff$", ""));
            string str2 = null;
            Utility.WithTempFile(out str2, this.localAppDirectory);
            try
            {
                if (new FileInfo(inputFile).Length == 0)
                {
                    this.Log<DeltaPackageBuilder>().Info<string>("{0} exists unchanged, skipping", relativeFilePath);
                }
                else
                {
                    if (relativeFilePath.EndsWith(".bsdiff", StringComparison.InvariantCultureIgnoreCase))
                    {
                        using (FileStream stream = File.OpenWrite(str2))
                        {
                            using (FileStream stream2 = File.OpenRead(path))
                            {
                                this.Log<DeltaPackageBuilder>().Info<string>("Applying BSDiff to {0}", relativeFilePath);
                                BinaryPatchUtility.Apply(stream2, () => File.OpenRead(inputFile), stream);
                            }
                        }
                        this.verifyPatchedFile(relativeFilePath, inputFile, str2);
                    }
                    else if (relativeFilePath.EndsWith(".diff", StringComparison.InvariantCultureIgnoreCase))
                    {
                        this.Log<DeltaPackageBuilder>().Info<string>("Applying MSDiff to {0}", relativeFilePath);
                        new MsDeltaCompression().ApplyDelta(inputFile, path, str2);
                        this.verifyPatchedFile(relativeFilePath, inputFile, str2);
                    }
                    else
                    {
                        using (FileStream stream3 = File.OpenWrite(str2))
                        {
                            using (FileStream stream4 = File.OpenRead(inputFile))
                            {
                                this.Log<DeltaPackageBuilder>().Info<string>("Adding new file: {0}", relativeFilePath);
                                stream4.CopyTo(stream3);
                            }
                        }
                    }
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    DirectoryInfo parent = Directory.GetParent(path);
                    if (!parent.Exists)
                    {
                        parent.Create();
                    }
                    File.Move(str2, path);
                }
            }
            finally
            {
                if (File.Exists(str2))
                {
                    Utility.DeleteFileHarder(str2, true);
                }
            }
        }

        private bool bytesAreIdentical(byte[] oldData, byte[] newData)
        {
            if ((oldData == null) || (newData == null))
            {
                return (oldData == newData);
            }
            if (oldData.Length != newData.Length)
            {
                return false;
            }
            for (long i = 0L; i < newData.Length; i += 1L)
            {
                if (oldData[(int) ((IntPtr) i)] != newData[(int) ((IntPtr) i)])
                {
                    return false;
                }
            }
            return true;
        }

        private void createDeltaForSingleFile(FileInfo targetFile, DirectoryInfo workingDirectory, Dictionary<string, string> baseFileListing)
        {
            string key = targetFile.FullName.Replace(workingDirectory.FullName, "");
            if (!baseFileListing.ContainsKey(key))
            {
                this.Log<DeltaPackageBuilder>().Info<string>("{0} not found in base package, marking as new", key);
            }
            else
            {
                byte[] oldData = File.ReadAllBytes(baseFileListing[key]);
                byte[] newData = File.ReadAllBytes(targetFile.FullName);
                if (this.bytesAreIdentical(oldData, newData))
                {
                    this.Log<DeltaPackageBuilder>().Info<string>("{0} hasn't changed, writing dummy file", key);
                    File.Create(targetFile.FullName + ".diff").Dispose();
                    File.Create(targetFile.FullName + ".shasum").Dispose();
                    targetFile.Delete();
                }
                else
                {
                    this.Log<DeltaPackageBuilder>().Info<string, string>("Delta patching {0} => {1}", baseFileListing[key], targetFile.FullName);
                    MsDeltaCompression compression = new MsDeltaCompression();
                    try
                    {
                        compression.CreateDelta(baseFileListing[key], targetFile.FullName, targetFile.FullName + ".diff");
                    }
                    catch (Win32Exception)
                    {
                        this.Log<DeltaPackageBuilder>().Warn<string>("We couldn't create a delta for {0}, attempting to create bsdiff", targetFile.Name);
                        FileStream stream = null;
                        try
                        {
                            BinaryPatchUtility.Create(oldData, newData, File.Create(targetFile.FullName + ".bsdiff"));
                            File.WriteAllText(targetFile.FullName + ".diff", "1");
                        }
                        catch (Exception exception)
                        {
                            this.Log<DeltaPackageBuilder>().WarnException($"We really couldn't create a delta for {targetFile.Name}", exception);
                            return;
                        }
                        finally
                        {
                            if (stream != null)
                            {
                                stream.Dispose();
                            }
                        }
                    }
                    ReleaseEntry entry = ReleaseEntry.GenerateFromFile(new MemoryStream(newData), targetFile.Name + ".shasum", null);
                    File.WriteAllText(targetFile.FullName + ".shasum", entry.EntryAsString, Encoding.UTF8);
                    targetFile.Delete();
                }
            }
        }

        public ReleasePackage CreateDeltaPackage(ReleasePackage basePackage, ReleasePackage newPackage, string outputFile)
        {
            if (basePackage.Version > newPackage.Version)
            {
                throw new InvalidOperationException($"You cannot create a delta package based on version {basePackage.Version} as it is a later version than {newPackage.Version}");
            }
            if (basePackage.ReleasePackageFile == null)
            {
                throw new ArgumentException("The base package's release file is null", "basePackage");
            }
            if (!File.Exists(basePackage.ReleasePackageFile))
            {
                throw new FileNotFoundException("The base package release does not exist", basePackage.ReleasePackageFile);
            }
            if (!File.Exists(newPackage.ReleasePackageFile))
            {
                throw new FileNotFoundException("The new package release does not exist", newPackage.ReleasePackageFile);
            }
            string path = null;
            string str2 = null;
            using (Utility.WithTempDirectory(out path, null))
            {
                using (Utility.WithTempDirectory(out str2, null))
                {
                    DirectoryInfo baseTempInfo = new DirectoryInfo(path);
                    DirectoryInfo workingDirectory = new DirectoryInfo(str2);
                    this.Log<DeltaPackageBuilder>().Info<string, string, string>("Extracting {0} and {1} into {2}", basePackage.ReleasePackageFile, newPackage.ReleasePackageFile, str2);
                    FastZip zip = new FastZip();
                    zip.ExtractZip(basePackage.ReleasePackageFile, baseTempInfo.FullName, null);
                    zip.ExtractZip(newPackage.ReleasePackageFile, workingDirectory.FullName, null);
                    Dictionary<string, string> baseFileListing = Enumerable.ToDictionary<FileInfo, string, string>(from x in baseTempInfo.GetAllFilesRecursively()
                        where x.FullName.ToLowerInvariant().Contains("lib" + Path.DirectorySeparatorChar.ToString())
                        select x, k => k.FullName.Replace(baseTempInfo.FullName, ""), v => v.FullName);
                    foreach (FileInfo info2 in Enumerable.First<DirectoryInfo>(workingDirectory.GetDirectories(), x => x.Name.ToLowerInvariant() == "lib").GetAllFilesRecursively())
                    {
                        this.createDeltaForSingleFile(info2, workingDirectory, baseFileListing);
                    }
                    ReleasePackage.addDeltaFilesToContentTypes(workingDirectory.FullName);
                    zip.CreateZip(outputFile, workingDirectory.FullName, true, null);
                }
            }
            return new ReleasePackage(outputFile, false);
        }

        private void verifyPatchedFile(string relativeFilePath, string inputFile, string tempTargetFile)
        {
            ReleaseEntry entry = ReleaseEntry.ParseReleaseEntry(File.ReadAllText(Regex.Replace(inputFile, @"\.(bs)?diff$", ".shasum"), Encoding.UTF8));
            ReleaseEntry entry2 = ReleaseEntry.GenerateFromFile(tempTargetFile, null);
            if (entry.Filesize != entry2.Filesize)
            {
                this.Log<DeltaPackageBuilder>().Warn<string, long, long>("Patched file {0} has incorrect size, expected {1}, got {2}", relativeFilePath, entry.Filesize, entry2.Filesize);
                ChecksumFailedException exception1 = new ChecksumFailedException();
                exception1.Filename = relativeFilePath;
                throw exception1;
            }
            if (entry.SHA1 != entry2.SHA1)
            {
                this.Log<DeltaPackageBuilder>().Warn<string, string, string>("Patched file {0} has incorrect SHA1, expected {1}, got {2}", relativeFilePath, entry.SHA1, entry2.SHA1);
                ChecksumFailedException exception2 = new ChecksumFailedException();
                exception2.Filename = relativeFilePath;
                throw exception2;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly DeltaPackageBuilder.<>c <>9 = new DeltaPackageBuilder.<>c();
            public static Func<FileInfo, bool> <>9__2_0;
            public static Func<FileInfo, string> <>9__2_2;
            public static Func<DirectoryInfo, bool> <>9__2_3;
            public static Func<string, bool> <>9__3_1;
            public static Func<string, bool> <>9__3_2;
            public static Func<string, bool> <>9__3_8;

            internal bool <ApplyDeltaPackage>b__3_1(string x) => 
                x.StartsWith("lib", StringComparison.InvariantCultureIgnoreCase);

            internal bool <ApplyDeltaPackage>b__3_2(string x) => 
                !x.EndsWith(".shasum", StringComparison.InvariantCultureIgnoreCase);

            internal bool <ApplyDeltaPackage>b__3_8(string x) => 
                !x.StartsWith("lib", StringComparison.InvariantCultureIgnoreCase);

            internal bool <CreateDeltaPackage>b__2_0(FileInfo x) => 
                x.FullName.ToLowerInvariant().Contains("lib" + Path.DirectorySeparatorChar.ToString());

            internal string <CreateDeltaPackage>b__2_2(FileInfo v) => 
                v.FullName;

            internal bool <CreateDeltaPackage>b__2_3(DirectoryInfo x) => 
                (x.Name.ToLowerInvariant() == "lib");
        }
    }
}

