namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;

    internal static class PackageExtensions
    {
        private const string TagsProperty = "Tags";
        private static readonly string[] _packagePropertiesToSearch = new string[] { "Id", "Description", "Tags" };

        public static IEnumerable<IPackage> AsCollapsed(this IEnumerable<IPackage> source) => 
            source.DistinctLast<IPackage>(((IEqualityComparer<IPackage>) PackageEqualityComparer.Id), PackageComparer.Version);

        private static Expression BuildExpressionForTerm(ParameterExpression packageParameterExpression, string term, string propertyName)
        {
            if (propertyName.Equals("Tags", StringComparison.OrdinalIgnoreCase))
            {
                term = " " + term + " ";
            }
            Type[] types = new Type[] { typeof(string) };
            MethodInfo method = typeof(string).GetMethod("Contains", types);
            MethodInfo info2 = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
            MemberExpression instance = !propertyName.Equals("Id", StringComparison.OrdinalIgnoreCase) ? Expression.Property(packageParameterExpression, propertyName) : Expression.Property(Expression.TypeAs(packageParameterExpression, typeof(IPackageName)), propertyName);
            MethodCallExpression expression2 = Expression.Call(instance, info2);
            Expression[] arguments = new Expression[] { Expression.Constant(term.ToLower()) };
            return Expression.AndAlso(Expression.NotEqual(instance, Expression.Constant(null)), Expression.Call(expression2, method, arguments));
        }

        private static Expression<Func<T, bool>> BuildSearchExpression<T>(IEnumerable<string> propertiesToSearch, IEnumerable<string> searchTerms) where T: IPackage
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(IPackageMetadata));
            ParameterExpression[] parameters = new ParameterExpression[] { parameterExpression };
            return Expression.Lambda<Func<T, bool>>(Enumerable.Aggregate<Expression>(from term in searchTerms
                from property in propertiesToSearch
                select BuildExpressionForTerm(parameterExpression, term, property), new Func<Expression, Expression, Expression>(Expression.OrElse)), parameters);
        }

        internal static IEnumerable<IPackage> CollapseById(this IEnumerable<IPackage> source) => 
            (from g in Enumerable.GroupBy<IPackage, string>(source, p => p.Id, StringComparer.OrdinalIgnoreCase) select (from p in g
                orderby p.Version descending
                select p).First<IPackage>());

        public static IEnumerable<IPackage> FilterByPrerelease(this IEnumerable<IPackage> packages, bool allowPrerelease)
        {
            if (packages == null)
            {
                return null;
            }
            if (!allowPrerelease)
            {
                packages = from p in packages
                    where p.IsReleaseVersion()
                    select p;
            }
            return packages;
        }

        public static IQueryable<T> Find<T>(this IQueryable<T> packages, string searchText) where T: IPackage => 
            packages.Find<T>(_packagePropertiesToSearch, searchText);

        private static IQueryable<T> Find<T>(this IQueryable<T> packages, IEnumerable<string> propertiesToSearch, IEnumerable<string> searchTerms) where T: IPackage
        {
            if (!searchTerms.Any<string>())
            {
                return packages;
            }
            IEnumerable<string> source = from s in searchTerms
                where s != null
                select s;
            return (source.Any<string>() ? Queryable.Where<T>(packages, BuildSearchExpression<T>(propertiesToSearch, source)) : packages);
        }

        public static IQueryable<T> Find<T>(this IQueryable<T> packages, IEnumerable<string> propertiesToSearch, string searchText) where T: IPackage
        {
            if (propertiesToSearch.IsEmpty<string>())
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "propertiesToSearch");
            }
            return (!string.IsNullOrEmpty(searchText) ? packages.Find<T>(propertiesToSearch, searchText.Split(new char[0])) : packages);
        }

        public static IEnumerable<IPackage> FindByVersion(this IEnumerable<IPackage> source, IVersionSpec versionSpec)
        {
            if (versionSpec == null)
            {
                throw new ArgumentNullException("versionSpec");
            }
            return Enumerable.Where<IPackage>(source, versionSpec.ToDelegate());
        }

        public static IQueryable<T> FindLatestVersion<T>(this IQueryable<T> packages) where T: IPackage
        {
            ParameterExpression expression = Expression.Parameter(typeof(T), "p");
            ParameterExpression[] parameters = new ParameterExpression[] { expression };
            return Queryable.Where<T>(packages, Expression.Lambda<Func<T, bool>>(Expression.Property(Expression.Convert(expression, typeof(IPackage)), (MethodInfo) methodof(IPackage.get_IsLatestVersion)), parameters));
        }

        public static IEnumerable<IPackageFile> GetBuildFiles(this IPackage package)
        {
            string targetsFile = package.Id + ".targets";
            string propsFile = package.Id + ".props";
            return (from p in package.GetFiles(Constants.BuildDirectory)
                where targetsFile.Equals(p.EffectivePath, StringComparison.OrdinalIgnoreCase) || propsFile.Equals(p.EffectivePath, StringComparison.OrdinalIgnoreCase)
                select p);
        }

        public static IEnumerable<PackageDependency> GetCompatiblePackageDependencies(this IPackageMetadata package, FrameworkName targetFramework)
        {
            IEnumerable<PackageDependencySet> dependencySets;
            if (targetFramework == null)
            {
                dependencySets = package.DependencySets;
            }
            else if (!VersionUtility.TryGetCompatibleItems<PackageDependencySet>(targetFramework, package.DependencySets, out dependencySets))
            {
                dependencySets = new PackageDependencySet[0];
            }
            return (from d in dependencySets select d.Dependencies);
        }

        public static IEnumerable<IPackageFile> GetContentFiles(this IPackage package) => 
            package.GetFiles(Constants.ContentDirectory);

        public static IEnumerable<IPackageFile> GetFiles(this IPackage package, string directory)
        {
            string folderPrefix = directory + Path.DirectorySeparatorChar.ToString();
            return (from file in package.GetFiles()
                where file.Path.StartsWith(folderPrefix, StringComparison.OrdinalIgnoreCase)
                select file);
        }

        public static string GetFullName(this IPackageName package) => 
            (package.Id + " " + package.Version);

        public static string GetHash(this IPackage package, IHashProvider hashProvider)
        {
            using (Stream stream = package.GetStream())
            {
                return Convert.ToBase64String(hashProvider.CalculateHash(stream));
            }
        }

        public static string GetHash(this IPackage package, string hashAlgorithm) => 
            package.GetHash(new CryptoHashProvider(hashAlgorithm));

        public static IEnumerable<IPackageFile> GetLibFiles(this IPackage package) => 
            package.GetFiles(Constants.LibDirectory);

        public static IEnumerable<IPackageFile> GetSatelliteFiles(this IPackage package) => 
            (!string.IsNullOrEmpty(package.Language) ? (from file in package.GetLibFiles()
                where Path.GetDirectoryName(file.Path).Split(new char[] { Path.DirectorySeparatorChar }).Contains<string>(package.Language, StringComparer.OrdinalIgnoreCase)
                select file) : Enumerable.Empty<IPackageFile>());

        public static IEnumerable<IPackageFile> GetToolFiles(this IPackage package) => 
            package.GetFiles(Constants.ToolsDirectory);

        public static bool HasFileWithNullTargetFramework(this IPackage package) => 
            Enumerable.Any<IPackageFile>(package.GetContentFiles().Concat<IPackageFile>(package.GetLibFiles()), file => file.TargetFramework == null);

        public static bool HasProjectContent(this IPackage package) => 
            (package.FrameworkAssemblies.Any<FrameworkAssemblyReference>() || (package.AssemblyReferences.Any<IPackageAssemblyReference>() || (package.GetContentFiles().Any<IPackageFile>() || (package.GetLibFiles().Any<IPackageFile>() || package.GetBuildFiles().Any<IPackageFile>()))));

        public static bool IsEmptyFolder(this IPackageFile packageFile) => 
            ((packageFile != null) && "_._".Equals(Path.GetFileName(packageFile.Path), StringComparison.OrdinalIgnoreCase));

        public static bool IsListed(this IPackage package)
        {
            if (package.Listed)
            {
                return true;
            }
            DateTimeOffset? published = package.Published;
            DateTimeOffset unpublished = Constants.Unpublished;
            return ((published != null) ? (published.GetValueOrDefault() > unpublished) : false);
        }

        public static bool IsReleaseVersion(this IPackageName packageMetadata) => 
            string.IsNullOrEmpty(packageMetadata.Version.SpecialVersion);

        public static bool IsSatellitePackage(this IPackageMetadata package)
        {
            if (string.IsNullOrEmpty(package.Language) || !package.Id.EndsWith("." + package.Language, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            string corePackageId = package.Id.Substring(0, (package.Id.Length - package.Language.Length) - 1);
            return Enumerable.Any<PackageDependency>(from s in package.DependencySets select s.Dependencies, d => d.Id.Equals(corePackageId, StringComparison.OrdinalIgnoreCase) && ((d.VersionSpec != null) && ((d.VersionSpec.MaxVersion == d.VersionSpec.MinVersion) && (d.VersionSpec.IsMaxInclusive && d.VersionSpec.IsMinInclusive))));
        }

        public static IEnumerable<PackageIssue> Validate(this IPackage package, IEnumerable<IPackageRule> rules)
        {
            if (package == null)
            {
                return null;
            }
            if (rules == null)
            {
                throw new ArgumentNullException("rules");
            }
            return (from r in rules
                where r != null
                select r.Validate(package));
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly PackageExtensions.<>c <>9 = new PackageExtensions.<>c();
            public static Func<PackageDependencySet, IEnumerable<PackageDependency>> <>9__4_0;
            public static Func<IPackageFile, bool> <>9__12_0;
            public static Func<IPackageRule, bool> <>9__14_0;
            public static Func<PackageDependencySet, IEnumerable<PackageDependency>> <>9__18_0;
            public static Func<IPackage, string> <>9__21_0;
            public static Func<IPackage, SemanticVersion> <>9__21_2;
            public static Func<IGrouping<string, IPackage>, IPackage> <>9__21_1;
            public static Func<IPackage, bool> <>9__22_0;

            internal string <CollapseById>b__21_0(IPackage p) => 
                p.Id;

            internal IPackage <CollapseById>b__21_1(IGrouping<string, IPackage> g) => 
                (from p in g
                    orderby p.Version descending
                    select p).First<IPackage>();

            internal SemanticVersion <CollapseById>b__21_2(IPackage p) => 
                p.Version;

            internal bool <FilterByPrerelease>b__22_0(IPackage p) => 
                p.IsReleaseVersion();

            internal IEnumerable<PackageDependency> <GetCompatiblePackageDependencies>b__18_0(PackageDependencySet d) => 
                d.Dependencies;

            internal bool <HasFileWithNullTargetFramework>b__12_0(IPackageFile file) => 
                (file.TargetFramework == null);

            internal IEnumerable<PackageDependency> <IsSatellitePackage>b__4_0(PackageDependencySet s) => 
                s.Dependencies;

            internal bool <Validate>b__14_0(IPackageRule r) => 
                (r != null);
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c__25<T> where T: IPackage
        {
            public static readonly PackageExtensions.<>c__25<T> <>9;
            public static Func<string, bool> <>9__25_0;

            static <>c__25()
            {
                PackageExtensions.<>c__25<T>.<>9 = new PackageExtensions.<>c__25<T>();
            }

            internal bool <Find>b__25_0(string s) => 
                (s != null);
        }
    }
}

