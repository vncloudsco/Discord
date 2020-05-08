namespace NuGet.Resources
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.CompilerServices;

    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), DebuggerNonUserCode, CompilerGenerated]
    internal class AnalysisResources
    {
        private static System.Resources.ResourceManager resourceMan;
        private static CultureInfo resourceCulture;

        internal AnalysisResources()
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (resourceMan == null)
                {
                    resourceMan = new System.Resources.ResourceManager("NuGet.Resources.AnalysisResources", typeof(AnalysisResources).Assembly);
                }
                return resourceMan;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            get => 
                resourceCulture;
            set => 
                (resourceCulture = value);
        }

        internal static string AssemblyOutsideLibDescription =>
            ResourceManager.GetString("AssemblyOutsideLibDescription", resourceCulture);

        internal static string AssemblyOutsideLibSolution =>
            ResourceManager.GetString("AssemblyOutsideLibSolution", resourceCulture);

        internal static string AssemblyOutsideLibTitle =>
            ResourceManager.GetString("AssemblyOutsideLibTitle", resourceCulture);

        internal static string AssemblyUnderLibDescription =>
            ResourceManager.GetString("AssemblyUnderLibDescription", resourceCulture);

        internal static string AssemblyUnderLibSolution =>
            ResourceManager.GetString("AssemblyUnderLibSolution", resourceCulture);

        internal static string AssemblyUnderLibTitle =>
            ResourceManager.GetString("AssemblyUnderLibTitle", resourceCulture);

        internal static string InvalidFrameworkDescription =>
            ResourceManager.GetString("InvalidFrameworkDescription", resourceCulture);

        internal static string InvalidFrameworkSolution =>
            ResourceManager.GetString("InvalidFrameworkSolution", resourceCulture);

        internal static string InvalidFrameworkTitle =>
            ResourceManager.GetString("InvalidFrameworkTitle", resourceCulture);

        internal static string MisplacedInitScriptDescription =>
            ResourceManager.GetString("MisplacedInitScriptDescription", resourceCulture);

        internal static string MisplacedInitScriptSolution =>
            ResourceManager.GetString("MisplacedInitScriptSolution", resourceCulture);

        internal static string MisplacedInitScriptTitle =>
            ResourceManager.GetString("MisplacedInitScriptTitle", resourceCulture);

        internal static string MisplacedTransformFileDescription =>
            ResourceManager.GetString("MisplacedTransformFileDescription", resourceCulture);

        internal static string MisplacedTransformFileSolution =>
            ResourceManager.GetString("MisplacedTransformFileSolution", resourceCulture);

        internal static string MisplacedTransformFileTitle =>
            ResourceManager.GetString("MisplacedTransformFileTitle", resourceCulture);

        internal static string MissingSummaryDescription =>
            ResourceManager.GetString("MissingSummaryDescription", resourceCulture);

        internal static string MissingSummarySolution =>
            ResourceManager.GetString("MissingSummarySolution", resourceCulture);

        internal static string MissingSummaryTitle =>
            ResourceManager.GetString("MissingSummaryTitle", resourceCulture);

        internal static string ScriptOutsideToolsDescription =>
            ResourceManager.GetString("ScriptOutsideToolsDescription", resourceCulture);

        internal static string ScriptOutsideToolsSolution =>
            ResourceManager.GetString("ScriptOutsideToolsSolution", resourceCulture);

        internal static string ScriptOutsideToolsTitle =>
            ResourceManager.GetString("ScriptOutsideToolsTitle", resourceCulture);

        internal static string UnrecognizedScriptDescription =>
            ResourceManager.GetString("UnrecognizedScriptDescription", resourceCulture);

        internal static string UnrecognizedScriptSolution =>
            ResourceManager.GetString("UnrecognizedScriptSolution", resourceCulture);

        internal static string UnrecognizedScriptTitle =>
            ResourceManager.GetString("UnrecognizedScriptTitle", resourceCulture);

        internal static string WinRTObsoleteDescription =>
            ResourceManager.GetString("WinRTObsoleteDescription", resourceCulture);

        internal static string WinRTObsoleteSolution =>
            ResourceManager.GetString("WinRTObsoleteSolution", resourceCulture);

        internal static string WinRTObsoleteTitle =>
            ResourceManager.GetString("WinRTObsoleteTitle", resourceCulture);
    }
}

