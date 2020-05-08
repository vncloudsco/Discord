namespace NuGet.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal static class AssemblyNameExtensions
    {
        public static string GetPublicKeyTokenString(this AssemblyName assemblyName) => 
            string.Join(string.Empty, (IEnumerable<string>) (from b in assemblyName.GetPublicKeyToken() select b.ToString("x2", CultureInfo.InvariantCulture)));

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly AssemblyNameExtensions.<>c <>9 = new AssemblyNameExtensions.<>c();
            public static Func<byte, string> <>9__0_0;

            internal string <GetPublicKeyTokenString>b__0_0(byte b) => 
                b.ToString("x2", CultureInfo.InvariantCulture);
        }
    }
}

