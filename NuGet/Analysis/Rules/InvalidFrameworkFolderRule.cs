namespace NuGet.Analysis.Rules
{
    using NuGet;
    using NuGet.Resources;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.Versioning;

    internal class InvalidFrameworkFolderRule : IPackageRule
    {
        private PackageIssue CreatePackageIssue(string target)
        {
            object[] args = new object[] { target };
            return new PackageIssue(AnalysisResources.InvalidFrameworkTitle, string.Format(CultureInfo.CurrentCulture, AnalysisResources.InvalidFrameworkDescription, args), AnalysisResources.InvalidFrameworkSolution);
        }

        private static bool IsValidCultureName(IPackage package, string name) => 
            (!string.IsNullOrEmpty(package.Language) ? name.Equals(package.Language, StringComparison.OrdinalIgnoreCase) : false);

        private static bool IsValidFrameworkName(string name)
        {
            FrameworkName unsupportedFrameworkName;
            try
            {
                unsupportedFrameworkName = VersionUtility.ParseFrameworkName(name);
            }
            catch (ArgumentException)
            {
                unsupportedFrameworkName = VersionUtility.UnsupportedFrameworkName;
            }
            return (unsupportedFrameworkName != VersionUtility.UnsupportedFrameworkName);
        }

        public IEnumerable<PackageIssue> Validate(IPackage package)
        {
            HashSet<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (IEnumerator<IPackageFile> enumerator = package.GetFiles().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    char[] separator = new char[] { Path.DirectorySeparatorChar };
                    string[] strArray = enumerator.Current.Path.Split(separator);
                    if ((strArray.Length >= 3) && strArray[0].Equals(Constants.LibDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        set.Add(strArray[1]);
                    }
                }
            }
            return Enumerable.Select<string, PackageIssue>(from s in set
                where !IsValidFrameworkName(s) && !IsValidCultureName(package, s)
                select s, new Func<string, PackageIssue>(this.CreatePackageIssue));
        }
    }
}

