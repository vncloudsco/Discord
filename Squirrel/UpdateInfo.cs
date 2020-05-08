namespace Squirrel
{
    using NuGet;
    using Splat;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class UpdateInfo : IEnableLogger
    {
        protected UpdateInfo(ReleaseEntry currentlyInstalledVersion, IEnumerable<ReleaseEntry> releasesToApply, string packageDirectory)
        {
            this.CurrentlyInstalledVersion = currentlyInstalledVersion;
            this.ReleasesToApply = (releasesToApply ?? Enumerable.Empty<ReleaseEntry>()).ToList<ReleaseEntry>();
            this.FutureReleaseEntry = this.ReleasesToApply.Any<ReleaseEntry>() ? this.ReleasesToApply.MaxBy<ReleaseEntry, SemanticVersion>(x => x.Version).FirstOrDefault<ReleaseEntry>() : this.CurrentlyInstalledVersion;
            this.PackageDirectory = packageDirectory;
        }

        public static UpdateInfo Create(ReleaseEntry currentVersion, IEnumerable<ReleaseEntry> availableReleases, string packageDirectory)
        {
            ReleaseEntry entry = Enumerable.FirstOrDefault<ReleaseEntry>(availableReleases.MaxBy<ReleaseEntry, SemanticVersion>(x => x.Version), x => !x.IsDelta);
            if (entry == null)
            {
                throw new Exception("There should always be at least one full release");
            }
            if (currentVersion == null)
            {
                ReleaseEntry[] entryArray1 = new ReleaseEntry[] { entry };
                return new UpdateInfo(currentVersion, entryArray1, packageDirectory);
            }
            if (currentVersion.Version == entry.Version)
            {
                return new UpdateInfo(currentVersion, Enumerable.Empty<ReleaseEntry>(), packageDirectory);
            }
            IOrderedEnumerable<ReleaseEntry> enumerable = from v in availableReleases
                where v.Version > currentVersion.Version
                orderby v.Version
                select v;
            long num = Enumerable.Sum<ReleaseEntry>(from x in enumerable
                where x.IsDelta
                select x, x => x.Filesize);
            if ((num < entry.Filesize) && (num > 0L))
            {
                return new UpdateInfo(currentVersion, (from x in enumerable
                    where x.IsDelta
                    select x).ToArray<ReleaseEntry>(), packageDirectory);
            }
            ReleaseEntry[] releasesToApply = new ReleaseEntry[] { entry };
            return new UpdateInfo(currentVersion, releasesToApply, packageDirectory);
        }

        public Dictionary<ReleaseEntry, string> FetchReleaseNotes() => 
            Enumerable.ToDictionary<Tuple<ReleaseEntry, string>, ReleaseEntry, string>(Enumerable.SelectMany<ReleaseEntry, Tuple<ReleaseEntry, string>>(this.ReleasesToApply, delegate (ReleaseEntry x) {
                try
                {
                    string releaseNotes = x.GetReleaseNotes(this.PackageDirectory);
                    return EnumerableExtensions.Return<Tuple<ReleaseEntry, string>>(Tuple.Create<ReleaseEntry, string>(x, releaseNotes));
                }
                catch (Exception exception)
                {
                    this.Log<UpdateInfo>().WarnException("Couldn't get release notes for:" + x.Filename, exception);
                    return Enumerable.Empty<Tuple<ReleaseEntry, string>>();
                }
            }), k => k.Item1, v => v.Item2);

        [DataMember]
        public ReleaseEntry CurrentlyInstalledVersion { get; protected set; }

        [DataMember]
        public ReleaseEntry FutureReleaseEntry { get; protected set; }

        [DataMember]
        public List<ReleaseEntry> ReleasesToApply { get; protected set; }

        [IgnoreDataMember]
        public bool IsBootstrapping =>
            ReferenceEquals(this.CurrentlyInstalledVersion, null);

        [IgnoreDataMember]
        public string PackageDirectory { get; protected set; }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly UpdateInfo.<>c <>9 = new UpdateInfo.<>c();
            public static Func<ReleaseEntry, SemanticVersion> <>9__18_0;
            public static Func<Tuple<ReleaseEntry, string>, ReleaseEntry> <>9__19_1;
            public static Func<Tuple<ReleaseEntry, string>, string> <>9__19_2;
            public static Func<ReleaseEntry, SemanticVersion> <>9__20_0;
            public static Func<ReleaseEntry, bool> <>9__20_1;
            public static Func<ReleaseEntry, SemanticVersion> <>9__20_3;
            public static Func<ReleaseEntry, bool> <>9__20_4;
            public static Func<ReleaseEntry, long> <>9__20_5;
            public static Func<ReleaseEntry, bool> <>9__20_6;

            internal SemanticVersion <.ctor>b__18_0(ReleaseEntry x) => 
                x.Version;

            internal SemanticVersion <Create>b__20_0(ReleaseEntry x) => 
                x.Version;

            internal bool <Create>b__20_1(ReleaseEntry x) => 
                !x.IsDelta;

            internal SemanticVersion <Create>b__20_3(ReleaseEntry v) => 
                v.Version;

            internal bool <Create>b__20_4(ReleaseEntry x) => 
                x.IsDelta;

            internal long <Create>b__20_5(ReleaseEntry x) => 
                x.Filesize;

            internal bool <Create>b__20_6(ReleaseEntry x) => 
                x.IsDelta;

            internal ReleaseEntry <FetchReleaseNotes>b__19_1(Tuple<ReleaseEntry, string> k) => 
                k.Item1;

            internal string <FetchReleaseNotes>b__19_2(Tuple<ReleaseEntry, string> v) => 
                v.Item2;
        }
    }
}

