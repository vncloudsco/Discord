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

    internal class MisplacedScriptFileRule : IPackageRule
    {
        private const string ScriptExtension = ".ps1";

        private static PackageIssue CreatePackageIssueForMisplacedScript(string target)
        {
            object[] args = new object[] { target };
            return new PackageIssue(AnalysisResources.ScriptOutsideToolsTitle, string.Format(CultureInfo.CurrentCulture, AnalysisResources.ScriptOutsideToolsDescription, args), AnalysisResources.ScriptOutsideToolsSolution);
        }

        private static PackageIssue CreatePackageIssueForUnrecognizedScripts(string target)
        {
            object[] args = new object[] { target };
            return new PackageIssue(AnalysisResources.UnrecognizedScriptTitle, string.Format(CultureInfo.CurrentCulture, AnalysisResources.UnrecognizedScriptDescription, args), AnalysisResources.UnrecognizedScriptSolution);
        }

        [IteratorStateMachine(typeof(<Validate>d__1))]
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
                if (path.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase))
                {
                    if (!path.StartsWith(Constants.ToolsDirectory + Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        yield return CreatePackageIssueForMisplacedScript(path);
                        yield break;
                        break;
                    }
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
                    if (!fileNameWithoutExtension.Equals("install", StringComparison.OrdinalIgnoreCase) && (!fileNameWithoutExtension.Equals("uninstall", StringComparison.OrdinalIgnoreCase) && !fileNameWithoutExtension.Equals("init", StringComparison.OrdinalIgnoreCase)))
                    {
                        yield return CreatePackageIssueForUnrecognizedScripts(path);
                        yield break;
                        break;
                    }
                    path = null;
                    continue;
                }
            }
        }

    }
}

