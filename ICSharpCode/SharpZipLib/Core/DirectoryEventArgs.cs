namespace ICSharpCode.SharpZipLib.Core
{
    using System;

    internal class DirectoryEventArgs : ScanEventArgs
    {
        private bool hasMatchingFiles_;

        public DirectoryEventArgs(string name, bool hasMatchingFiles) : base(name)
        {
            this.hasMatchingFiles_ = hasMatchingFiles;
        }

        public bool HasMatchingFiles =>
            this.hasMatchingFiles_;
    }
}

