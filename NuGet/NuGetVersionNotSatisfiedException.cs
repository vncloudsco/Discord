namespace NuGet
{
    using System;

    [Serializable]
    internal class NuGetVersionNotSatisfiedException : Exception
    {
        public NuGetVersionNotSatisfiedException()
        {
        }

        public NuGetVersionNotSatisfiedException(string message) : base(message)
        {
        }

        public NuGetVersionNotSatisfiedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

