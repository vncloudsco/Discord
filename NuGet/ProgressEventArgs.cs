namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;

    internal class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(int percentComplete) : this(null, percentComplete)
        {
        }

        public ProgressEventArgs(string operation, int percentComplete)
        {
            this.Operation = operation;
            this.PercentComplete = percentComplete;
        }

        public string Operation { get; private set; }

        public int PercentComplete { get; private set; }
    }
}

