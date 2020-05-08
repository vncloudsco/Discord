namespace NuGet
{
    using NuGet.Analysis.Rules;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal static class DefaultPackageRuleSet
    {
        private static readonly ReadOnlyCollection<IPackageRule> _rules;

        static DefaultPackageRuleSet()
        {
            IPackageRule[] list = new IPackageRule[] { new InvalidFrameworkFolderRule(), new MisplacedAssemblyRule(), new MisplacedScriptFileRule(), new MisplacedTransformFileRule(), new MissingSummaryRule(), new InitScriptNotUnderToolsRule(), new WinRTNameIsObsoleteRule() };
            _rules = new ReadOnlyCollection<IPackageRule>(list);
        }

        public static IEnumerable<IPackageRule> Rules =>
            _rules;
    }
}

