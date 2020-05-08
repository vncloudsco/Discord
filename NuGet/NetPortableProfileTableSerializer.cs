namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Json;
    using System.Runtime.Versioning;

    internal static class NetPortableProfileTableSerializer
    {
        private static readonly DataContractJsonSerializer _serializer = new DataContractJsonSerializer(typeof(IEnumerable<PortableProfile>));

        internal static NetPortableProfileTable Deserialize(Stream input) => 
            new NetPortableProfileTable(from p in (IEnumerable<PortableProfile>) _serializer.ReadObject(input) select new NetPortableProfile(p.FrameworkVersion, p.Name, from f in p.SupportedFrameworks select new FrameworkName(f), from f in p.OptionalFrameworks select new FrameworkName(f)));

        internal static void Serialize(NetPortableProfileTable portableProfileTable, Stream output)
        {
            IEnumerable<PortableProfile> graph = Enumerable.Select<NetPortableProfile, PortableProfile>(portableProfileTable.Profiles, delegate (NetPortableProfile p) {
                PortableProfile profile1 = new PortableProfile();
                profile1.Name = p.Name;
                profile1.FrameworkVersion = p.FrameworkVersion;
                profile1.SupportedFrameworks = (from f in p.SupportedFrameworks select f.get_FullName()).ToArray<string>();
                PortableProfile local3 = profile1;
                PortableProfile local4 = profile1;
                local4.OptionalFrameworks = (from f in p.OptionalFrameworks select f.get_FullName()).ToArray<string>();
                return local4;
            });
            _serializer.WriteObject(output, graph);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly NetPortableProfileTableSerializer.<>c <>9 = new NetPortableProfileTableSerializer.<>c();
            public static Func<FrameworkName, string> <>9__1_1;
            public static Func<FrameworkName, string> <>9__1_2;
            public static Func<NetPortableProfile, NetPortableProfileTableSerializer.PortableProfile> <>9__1_0;
            public static Func<string, FrameworkName> <>9__2_1;
            public static Func<string, FrameworkName> <>9__2_2;
            public static Func<NetPortableProfileTableSerializer.PortableProfile, NetPortableProfile> <>9__2_0;

            internal NetPortableProfile <Deserialize>b__2_0(NetPortableProfileTableSerializer.PortableProfile p) => 
                new NetPortableProfile(p.FrameworkVersion, p.Name, from f in p.SupportedFrameworks select new FrameworkName(f), from f in p.OptionalFrameworks select new FrameworkName(f));

            internal FrameworkName <Deserialize>b__2_1(string f) => 
                new FrameworkName(f);

            internal FrameworkName <Deserialize>b__2_2(string f) => 
                new FrameworkName(f);

            internal NetPortableProfileTableSerializer.PortableProfile <Serialize>b__1_0(NetPortableProfile p)
            {
                NetPortableProfileTableSerializer.PortableProfile profile1 = new NetPortableProfileTableSerializer.PortableProfile();
                profile1.Name = p.Name;
                profile1.FrameworkVersion = p.FrameworkVersion;
                profile1.SupportedFrameworks = (from f in p.SupportedFrameworks select f.get_FullName()).ToArray<string>();
                NetPortableProfileTableSerializer.PortableProfile local3 = profile1;
                NetPortableProfileTableSerializer.PortableProfile local4 = profile1;
                local4.OptionalFrameworks = (from f in p.OptionalFrameworks select f.get_FullName()).ToArray<string>();
                return local4;
            }

            internal string <Serialize>b__1_1(FrameworkName f) => 
                f.get_FullName();

            internal string <Serialize>b__1_2(FrameworkName f) => 
                f.get_FullName();
        }

        [DataContract]
        private class PortableProfile
        {
            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public string FrameworkVersion { get; set; }

            [DataMember]
            public string[] SupportedFrameworks { get; set; }

            [DataMember]
            public string[] OptionalFrameworks { get; set; }
        }
    }
}

