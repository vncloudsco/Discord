namespace NuGet
{
    using Microsoft.Win32;
    using System;

    internal static class EnvironmentUtility
    {
        private static bool _runningFromCommandLine;
        private static readonly bool _isMonoRuntime = (Type.GetType("Mono.Runtime") != null);

        public static void SetRunningFromCommandLine()
        {
            _runningFromCommandLine = true;
        }

        public static bool IsMonoRuntime =>
            _isMonoRuntime;

        public static bool RunningFromCommandLine =>
            _runningFromCommandLine;

        public static bool IsNet45Installed
        {
            get
            {
                bool flag;
                using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                {
                    using (RegistryKey key2 = key.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"))
                    {
                        if (key2 == null)
                        {
                            flag = false;
                        }
                        else
                        {
                            object obj2 = key2.GetValue("Release");
                            flag = (obj2 is int) && (((int) obj2) >= 0x5c615);
                        }
                    }
                }
                return flag;
            }
        }
    }
}

