namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal static class PackageSourceProviderExtensions
    {
        public static AggregateRepository CreateAggregateRepository(this IPackageSourceProvider provider, IPackageRepositoryFactory factory, bool ignoreFailingRepositories) => 
            new AggregateRepository(factory, from s in provider.GetEnabledPackageSources() select s.Source, ignoreFailingRepositories);

        public static IPackageRepository CreatePriorityPackageRepository(this IPackageSourceProvider provider, IPackageRepositoryFactory factory, IPackageRepository primaryRepository)
        {
            PackageSource[] sources = (from s in provider.GetEnabledPackageSources()
                where !s.Source.Equals(primaryRepository.Source, StringComparison.OrdinalIgnoreCase)
                select s).ToArray<PackageSource>();
            if (sources.Length == 0)
            {
                return primaryRepository;
            }
            return new PriorityPackageRepository(primaryRepository, AggregateRepository.Create(factory, sources, true));
        }

        public static IEnumerable<PackageSource> GetEnabledPackageSources(this IPackageSourceProvider provider) => 
            (from p in provider.LoadPackageSources()
                where p.IsEnabled
                select p);

        public static string ResolveSource(this IPackageSourceProvider provider, string value) => 
            ((from source in provider.GetEnabledPackageSources()
                where source.Name.Equals(value, StringComparison.CurrentCultureIgnoreCase) || source.Source.Equals(value, StringComparison.OrdinalIgnoreCase)
                select source.Source).FirstOrDefault<string>() ?? value);

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PackageSourceProviderExtensions.<>c <>9 = new PackageSourceProviderExtensions.<>c();
            public static Func<PackageSource, string> <>9__0_0;
            public static Func<PackageSource, string> <>9__2_1;
            public static Func<PackageSource, bool> <>9__3_0;

            internal string <CreateAggregateRepository>b__0_0(PackageSource s) => 
                s.Source;

            internal bool <GetEnabledPackageSources>b__3_0(PackageSource p) => 
                p.IsEnabled;

            internal string <ResolveSource>b__2_1(PackageSource source) => 
                source.Source;
        }
    }
}

