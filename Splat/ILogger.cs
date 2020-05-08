namespace Splat
{
    using System;
    using System.ComponentModel;

    internal interface ILogger
    {
        void Write([Localizable(false)] string message, LogLevel logLevel);

        LogLevel Level { get; set; }
    }
}

