namespace ICSharpCode.SharpZipLib.Core
{
    using System;

    internal class ScanFailureEventArgs : EventArgs
    {
        private string name_;
        private System.Exception exception_;
        private bool continueRunning_;

        public ScanFailureEventArgs(string name, System.Exception e)
        {
            this.name_ = name;
            this.exception_ = e;
            this.continueRunning_ = true;
        }

        public string Name =>
            this.name_;

        public System.Exception Exception =>
            this.exception_;

        public bool ContinueRunning
        {
            get => 
                this.continueRunning_;
            set => 
                (this.continueRunning_ = value);
        }
    }
}

