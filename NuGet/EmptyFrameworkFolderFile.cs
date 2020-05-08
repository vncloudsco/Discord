namespace NuGet
{
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    internal sealed class EmptyFrameworkFolderFile : PhysicalPackageFile
    {
        public EmptyFrameworkFolderFile(string directoryPathInPackage) : this(() => Stream.Null)
        {
            if (directoryPathInPackage == null)
            {
                throw new ArgumentNullException("directoryPathInPackage");
            }
            base.TargetPath = Path.Combine(directoryPathInPackage, "_._");
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly EmptyFrameworkFolderFile.<>c <>9 = new EmptyFrameworkFolderFile.<>c();
            public static Func<Stream> <>9__0_0;

            internal Stream <.ctor>b__0_0() => 
                Stream.Null;
        }
    }
}

