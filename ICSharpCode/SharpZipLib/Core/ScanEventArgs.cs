namespace ICSharpCode.SharpZipLib.Core
{
    using System;

    internal class ScanEventArgs : EventArgs
    {
        private string name_;
        private bool continueRunning_ = true;

        public ScanEventArgs(string name)
        {
            this.name_ = name;
        }

        public string Name =>
            this.name_;

        public bool ContinueRunning
        {
            get => 
                this.continueRunning_;
            set => 
                (this.continueRunning_ = value);
        }
    }
}

