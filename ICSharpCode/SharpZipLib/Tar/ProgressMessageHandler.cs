namespace ICSharpCode.SharpZipLib.Tar
{
    using System;
    using System.Runtime.CompilerServices;

    internal delegate void ProgressMessageHandler(TarArchive archive, TarEntry entry, string message);
}

