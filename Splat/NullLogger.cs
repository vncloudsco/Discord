namespace Splat
{
    using System;
    using System.Runtime.CompilerServices;

    internal class NullLogger : ILogger
    {
        public void Write(string message, LogLevel logLevel)
        {
        }

        public LogLevel Level { get; set; }
    }
}

