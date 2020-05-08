namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;

    internal sealed class FileTransformExtensions : IEquatable<FileTransformExtensions>
    {
        public FileTransformExtensions(string installExtension, string uninstallExtension)
        {
            if (string.IsNullOrEmpty(installExtension))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "installExtension");
            }
            if (string.IsNullOrEmpty(uninstallExtension))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, "uninstallExtension");
            }
            this.InstallExtension = installExtension;
            this.UninstallExtension = uninstallExtension;
        }

        public bool Equals(FileTransformExtensions other) => 
            (string.Equals(this.InstallExtension, other.InstallExtension, StringComparison.OrdinalIgnoreCase) && string.Equals(this.UninstallExtension, other.UninstallExtension, StringComparison.OrdinalIgnoreCase));

        public override int GetHashCode() => 
            ((this.InstallExtension.GetHashCode() * 0xc41) + this.UninstallExtension.GetHashCode());

        public string InstallExtension { get; private set; }

        public string UninstallExtension { get; private set; }
    }
}

