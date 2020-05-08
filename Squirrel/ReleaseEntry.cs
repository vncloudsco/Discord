namespace Squirrel
{
    using NuGet;
    using Splat;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    [DataContract]
    internal class ReleaseEntry : IEnableLogger, IReleaseEntry
    {
        private static readonly Regex entryRegex = new Regex(@"^([0-9a-fA-F]{40})\s+(\S+)\s+(\d+)[\r]*$");
        private static readonly Regex commentRegex = new Regex("#.*$");

        protected ReleaseEntry(string sha1, string filename, long filesize, bool isDelta, string baseUrl = null, string query = null)
        {
            this.SHA1 = sha1;
            this.BaseUrl = baseUrl;
            this.Filename = filename;
            this.Query = query;
            this.Filesize = filesize;
            this.IsDelta = isDelta;
        }

        public static List<ReleaseEntry> BuildReleasesFile(string releasePackagesDir)
        {
            DirectoryInfo info = new DirectoryInfo(releasePackagesDir);
            ConcurrentQueue<ReleaseEntry> entriesQueue = new ConcurrentQueue<ReleaseEntry>();
            Parallel.ForEach<FileInfo>(info.GetFiles("*.nupkg"), delegate (FileInfo x) {
                using (FileStream stream = x.OpenRead())
                {
                    entriesQueue.Enqueue(GenerateFromFile(stream, x.Name, null));
                }
            });
            List<ReleaseEntry> releaseEntries = entriesQueue.ToList<ReleaseEntry>();
            string path = null;
            Utility.WithTempFile(out path, releasePackagesDir);
            try
            {
                using (FileStream stream = File.OpenWrite(path))
                {
                    if (releaseEntries.Count > 0)
                    {
                        WriteReleaseFile(releaseEntries, stream);
                    }
                }
                string str2 = Path.Combine(info.FullName, "RELEASES");
                if (File.Exists(str2))
                {
                    File.Delete(str2);
                }
                File.Move(path, str2);
            }
            finally
            {
                if (File.Exists(path))
                {
                    Utility.DeleteFileHarder(path, true);
                }
            }
            return releaseEntries;
        }

        private static bool filenameIsDeltaFile(string filename) => 
            filename.EndsWith("-delta.nupkg", StringComparison.InvariantCultureIgnoreCase);

        public static ReleaseEntry GenerateFromFile(string path, string baseUrl = null)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                return GenerateFromFile(stream, Path.GetFileName(path), baseUrl);
            }
        }

        public static ReleaseEntry GenerateFromFile(Stream file, string filename, string baseUrl = null) => 
            new ReleaseEntry(Utility.CalculateStreamSHA1(file), filename, file.Length, filenameIsDeltaFile(filename), baseUrl, null);

        public Uri GetIconUrl(string packageDirectory) => 
            new ZipPackage(Path.Combine(packageDirectory, this.Filename)).IconUrl;

        public static ReleasePackage GetPreviousRelease(IEnumerable<ReleaseEntry> releaseEntries, IReleasePackage package, string targetDir) => 
            (((releaseEntries == null) || !releaseEntries.Any<ReleaseEntry>()) ? null : (from x in releaseEntries
                where !x.IsDelta
                where x.Version < package.ToSemanticVersion()
                orderby x.Version descending
                select new ReleasePackage(Path.Combine(targetDir, x.Filename), true)).FirstOrDefault<ReleasePackage>());

        public string GetReleaseNotes(string packageDirectory)
        {
            ZipPackage package1 = new ZipPackage(Path.Combine(packageDirectory, this.Filename));
            string id = package1.Id;
            if (string.IsNullOrWhiteSpace(package1.ReleaseNotes))
            {
                throw new Exception($"Invalid 'ReleaseNotes' value in nuspec file at '{Path.Combine(packageDirectory, this.Filename)}'");
            }
            return package1.ReleaseNotes;
        }

        public static ReleaseEntry ParseReleaseEntry(string entry)
        {
            entry = commentRegex.Replace(entry, "");
            if (string.IsNullOrWhiteSpace(entry))
            {
                return null;
            }
            Match match1 = entryRegex.Match(entry);
            if (!match1.Success)
            {
                throw new Exception("Invalid release entry: " + entry);
            }
            Match local1 = match1;
            if (local1.Groups.Count != 4)
            {
                throw new Exception("Invalid release entry: " + entry);
            }
            Match local2 = local1;
            string urlOrPath = local2.Groups[2].Value;
            string baseUrl = null;
            string query = null;
            if (Utility.IsHttpUrl(urlOrPath))
            {
                Uri uri = new Uri(urlOrPath);
                string localPath = uri.LocalPath;
                string leftPart = uri.GetLeftPart(UriPartial.Authority);
                if (string.IsNullOrEmpty(localPath) || string.IsNullOrEmpty(leftPart))
                {
                    throw new Exception("Invalid URL");
                }
                int length = localPath.LastIndexOf("/") + 1;
                baseUrl = leftPart + localPath.Substring(0, length);
                urlOrPath = localPath.Substring(length);
                if (!string.IsNullOrEmpty(uri.Query))
                {
                    query = uri.Query;
                }
            }
            if (urlOrPath.IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
            {
                throw new Exception("Filename can either be an absolute HTTP[s] URL, *or* a file name");
            }
            Match local3 = local2;
            return new ReleaseEntry(local3.Groups[1].Value, urlOrPath, long.Parse(local3.Groups[3].Value), filenameIsDeltaFile(urlOrPath), baseUrl, query);
        }

        public static IEnumerable<ReleaseEntry> ParseReleaseFile(string fileContents)
        {
            if (string.IsNullOrEmpty(fileContents))
            {
                return new ReleaseEntry[0];
            }
            fileContents = Utility.RemoveByteOrderMarkerIfPresent(fileContents);
            if (fileContents.StartsWith("<?xml"))
            {
                return new ReleaseEntry[0];
            }
            char[] separator = new char[] { '\n' };
            ReleaseEntry[] entryArray = (from x in Enumerable.Select<string, ReleaseEntry>(from x in fileContents.Split(separator)
                where !string.IsNullOrWhiteSpace(x)
                select x, new Func<string, ReleaseEntry>(ReleaseEntry.ParseReleaseEntry))
                where x != null
                select x).ToArray<ReleaseEntry>();
            return (Enumerable.Any<ReleaseEntry>(entryArray, x => ReferenceEquals(x, null)) ? ((IEnumerable<ReleaseEntry>) new ReleaseEntry[0]) : ((IEnumerable<ReleaseEntry>) entryArray));
        }

        public static void WriteReleaseFile(IEnumerable<ReleaseEntry> releaseEntries, Stream stream)
        {
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.Write(string.Join("\n", (IEnumerable<string>) (from x in releaseEntries
                    orderby x.Version, x.IsDelta descending
                    select x.EntryAsString)));
            }
        }

        public static void WriteReleaseFile(IEnumerable<ReleaseEntry> releaseEntries, string path)
        {
            using (FileStream stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                WriteReleaseFile(releaseEntries, stream);
            }
        }

        [DataMember]
        public string SHA1 { get; protected set; }

        [DataMember]
        public string BaseUrl { get; protected set; }

        [DataMember]
        public string Filename { get; protected set; }

        [DataMember]
        public string Query { get; protected set; }

        [DataMember]
        public long Filesize { get; protected set; }

        [DataMember]
        public bool IsDelta { get; protected set; }

        [IgnoreDataMember]
        public string EntryAsString =>
            $"{this.SHA1} {this.BaseUrl}{this.Filename} {this.Filesize}";

        [IgnoreDataMember]
        public SemanticVersion Version =>
            this.Filename.ToSemanticVersion();

        [IgnoreDataMember]
        public string PackageName
        {
            get
            {
                char[] anyOf = new char[] { '-', '.' };
                return this.Filename.Substring(0, this.Filename.IndexOfAny(anyOf));
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly ReleaseEntry.<>c <>9 = new ReleaseEntry.<>c();
            public static Func<string, bool> <>9__36_0;
            public static Func<ReleaseEntry, bool> <>9__36_1;
            public static Func<ReleaseEntry, bool> <>9__36_2;
            public static Func<ReleaseEntry, SemanticVersion> <>9__37_0;
            public static Func<ReleaseEntry, bool> <>9__37_1;
            public static Func<ReleaseEntry, string> <>9__37_2;
            public static Func<ReleaseEntry, bool> <>9__43_0;
            public static Func<ReleaseEntry, SemanticVersion> <>9__43_2;

            internal bool <GetPreviousRelease>b__43_0(ReleaseEntry x) => 
                !x.IsDelta;

            internal SemanticVersion <GetPreviousRelease>b__43_2(ReleaseEntry x) => 
                x.Version;

            internal bool <ParseReleaseFile>b__36_0(string x) => 
                !string.IsNullOrWhiteSpace(x);

            internal bool <ParseReleaseFile>b__36_1(ReleaseEntry x) => 
                (x != null);

            internal bool <ParseReleaseFile>b__36_2(ReleaseEntry x) => 
                ReferenceEquals(x, null);

            internal SemanticVersion <WriteReleaseFile>b__37_0(ReleaseEntry x) => 
                x.Version;

            internal bool <WriteReleaseFile>b__37_1(ReleaseEntry x) => 
                x.IsDelta;

            internal string <WriteReleaseFile>b__37_2(ReleaseEntry x) => 
                x.EntryAsString;
        }
    }
}

