namespace DeltaCompressionDotNet.MsDelta
{
    using DeltaCompressionDotNet;
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    internal sealed class MsDeltaCompression : IDeltaCompression
    {
        public void ApplyDelta(string deltaFilePath, string oldFilePath, string newFilePath)
        {
            if (!NativeMethods.ApplyDelta(ApplyFlags.AllowLegacy, oldFilePath, deltaFilePath, newFilePath))
            {
                throw new Win32Exception();
            }
        }

        public void CreateDelta(string oldFilePath, string newFilePath, string deltaFilePath)
        {
            DeltaInput globalOptions = new DeltaInput();
            if (!NativeMethods.CreateDelta(FileTypeSet.Executables, 0L, 0L, oldFilePath, newFilePath, null, null, globalOptions, IntPtr.Zero, HashAlgId.Crc32, deltaFilePath))
            {
                throw new Win32Exception();
            }
        }
    }
}

