namespace Splat
{
    using System;
    using System.Runtime.CompilerServices;

    internal class DebugLogger : ILogger
    {
        public void Write(string message, LogLevel logLevel)
        {
            LogLevel level = this.Level;
            LogLevel level2 = logLevel;
        }

        public LogLevel Level { get; set; }
    }
}

