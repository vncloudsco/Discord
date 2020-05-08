namespace Squirrel
{
    using Mono.Cecil;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal static class SquirrelAwareExecutableDetector
    {
        public static List<string> GetAllSquirrelAwareApps(string directory, int minimumVersion = 1) => 
            Enumerable.Where<string>(from x in new DirectoryInfo(directory).EnumerateFiles()
                where x.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                select x.FullName, delegate (string x) {
                int? pESquirrelAwareVersion = GetPESquirrelAwareVersion(x);
                return ((pESquirrelAwareVersion != null) ? pESquirrelAwareVersion.GetValueOrDefault() : -1) >= minimumVersion;
            }).ToList<string>();

        private static int? GetAssemblySquirrelAwareVersion(string executable)
        {
            int? nullable;
            int? nullable2;
            try
            {
                AssemblyDefinition definition = AssemblyDefinition.ReadAssembly(executable);
                if (!definition.HasCustomAttributes)
                {
                    nullable = null;
                    nullable = nullable;
                }
                else
                {
                    CustomAttribute attribute = Enumerable.FirstOrDefault<CustomAttribute>(definition.CustomAttributes, x => (x.AttributeType.FullName == typeof(AssemblyMetadataAttribute).FullName) ? ((x.ConstructorArguments.Count == 2) ? (x.ConstructorArguments[0].Value.ToString() == "SquirrelAwareVersion") : false) : false);
                    if (attribute == null)
                    {
                        nullable2 = null;
                        nullable = nullable2;
                    }
                    else
                    {
                        int num;
                        if (int.TryParse(attribute.ConstructorArguments[1].Value.ToString(), NumberStyles.Integer, CultureInfo.CurrentCulture, out num))
                        {
                            nullable = new int?(num);
                        }
                        else
                        {
                            nullable2 = null;
                            nullable = nullable2;
                        }
                    }
                }
            }
            catch (FileLoadException)
            {
                nullable2 = null;
                nullable = nullable2;
            }
            catch (BadImageFormatException)
            {
                nullable2 = null;
                nullable = nullable2;
            }
            return nullable;
        }

        public static int? GetPESquirrelAwareVersion(string executable)
        {
            if (!File.Exists(executable))
            {
                return null;
            }
            string fullname = Path.GetFullPath(executable);
            return delegate {
                int? assemblySquirrelAwareVersion = GetAssemblySquirrelAwareVersion(fullname);
                return ((assemblySquirrelAwareVersion != null) ? assemblySquirrelAwareVersion : GetVersionBlockSquirrelAwareValue(fullname));
            }.Retry<int?>(2);
        }

        private static int? GetVersionBlockSquirrelAwareValue(string executable)
        {
            int fileVersionInfoSize = NativeMethods.GetFileVersionInfoSize(executable, IntPtr.Zero);
            if ((fileVersionInfoSize > 0) && (fileVersionInfoSize <= 0x1000))
            {
                IntPtr ptr;
                int num2;
                byte[] lpData = new byte[fileVersionInfoSize];
                if (!NativeMethods.GetFileVersionInfo(executable, IntPtr.Zero, fileVersionInfoSize, lpData))
                {
                    return null;
                }
                if (NativeMethods.VerQueryValue(lpData, @"\StringFileInfo\040904B0\SquirrelAwareVersion", out ptr, out num2))
                {
                    return 1;
                }
            }
            return null;
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly SquirrelAwareExecutableDetector.<>c <>9 = new SquirrelAwareExecutableDetector.<>c();
            public static Func<FileInfo, bool> <>9__0_0;
            public static Func<FileInfo, string> <>9__0_1;
            public static Func<CustomAttribute, bool> <>9__2_0;

            internal bool <GetAllSquirrelAwareApps>b__0_0(FileInfo x) => 
                x.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);

            internal string <GetAllSquirrelAwareApps>b__0_1(FileInfo x) => 
                x.FullName;

            internal bool <GetAssemblySquirrelAwareVersion>b__2_0(CustomAttribute x) => 
                ((x.AttributeType.FullName == typeof(AssemblyMetadataAttribute).FullName) ? ((x.ConstructorArguments.Count == 2) ? (x.ConstructorArguments[0].Value.ToString() == "SquirrelAwareVersion") : false) : false);
        }
    }
}

