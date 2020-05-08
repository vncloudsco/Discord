namespace NuGet
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal class UnzippedPackageRepository : PackageRepositoryBase, IPackageLookup, IPackageRepository
    {
        public UnzippedPackageRepository(string physicalPath) : this(new DefaultPackagePathResolver(physicalPath), new PhysicalFileSystem(physicalPath))
        {
        }

        public UnzippedPackageRepository(IPackagePathResolver pathResolver, IFileSystem fileSystem)
        {
            this.FileSystem = fileSystem;
            this.PathResolver = pathResolver;
        }

        public bool Exists(string packageId, SemanticVersion version)
        {
            string packageFileName = GetPackageFileName(packageId, version);
            string path = packageFileName + Constants.PackageExtension;
            return (this.FileSystem.FileExists(path) && this.FileSystem.DirectoryExists(packageFileName));
        }

        public IPackage FindPackage(string packageId, SemanticVersion version)
        {
            string packageFileName = GetPackageFileName(packageId, version);
            return (!this.Exists(packageId, version) ? null : new UnzippedPackage(this.FileSystem, packageFileName));
        }

        public IEnumerable<IPackage> FindPackagesById(string packageId)
        {
            <>c__DisplayClass16_0 class_;
            ParameterExpression expression = Expression.Parameter(typeof(IPackage), "p");
            Expression[] arguments = new Expression[] { Expression.Field(Expression.Constant(class_, typeof(<>c__DisplayClass16_0)), fieldof(<>c__DisplayClass16_0.packageId)), Expression.Constant(StringComparison.OrdinalIgnoreCase, typeof(StringComparison)) };
            ParameterExpression[] parameters = new ParameterExpression[] { expression };
            return Queryable.Where<IPackage>(this.GetPackages(), Expression.Lambda<Func<IPackage, bool>>(Expression.Call(Expression.Property(expression, (MethodInfo) methodof(IPackageName.get_Id)), (MethodInfo) methodof(string.Equals), arguments), parameters));
        }

        private static string GetPackageFileName(string packageId, SemanticVersion version) => 
            (packageId + "." + version.ToString());

        public override IQueryable<IPackage> GetPackages() => 
            ((IQueryable<IPackage>) (from file in this.FileSystem.GetFiles("", "*" + Constants.PackageExtension)
                let packageName = Path.GetFileNameWithoutExtension(file)
                where this.FileSystem.DirectoryExists(packageName)
                select new UnzippedPackage(this.FileSystem, packageName)).AsQueryable<UnzippedPackage>());

        protected IFileSystem FileSystem { get; private set; }

        internal IPackagePathResolver PathResolver { get; set; }

        public override string Source =>
            this.FileSystem.Root;

        public override bool SupportsPrereleasePackages =>
            true;

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly UnzippedPackageRepository.<>c <>9 = new UnzippedPackageRepository.<>c();
            public static Func<string, <>f__AnonymousType5<string, string>> <>9__14_0;

            internal <>f__AnonymousType5<string, string> <GetPackages>b__14_0(string file) => 
                new { 
                    file = file,
                    packageName = Path.GetFileNameWithoutExtension(file)
                };
        }
    }
}

