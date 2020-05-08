namespace NuGet.Analysis.Rules
{
    using NuGet;
    using NuGet.Resources;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal class MisplacedAssemblyRule : IPackageRule
    {
        private static PackageIssue CreatePackageIssueForAssembliesOutsideLib(string target)
        {
            object[] args = new object[] { target };
            return new PackageIssue(AnalysisResources.AssemblyOutsideLibTitle, string.Format(CultureInfo.CurrentCulture, AnalysisResources.AssemblyOutsideLibDescription, args), AnalysisResources.AssemblyOutsideLibSolution);
        }

        private static PackageIssue CreatePackageIssueForAssembliesUnderLib(string target)
        {
            object[] args = new object[] { target };
            return new PackageIssue(AnalysisResources.AssemblyUnderLibTitle, string.Format(CultureInfo.CurrentCulture, AnalysisResources.AssemblyUnderLibDescription, args), AnalysisResources.AssemblyUnderLibSolution);
        }

        [IteratorStateMachine(typeof(<Validate>d__0))]
        public IEnumerable<PackageIssue> Validate(IPackage package)
        {
            IEnumerator<IPackageFile> enumerator = package.GetFiles().GetEnumerator();
            while (true)
            {
                if (!enumerator.MoveNext())
                {
                    enumerator = null;
                    yield break;
                    break;
                }
                IPackageFile current = enumerator.Current;
                string path = current.Path;
                string directoryName = Path.GetDirectoryName(path);
                if (directoryName.Equals(Constants.LibDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    if (PackageHelper.IsAssembly(path))
                    {
                        yield return CreatePackageIssueForAssembliesUnderLib(path);
                        yield break;
                        break;
                    }
                }
                else if (!directoryName.StartsWith(Constants.LibDirectory + Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase) && (path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".winmd", StringComparison.OrdinalIgnoreCase)))
                {
                    yield return CreatePackageIssueForAssembliesOutsideLib(path);
                    yield break;
                    break;
                }
                path = null;
                directoryName = null;
            }
        }

    }
}

