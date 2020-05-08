namespace Splat
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;

    internal class WrappingFullLogger : IFullLogger, ILogger
    {
        private readonly ILogger _inner;
        private readonly string prefix;
        private readonly MethodInfo stringFormat;

        public WrappingFullLogger(ILogger inner, Type callingType)
        {
            this._inner = inner;
            object[] args = new object[] { callingType.Name };
            this.prefix = string.Format(CultureInfo.InvariantCulture, "{0}: ", args);
            Type[] types = new Type[] { typeof(IFormatProvider), typeof(string), typeof(object[]) };
            this.stringFormat = typeof(string).GetMethod("Format", types);
        }

        public void Debug(string message)
        {
            this._inner.Write(this.prefix + message, LogLevel.Debug);
        }

        public void Debug<T>(T value)
        {
            this._inner.Write(this.prefix + value, LogLevel.Debug);
        }

        public void Debug<T>(IFormatProvider formatProvider, T value)
        {
            object[] args = new object[] { this.prefix, value };
            this._inner.Write(string.Format(formatProvider, "{0}{1}", args), LogLevel.Debug);
        }

        public void Debug(string message, params object[] args)
        {
            this._inner.Write(this.prefix + this.InvokeStringFormat(CultureInfo.InvariantCulture, message, args), LogLevel.Debug);
        }

        public void Debug<TArgument>(string message, TArgument argument)
        {
            object[] args = new object[] { argument };
            this._inner.Write(this.prefix + string.Format(CultureInfo.InvariantCulture, message, args), LogLevel.Debug);
        }

        public void Debug(IFormatProvider formatProvider, string message, params object[] args)
        {
            this._inner.Write(this.prefix + this.InvokeStringFormat(formatProvider, message, args), LogLevel.Debug);
        }

        public void Debug<TArgument>(IFormatProvider formatProvider, string message, TArgument argument)
        {
            object[] args = new object[] { argument };
            this._inner.Write(this.prefix + string.Format(formatProvider, message, args), LogLevel.Debug);
        }

        public void Debug<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2)
        {
            object[] args = new object[] { argument1, argument2 };
            this._inner.Write(this.prefix + string.Format(CultureInfo.InvariantCulture, message, args), LogLevel.Debug);
        }

        public void Debug<TArgument1, TArgument2>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2)
        {
            object[] args = new object[] { argument1, argument2 };
            this._inner.Write(this.prefix + string.Format(formatProvider, message, args), LogLevel.Debug);
        }

        public void Debug<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            object[] args = new object[] { argument1, argument2, argument3 };
            this._inner.Write(this.prefix + string.Format(CultureInfo.InvariantCulture, message, args), LogLevel.Debug);
        }

        public void Debug<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            object[] args = new object[] { argument1, argument2, argument3 };
            this._inner.Write(this.prefix + string.Format(formatProvider, message, args), LogLevel.Debug);
        }

        public void DebugException(string message, Exception exception)
        {
            this._inner.Write($"{this.prefix}{message}: {exception}", LogLevel.Debug);
        }

        public void Error(string message)
        {
            this._inner.Write(this.prefix + message, LogLevel.Error);
        }

        public void Error<T>(T value)
        {
            this._inner.Write(this.prefix + value, LogLevel.Error);
        }

        public void Error<T>(IFormatProvider formatProvider, T value)
        {
            object[] args = new object[] { this.prefix, value };
            this._inner.Write(string.Format(formatProvider, "{0}{1}", args), LogLevel.Error);
        }

        public void Error(string message, params object[] args)
        {
            this._inner.Write(this.prefix + this.InvokeStringFormat(CultureInfo.InvariantCulture, message, args), LogLevel.Error);
        }

        public void Error<TArgument>(string message, TArgument argument)
        {
            object[] args = new object[] { argument };
            this._inner.Write(this.prefix + string.Format(CultureInfo.InvariantCulture, message, args), LogLevel.Error);
        }

        public void Error(IFormatProvider formatProvider, string message, params object[] args)
        {
            this._inner.Write(this.prefix + this.InvokeStringFormat(formatProvider, message, args), LogLevel.Error);
        }

        public void Error<TArgument>(IFormatProvider formatProvider, string message, TArgument argument)
        {
            object[] args = new object[] { argument };
            this._inner.Write(this.prefix + string.Format(formatProvider, message, args), LogLevel.Error);
        }

        public void Error<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2)
        {
            object[] args = new object[] { argument1, argument2 };
            this._inner.Write(this.prefix + string.Format(CultureInfo.InvariantCulture, message, args), LogLevel.Error);
        }

        public void Error<TArgument1, TArgument2>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2)
        {
            object[] args = new object[] { argument1, argument2 };
            this._inner.Write(this.prefix + string.Format(formatProvider, message, args), LogLevel.Error);
        }

        public void Error<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            object[] args = new object[] { argument1, argument2, argument3 };
            this._inner.Write(this.prefix + string.Format(CultureInfo.InvariantCulture, message, args), LogLevel.Error);
        }

        public void Error<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            object[] args = new object[] { argument1, argument2, argument3 };
            this._inner.Write(this.prefix + string.Format(formatProvider, message, args), LogLevel.Error);
        }

        public void ErrorException(string message, Exception exception)
        {
            this._inner.Write($"{this.prefix}{message}: {exception}", LogLevel.Error);
        }

        public void Fatal(string message)
        {
            this._inner.Write(this.prefix + message, LogLevel.Fatal);
        }

        public void Fatal<T>(T value)
        {
            this._inner.Write(this.prefix + value, LogLevel.Fatal);
        }

        public void Fatal<T>(IFormatProvider formatProvider, T value)
        {
            object[] args = new object[] { this.prefix, value };
            this._inner.Write(string.Format(formatProvider, "{0}{1}", args), LogLevel.Fatal);
        }

        public void Fatal(string message, params object[] args)
        {
            this._inner.Write(this.prefix + this.InvokeStringFormat(CultureInfo.InvariantCulture, message, args), LogLevel.Fatal);
        }

        public void Fatal<TArgument>(string message, TArgument argument)
        {
            object[] args = new object[] { argument };
            this._inner.Write(this.prefix + string.Format(CultureInfo.InvariantCulture, message, args), LogLevel.Fatal);
        }

        public void Fatal(IFormatProvider formatProvider, string message, params object[] args)
        {
            this._inner.Write(this.prefix + this.InvokeStringFormat(formatProvider, message, args), LogLevel.Fatal);
        }

        public void Fatal<TArgument>(IFormatProvider formatProvider, string message, TArgument argument)
        {
            object[] args = new object[] { argument };
            this._inner.Write(this.prefix + string.Format(formatProvider, message, args), LogLevel.Fatal);
        }

        public void Fatal<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2)
        {
            object[] args = new object[] { argument1, argument2 };
            this._inner.Write(this.prefix + string.Format(CultureInfo.InvariantCulture, message, args), LogLevel.Fatal);
        }

        public void Fatal<TArgument1, TArgument2>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2)
        {
            object[] args = new object[] { argument1, argument2 };
            this._inner.Write(this.prefix + string.Format(formatProvider, message, args), LogLevel.Fatal);
        }

        public void Fatal<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            object[] args = new object[] { argument1, argument2, argument3 };
            this._inner.Write(this.prefix + string.Format(CultureInfo.InvariantCulture, message, args), LogLevel.Fatal);
        }

        public void Fatal<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            object[] args = new object[] { argument1, argument2, argument3 };
            this._inner.Write(this.prefix + string.Format(formatProvider, message, args), LogLevel.Fatal);
        }

        public void FatalException(string message, Exception exception)
        {
            this._inner.Write($"{this.prefix}{message}: {exception}", LogLevel.Fatal);
        }

        public void Info(string message)
        {
            this._inner.Write(this.prefix + message, LogLevel.Info);
        }

        public void Info<T>(T value)
        {
            this._inner.Write(this.prefix + value, LogLevel.Info);
        }

        public void Info<T>(IFormatProvider formatProvider, T value)
        {
            object[] args = new object[] { this.prefix, value };
            this._inner.Write(string.Format(formatProvider, "{0}{1}", args), LogLevel.Info);
        }

        public void Info(string message, params object[] args)
        {
            this._inner.Write(this.prefix + this.InvokeStringFormat(CultureInfo.InvariantCulture, message, args), LogLevel.Info);
        }

        public void Info<TArgument>(string message, TArgument argument)
        {
            object[] args = new object[] { argument };
            this._inner.Write(this.prefix + string.Format(CultureInfo.InvariantCulture, message, args), LogLevel.Info);
        }

        public void Info(IFormatProvider formatProvider, string message, params object[] args)
        {
            this._inner.Write(this.prefix + this.InvokeStringFormat(formatProvider, message, args), LogLevel.Info);
        }

        public void Info<TArgument>(IFormatProvider formatProvider, string message, TArgument argument)
        {
            object[] args = new object[] { argument };
            this._inner.Write(this.prefix + string.Format(formatProvider, message, args), LogLevel.Info);
        }

        public void Info<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2)
        {
            object[] args = new object[] { argument1, argument2 };
            this._inner.Write(this.prefix + string.Format(CultureInfo.InvariantCulture, message, args), LogLevel.Info);
        }

        public void Info<TArgument1, TArgument2>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2)
        {
            object[] args = new object[] { argument1, argument2 };
            this._inner.Write(this.prefix + string.Format(formatProvider, message, args), LogLevel.Info);
        }

        public void Info<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            object[] args = new object[] { argument1, argument2, argument3 };
            this._inner.Write(this.prefix + string.Format(CultureInfo.InvariantCulture, message, args), LogLevel.Info);
        }

        public void Info<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            object[] args = new object[] { argument1, argument2, argument3 };
            this._inner.Write(this.prefix + string.Format(formatProvider, message, args), LogLevel.Info);
        }

        public void InfoException(string message, Exception exception)
        {
            this._inner.Write($"{this.prefix}{message}: {exception}", LogLevel.Info);
        }

        private string InvokeStringFormat(IFormatProvider formatProvider, string message, object[] args)
        {
            object[] parameters = new object[] { formatProvider, message, args };
            return (string) this.stringFormat.Invoke(null, parameters);
        }

        public void Warn(string message)
        {
            this._inner.Write(this.prefix + message, LogLevel.Warn);
        }

        public void Warn<T>(T value)
        {
            this._inner.Write(this.prefix + value, LogLevel.Warn);
        }

        public void Warn<T>(IFormatProvider formatProvider, T value)
        {
            object[] args = new object[] { this.prefix, value };
            this._inner.Write(string.Format(formatProvider, "{0}{1}", args), LogLevel.Warn);
        }

        public void Warn(string message, params object[] args)
        {
            this._inner.Write(this.prefix + this.InvokeStringFormat(CultureInfo.InvariantCulture, message, args), LogLevel.Warn);
        }

        public void Warn<TArgument>(string message, TArgument argument)
        {
            object[] args = new object[] { argument };
            this._inner.Write(this.prefix + string.Format(CultureInfo.InvariantCulture, message, args), LogLevel.Warn);
        }

        public void Warn(IFormatProvider formatProvider, string message, params object[] args)
        {
            this._inner.Write(this.prefix + this.InvokeStringFormat(formatProvider, message, args), LogLevel.Warn);
        }

        public void Warn<TArgument>(IFormatProvider formatProvider, string message, TArgument argument)
        {
            object[] args = new object[] { argument };
            this._inner.Write(this.prefix + string.Format(formatProvider, message, args), LogLevel.Warn);
        }

        public void Warn<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2)
        {
            object[] args = new object[] { argument1, argument2 };
            this._inner.Write(this.prefix + string.Format(CultureInfo.InvariantCulture, message, args), LogLevel.Warn);
        }

        public void Warn<TArgument1, TArgument2>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2)
        {
            object[] args = new object[] { argument1, argument2 };
            this._inner.Write(this.prefix + string.Format(formatProvider, message, args), LogLevel.Warn);
        }

        public void Warn<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            object[] args = new object[] { argument1, argument2, argument3 };
            this._inner.Write(this.prefix + string.Format(CultureInfo.InvariantCulture, message, args), LogLevel.Warn);
        }

        public void Warn<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3)
        {
            object[] args = new object[] { argument1, argument2, argument3 };
            this._inner.Write(this.prefix + string.Format(formatProvider, message, args), LogLevel.Warn);
        }

        public void WarnException(string message, Exception exception)
        {
            this._inner.Write($"{this.prefix}{message}: {exception}", LogLevel.Warn);
        }

        public void Write([Localizable(false)] string message, LogLevel logLevel)
        {
            this._inner.Write(message, logLevel);
        }

        public LogLevel Level
        {
            get => 
                this._inner.Level;
            set => 
                (this._inner.Level = value);
        }
    }
}

