namespace Splat
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    internal class PlatformModeDetector : IModeDetector
    {
        public bool? InDesignMode()
        {
            string[] strArray = new string[] { "BLEND.EXE", "XDESPROC.EXE" };
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                string exeName = new FileInfo(entryAssembly.Location).Name.ToUpperInvariant();
                if (Enumerable.Any<string>(strArray, x => x.Contains(exeName)))
                {
                    return true;
                }
            }
            return false;
        }

        public bool? InUnitTestRunner()
        {
            string[] assemblyList = new string[] { "CSUNIT", "NUNIT", "XUNIT", "MBUNIT", "NBEHAVE" };
            try
            {
                return new bool?(searchForAssembly(assemblyList));
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool searchForAssembly(IEnumerable<string> assemblyList) => 
            Enumerable.Any<Assembly>(AppDomain.CurrentDomain.GetAssemblies(), x => Enumerable.Any<string>(assemblyList, name => x.FullName.ToUpperInvariant().Contains(name)));
    }
}

