namespace NuGet
{
    using System;
    using System.Runtime.CompilerServices;

    internal interface IProgressProvider
    {
        event EventHandler<ProgressEventArgs> ProgressAvailable;
    }
}

