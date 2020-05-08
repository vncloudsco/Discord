namespace NuGet
{
    using System;

    internal class NullLogger : ILogger, IFileConflictResolver
    {
        private static readonly ILogger _instance = new NullLogger();

        public void Log(MessageLevel level, string message, params object[] args)
        {
        }

        public FileConflictResolution ResolveFileConflict(string message) => 
            FileConflictResolution.Ignore;

        public static ILogger Instance =>
            _instance;
    }
}

