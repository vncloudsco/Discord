namespace ICSharpCode.SharpZipLib.Core
{
    using System;

    internal class ProgressEventArgs : EventArgs
    {
        private string name_;
        private long processed_;
        private long target_;
        private bool continueRunning_ = true;

        public ProgressEventArgs(string name, long processed, long target)
        {
            this.name_ = name;
            this.processed_ = processed;
            this.target_ = target;
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

        public float PercentComplete =>
            ((this.target_ > 0L) ? ((((float) this.processed_) / ((float) this.target_)) * 100f) : 0f);

        public long Processed =>
            this.processed_;

        public long Target =>
            this.target_;
    }
}

